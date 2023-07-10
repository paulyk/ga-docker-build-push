using Microsoft.Extensions.Logging;
using SKD.KitStatusFeed;

namespace SKD.Service;

public class PartnerStatusService {
    private SkdContext _context;
    private KitStatusFeedService _kitStatusFeedService;
    private KitService _kitService;
    private ILogger<PartnerStatusService> _logger;

    public PartnerStatusService(SkdContext context, KitStatusFeedService kitStatusFeedService, KitService kitService, ILogger<PartnerStatusService> logger) {
        _context = context;
        _kitStatusFeedService = kitStatusFeedService;
        _kitService = kitService;
        _logger = logger;
    }

    #region UpdatePartneStatusAsync
    public async Task<MutationResult<UpdatePartnerStatusPayload>> UpdatePartneStatusAsync(
        UpdatePartnerStatusInput input
    ) {
        var result = new MutationResult<UpdatePartnerStatusPayload>() {
            Errors = await ValidateUpdatePartnerStatusInput(input),
            Payload = new UpdatePartnerStatusPayload() {
                KitNo = input.KitNo,
                Message = "",
                UpdatedStatuses = new()
            }
        };

        int startStatusSquence = await StartFromPartnerStatusCodeSequenceAsync(input.KitNo);
        if (startStatusSquence < 0) {
            result.Payload.Message = "Already up to date";
            return result;
        }

        var kitTimelineEventsToUpdated = await GetKitTimelineEventsToSendAsync(input.KitNo, startStatusSquence);
        foreach (var timelineEvent in kitTimelineEventsToUpdated) {
            var parnterStatusCode = (PartnerStatusCode)timelineEvent.EventType.Sequence;
            var pppInput = await GetProcessPartnerStatusInputAsync(input.KitNo, parnterStatusCode);
            var response = await _kitStatusFeedService.ProcessPartnerStatusAsync(pppInput);
            _logger.LogInformation($"UpdatePartneStatusAsync: {input.KitNo} {parnterStatusCode} {response.Data?.PartnerStatusLayoutData.AckStatus}");
            result.Payload.UpdatedStatuses.Add(parnterStatusCode);
        }

        return result;

        async Task<int> StartFromPartnerStatusCodeSequenceAsync(string kitNo) {
            var currentPartnerStatusCode = await _kitStatusFeedService.GetCurrentStatusCodeAsync(input.KitNo);
            var codes = Enum.GetValues<PartnerStatusCode>().Select(t => t.ToString()).ToList();
            if (!codes.Contains(currentPartnerStatusCode)) {
                return (int)PartnerStatusCode.FPCR;
            }

            var currentStatusSequence = (int)Enum.Parse<PartnerStatusCode>(currentPartnerStatusCode);
            return currentStatusSequence < codes.Count ? currentStatusSequence + 1 : -1;
        }

        async Task<List<KitTimelineEvent>> GetKitTimelineEventsToSendAsync(string kitNo, int fromSequence) {
            return await _context.KitTimelineEvents
                .Include(t => t.EventType)
                .Where(t => t.Kit.KitNo == kitNo)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Sequence >= fromSequence)
                .OrderBy(t => t.EventType.Sequence)
                .ToListAsync();
        }
    }

    public async Task<List<Error>> ValidateUpdatePartnerStatusInput(UpdatePartnerStatusInput input) {
        var errors = new List<Error>();

        var kit = await _context.Kits.FirstOrDefaultAsync(t => t.KitNo == input.KitNo);
        if (kit == null) {
            errors.Add(new Error("KitNumber", $"Kit not found for {input.KitNo}"));
        }

        return errors; ;
    }

    public async Task<KitProcessPartnerStatusRequest> GetProcessPartnerStatusInputAsync(
            string kitNo,
            PartnerStatusCode targetKitStatusCode
        ) {

        var kit = await _context.Kits
                .Include(k => k.Lot).ThenInclude(l => l.Plant)
                .Include(k => k.Dealer)
                .Include(k => k.TimelineEvents.Where(e => e.RemovedAt == null))
                    .ThenInclude(k => k.EventType)
                .Include(k => k.KitComponents.Where(kc => kc.RemovedAt == null)).ThenInclude(kc => kc.Component)
                .Include(k => k.KitComponents.Where(kc => kc.RemovedAt == null)).ThenInclude(kc => kc.ComponentSerials)
            .Where(k => k.KitNo == kitNo)
            .FirstAsync();

        var kitStatusEvent = kit.TimelineEvents
            .First(se => se.EventType.PartnerStatusCode == targetKitStatusCode);

        var planBuildStatusEvent = kit.TimelineEvents
            .FirstOrDefault(t => t.EventType.Code == KitTimelineCode.PLAN_BUILD);

        var engineSerial = kit.KitComponents
            .Where(kc => kc.Component.Code == "EN")
            .SelectMany(kc => kc.ComponentSerials)
            .Select(cs => cs.Serial1)
            .FirstOrDefault();

        // prepare input
        var partnerStatusCode = kitStatusEvent.EventType.PartnerStatusCode.ToString();
        var statusDate = kitStatusEvent.EventDate.ToString("yyyy-MM-dd HH:mm:ss");
        var buildDate = planBuildStatusEvent != null ? planBuildStatusEvent.EventDate.ToString("yyyy-MM-dd") : "";
        var formattedEngineSerial = (engineSerial ?? "").Substring(0, Math.Min(20, (engineSerial ?? "").Length));
        var vin = kit.VIN;
        var actualDealerCode = kit.Dealer?.Code ?? "";


        var targetEventSequence = (int)targetKitStatusCode;

        var planBuildSequence = (int)PartnerStatusCode.FPBP;
        var buildCcompletedSequence = (int)PartnerStatusCode.FPBC;
        var wholeSaleSequence = (int)PartnerStatusCode.FPWS;

        var input = new KitProcessPartnerStatusRequest {
            PlantGsdb = kit.Lot.Plant.Code,
            PartnerGsdb = kit.Lot.Plant.PartnerPlantCode,

            Status = partnerStatusCode,
            StatusDate = statusDate,
            KitNumber = kit.KitNo,
            BuildDate = targetEventSequence >= planBuildSequence ? buildDate : "",
            LotNumber = kit.Lot.LotNo,

            PhysicalVin = targetEventSequence > planBuildSequence ? vin : "",
            EngineSerialNumber = targetEventSequence >= buildCcompletedSequence ? formattedEngineSerial : "",
            ActualDealerCode = targetEventSequence == wholeSaleSequence ? actualDealerCode : "",
        };

        return input;
    }

    #endregion

    #region assing vin'

    /// <summary>
    /// Get VIN from KitStatusFeed and assign to kit and update Kit.VIN
    /// Does nothting if VIN already assigned
    /// </summary>
    /// <param name="kitNo"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<UpdateKitVinPayload>> UpdateKitVinAsync(UpdateKitVinInput input) {
        List<Error> errors = await ValidateUpdateKitVinInputAsync(input);
        var result = InitializeMutationResult(input, errors);

        if (result.Errors.Any()) {
            return result;
        }

        var kit = await FetchKitByNumberAsync(input.KitNo);

        // get VIN from KitStatusFeed
        var vinFromKitStatusFeed = await FetchVinFromKitStatusFeedAsync(input.KitNo, kit.Lot.Plant.PartnerPlantCode);

        // Assign VIN if it's not null and different from the current one
        if (vinFromKitStatusFeed != null && kit.VIN != vinFromKitStatusFeed) {
            await AssignNewVinToKitAsync(input.KitNo, vinFromKitStatusFeed);
        }

        // Update result's payload with the VIN
        result.Payload.VIN = vinFromKitStatusFeed;

        return result;
    }

    public async Task<List<Error>> ValidateUpdateKitVinInputAsync(UpdateKitVinInput input) {
        var errors = new List<Error>();

        if (string.IsNullOrEmpty(input.KitNo) || input.KitNo.Length != 17) {
            errors.Add(new Error("KitNumber", "KitNumber is required and must be 17 characters"));
            return errors;
        }

        var existingKit = await _context.Kits
            .Include(k => k.Lot).ThenInclude(l => l.Plant)
            .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);

        if (existingKit == null) {
            errors.Add(new Error("KitNumber", $"Kit not found for {input.KitNo}"));
        }

        return errors;
    }

    private async Task<Kit> FetchKitByNumberAsync(string kitNo) {
        return await _context.Kits
            .Include(t => t.Lot).ThenInclude(t => t.Plant)
            .FirstAsync(t => t.KitNo == kitNo);
    }

    private async Task<string> FetchVinFromKitStatusFeedAsync(string kitNo, string partnerGsdb) {
        var vinFetchResult = await _kitStatusFeedService.GetPvinAsync(new KitPVinRequest {
            KitNumber = kitNo,
            PartnerGsdb = partnerGsdb
        });

        return vinFetchResult.Data.PvinFeedLayoutData.physicalVin;
    }

    private async Task AssignNewVinToKitAsync(string kitNo, string vin) {
        var setVinResult = await _kitService.UpdateKitVinAsync(new KitVinInput {
            KitNo = kitNo,
            VIN = vin
        });
    }

    private SKD.Service.MutationResult<UpdateKitVinPayload> InitializeMutationResult(UpdateKitVinInput input, List<Error> errors) {
        return new SKD.Service.MutationResult<UpdateKitVinPayload> {
            Payload = new UpdateKitVinPayload {
                KitNo = input.KitNo,
            },
            Errors = errors
        };
    }
    #endregion

    #region GetCurrentStatusAsync, GetPvinAsync
    public async Task<KitCurrentStatusResponse> GetKitCurrentStatusAsync(KitCurrentStatusRequest input) {
        var result = await _kitStatusFeedService.GetCurrentStatusAsync(input);
        return result.Data;
    }

    public async Task<PvinFeedLayoutData> GetPvinAsync(string kitNo) {
        // get partnergsdb from kitNo
        var kit = await _context.Kits
            .Include(k => k.Lot).ThenInclude(l => l.Plant)
            .FirstAsync(k => k.KitNo == kitNo);

        var result = await _kitStatusFeedService.GetPvinAsync(new KitPVinRequest {
            KitNumber = kit.KitNo,
            PartnerGsdb = kit.Lot.Plant.PartnerPlantCode
        });

        if (result.Error != null) {
            var errorMessage = "Error getting pvin";
            throw new Exception(errorMessage);
        }

        return result.Data.PvinFeedLayoutData;
    }

    #endregion

    #region SyncKitToPartnerStatusAsync
    public async Task<MutationResult<UpdatePartnerStatusPayload>> SyncKitToPartnerStatusAsync(
        UpdatePartnerStatusInput input
    ) {
        MutationResult<UpdatePartnerStatusPayload> result = new() {
            Errors = await ValidateSyncKitToPartnerStatusAsync(input.KitNo),
            Payload = new()
        };
        if (result.Errors.Any()) {
            return result;
        };

        var kit = _context.Kits
            .Include(k => k.TimelineEvents.Where(e => e.RemovedAt == null))
            .ThenInclude(t => t.EventType)
            .First(k => k.KitNo == input.KitNo);

        var currentStatusCode = Enum.Parse<PartnerStatusCode>(await _kitStatusFeedService.GetCurrentStatusCodeAsync(input.KitNo));
        var partnerStatusSequence = (int)currentStatusCode;

        _logger.LogInformation($"SyncKitToPartnerStatusAsync: {input.KitNo} {currentStatusCode} {partnerStatusSequence}");

        var orderedKitStatusEvents = kit.TimelineEvents
            .OrderBy(se => se.EventType.Sequence)
            .Where(se => se.EventType.Sequence <= partnerStatusSequence)
            .ToList();

        foreach (var kitStatusEvent in orderedKitStatusEvents) {
            if (kitStatusEvent.EventType.Sequence <= partnerStatusSequence) {
                // update local
                if (kitStatusEvent.PartnerStatusUpdatedAt == null) {
                    var setResult = await _kitService.SetPartnerUpdatedAtAsync(new SetPartnerUpdatedInput(
                         kit.KitNo,
                         kitStatusEvent.EventType.PartnerStatusCode
                     ));
                    if (setResult.Errors.Any()) {
                        result.Errors.AddRange(setResult.Errors);
                        return result;
                    }
                    result.Payload.UpdatedStatuses.Add(kitStatusEvent.EventType.PartnerStatusCode);
                }
            }
        }

        return result;
    }

    public async Task<List<Error>> ValidateSyncKitToPartnerStatusAsync(string kitNo) {
        var errors = new List<Error>();

        var kit = await _context.Kits
            .Include(k => k.TimelineEvents.Where(e => e.RemovedAt == null)).ThenInclude(t => t.EventType)
            .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        if (kit == null) {
            errors.Add(new Error("KitNumber", $"Kit not found for {kitNo}"));
        }

        return errors;
    }

    #endregion

}
#nullable enable

using System.Security.Cryptography.Xml;

namespace SKD.Service;

public class KitService {

    private readonly SkdContext context;
    private readonly DateTime currentDate;
    // private readonly int PlanBuildLeadTimeDays;

    public KitService(SkdContext ctx, DateTime currentDate) {
        this.context = ctx;
        this.currentDate = currentDate;
    }

    #region create kit timeline event

    /// <summary>
    /// Create a Build Start timeline event for a kit
    /// </summary>
    /// <param name="kitNo"></param>
    /// <returns></returns>
    public async Task<MutationResult<KitTimelineEvent>> CreateBuildStartEventAsync(string kitNo) {
        // get event date from first component serial date for 
        var componentSerialScanDate = context.ComponentSerials
            .Where(cs => cs.RemovedAt == null)
            .Where(cs => cs.KitComponent.Kit.KitNo == kitNo)
            .Select(cs => cs.CreatedAt).FirstOrDefault();

        return await CreateKitTimelineEventAsync(new KitTimelineEventInput {
            KitNo = kitNo,
            EventCode = KitTimelineCode.BUILD_START,
            EventDate = componentSerialScanDate,
            EventNote = ""
        });
    }

    /// <summary>
    /// Create a timeline event for a kit
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<MutationResult<KitTimelineEvent>> CreateKitTimelineEventAsync(KitTimelineEventInput input) {
        MutationResult<KitTimelineEvent> result = new();
        result.Errors = await ValidateCreateKitTimelineEventAsync(input);
        if (result.Errors.Count > 0) {
            return result;
        }

        var kit = await context.Kits
            .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
            .FirstAsync(t => t.KitNo == input.KitNo);

        // mark other timeline events of the same type as removed for this kit
        kit.TimelineEvents
            .Where(t => t.EventType.Code == input.EventCode)
            .ToList().ForEach(timeline => {
                if (timeline.RemovedAt == null) {
                    timeline.RemovedAt = DateTime.UtcNow;
                }
            });

        // create timeline event and add to kit
        var newTimelineEvent = new KitTimelineEvent {
            EventType = await context.KitTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == input.EventCode),
            EventDate = input.EventDate,
            EventNote = input.EventNote
        };
        
        // Set kit dealer code if provided
        if (!String.IsNullOrWhiteSpace(input.DealerCode)) {
            kit.Dealer = await context.Dealers.FirstOrDefaultAsync(t => t.Code == input.DealerCode);
        }

        kit.TimelineEvents.Add(newTimelineEvent);

        // save
        result.Payload = newTimelineEvent;
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<List<Error>> ValidateCreateKitTimelineEventAsync(KitTimelineEventInput input) {
        var errors = new List<Error>();

        // kitNo
        var kit = await context.Kits.AsNoTracking()
            .Include(t => t.Lot)
            .Include(t => t.TimelineEvents.Where(t => t.RemovedAt == null))
                .ThenInclude(t => t.EventType)
            .Include(t => t.Dealer)
            .Include(t => t.KitComponents.Where(t => t.RemovedAt == null))
                .ThenInclude(t => t.ComponentSerials.Where(t => t.RemovedAt == null))
            .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);

        // kit not found
        if (kit == null) {
            errors.Add(new Error("KitNo", $"kit not found for kitNo: {input.KitNo}"));
            return errors;
        }

        // duplicate timeline event
        var duplicate = kit.TimelineEvents
            .OrderByDescending(t => t.EventType.Sequence)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == input.EventCode)
            .Where(t => t.EventDate == input.EventDate)
            .Where(t => t.EventNote == input.EventNote)
            .FirstOrDefault();

        if (duplicate != null) {
            var dateStr = input.EventDate.ToShortDateString();
            errors.Add(new Error("", $"duplicate kit timeline event: {input.EventCode} {dateStr} "));
            return errors;
        }

        // setup
        var kitTimelineEventTypes = await context.KitTimelineEventTypes.AsNoTracking()
            .Where(t => t.RemovedAt == null).ToListAsync();
        var inputEventType = kitTimelineEventTypes.First(t => t.Code == input.EventCode);
        var appSettings = await ApplicationSetting.GetKnownAppSettings(context);

        // Missing prior timeline event
        var priorEventType = kitTimelineEventTypes.FirstOrDefault(t => t.Sequence == inputEventType.Sequence - 1);
        if (priorEventType != null) {
            var priorTimelineEvent = kit.TimelineEvents.FirstOrDefault(t => t.EventType.Code == priorEventType.Code);
            if (priorTimelineEvent == null) {
                errors.Add(new Error("", $"Missing timeline event {priorEventType.Description}"));
                return errors;
            }
        }

        // Cannot set if the next timeline event in sequence already set
        var nextEventType = kitTimelineEventTypes.FirstOrDefault(t => t.Sequence == inputEventType.Sequence + 1);
        if (nextEventType != null) {
            var nextTimelineEvent = kit.TimelineEvents.FirstOrDefault(t => t.EventType.Code == nextEventType.Code);
            if (nextTimelineEvent != null) {
                errors.Add(new Error("", $"{nextEventType.Description} already set, cannot set {inputEventType.Description}"));
                return errors;
            }
        }

        // CUSTOM_RECEIVED 
        if (input.EventCode == KitTimelineCode.CUSTOM_RECEIVED) {
            if (currentDate <= input.EventDate) {
                errors.Add(new Error("", $"Custom received date must precede current date by {appSettings.PlanBuildLeadTimeDays} days"));
                return errors;
            }
        }

        // PLAN_BUILD 
        if (input.EventCode == KitTimelineCode.PLAN_BUILD) {
            var custom_receive_date = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == KitTimelineCode.CUSTOM_RECEIVED)
                .Select(t => t.EventDate).First();

            var custom_receive_plus_lead_time_date = custom_receive_date.AddDays(appSettings.PlanBuildLeadTimeDays);

            var plan_build_date = input.EventDate;
            if (custom_receive_plus_lead_time_date > plan_build_date) {
                errors.Add(new Error("", $"plan build must greater custom receive by {appSettings.PlanBuildLeadTimeDays} days"));
                return errors;
            }
        }

        // BUILD_START must have at least one component seraial scan
        if (input.EventCode == KitTimelineCode.BUILD_START) {
            var anyComponentSerialScan = kit.KitComponents
                .Where(t => t.RemovedAt == null)
                .SelectMany(t => t.ComponentSerials)
                .Where(t => t.RemovedAt == null)
                .Any();

            if (!anyComponentSerialScan) {
                errors.Add(new Error("", $"Build Start status requires at least one component serial scan"));
                return errors;
            }
        }

        // WHOLESALE kit must be associated with dealer to proceed
        if (input.EventCode == KitTimelineCode.WHOLE_SALE) {
            if (String.IsNullOrWhiteSpace(input.DealerCode)) {
                if (kit.Dealer == null) {
                    errors.Add(new Error("", $"Kit must be associated with dealer {kit.KitNo}"));
                    return errors;
                }
            } else {
                var dealer = await context.Dealers.AsNoTracking().FirstOrDefaultAsync(t => t.Code == input.DealerCode);
                if (dealer == null) {
                    errors.Add(new Error("", $"Dealer not found for code {input.DealerCode}"));
                    return errors;
                }
            }
        }

        // VIN Required for events sequence after PLAN BUILD
        var planBuildType = kitTimelineEventTypes.First(t => t.Code == KitTimelineCode.PLAN_BUILD);
        if (inputEventType.Sequence > planBuildType.Sequence && String.IsNullOrWhiteSpace(kit.VIN)) {
            errors.Add(new Error("", $"Kit does not have VIN, cannot save {input.EventCode} event"));
            return errors;
        }

        // Event date cannot be in the future for events for VERIFY_VIN onwards
        var verifyVinType = kitTimelineEventTypes.First(t => t.Code == KitTimelineCode.BUILD_START);
        if (inputEventType.Sequence > verifyVinType.Sequence) {
            if (input.EventDate.Date > currentDate.Date) {
                errors.Add(new Error("", $"Date cannot be in the future"));
                return errors;
            }
        }

        /* Remove feature: 2022-02-11, problem with ship file imports from Ford
        // shipment missing
        var hasAssociatedShipment = await context.ShipmentLots.AnyAsync(t => t.Lot.LotNo == kit.Lot.LotNo);
        if (!hasAssociatedShipment) {
            errors.Add(new Error("", $"shipment missing for lot: {kit.Lot.LotNo}"));
            return errors;
        }
        */

        return errors;
    }

    #endregion

    #region create lot timeline event
    public async Task<MutationResult<Lot>> CreateLotTimelineEventAsync(LotTimelineEventInput input) {
        MutationResult<Lot> result = new();
        result.Errors = await ValidateCreateLotTimelineEventAsync(input);
        if (result.Errors.Count > 0) {
            return result;
        }

        var kitLot = await context.Lots
            .Include(t => t.Kits)
                .ThenInclude(t => t.TimelineEvents)
                .ThenInclude(t => t.EventType)
            .FirstAsync(t => t.LotNo == input.LotNo);

        foreach (var kit in kitLot.Kits) {

            // mark other timeline events of the same type as removed for this kit
            kit.TimelineEvents
                .Where(t => t.EventType.Code == input.EventCode)
                .ToList().ForEach(timelineEvent => {
                    if (timelineEvent.RemovedAt == null) {
                        timelineEvent.RemovedAt = DateTime.UtcNow;
                    }
                });

            // create timeline event and add to kit
            var newTimelineEvent = new KitTimelineEvent {
                EventType = await context.KitTimelineEventTypes.FirstOrDefaultAsync(t => t.Code == input.EventCode),
                EventDate = input.EventDate,
                EventNote = input.EventNote
            };

            kit.TimelineEvents.Add(newTimelineEvent);

        }

        // // save
        result.Payload = kitLot;
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<List<Error>> ValidateCreateLotTimelineEventAsync(LotTimelineEventInput input) {
        var errors = new List<Error>();

        var lot = await context.Lots.AsNoTracking()
            .Include(t => t.Kits).ThenInclude(t => t.TimelineEvents).ThenInclude(t => t.EventType)
            .FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

        // kit lot 
        if (lot == null) {
            errors.Add(new Error("VIN", $"lot not found for lotNo: {input.LotNo}"));
            return errors;
        }

        // duplicate 
        var duplicateTimelineEventsFound = lot.Kits.SelectMany(t => t.TimelineEvents)
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == input.EventCode)
            .Where(t => t.EventDate == input.EventDate)
            .ToList();

        if (duplicateTimelineEventsFound.Count > 0) {
            var dateStr = input.EventDate.ToShortDateString();
            errors.Add(new Error("", $"duplicate kit timeline event: {input.LotNo}, Type: {input.EventCode} Date: {dateStr} "));
            return errors;
        }

        // CUSTOM_RECEIVED 
        if (input.EventCode == KitTimelineCode.CUSTOM_RECEIVED) {
            if (input.EventDate.Date >= currentDate) {
                errors.Add(new Error("", $"custom received date must be before current date"));
                return errors;
            }
            if (input.EventDate.Date < currentDate.AddMonths(-6)) {
                errors.Add(new Error("", $"custom received cannot be more than 6 months ago"));
                return errors;
            }
        }

        /* Remove feature: 2022-02-11, problem with ship file imports from Ford
        // shipment missing        
        var hasAssociatedShipment = await context.ShipmentLots.AnyAsync(t => t.Lot.LotNo == lot.LotNo);
        if (!hasAssociatedShipment) {
            errors.Add(new Error("", $"shipment missing for lot: {lot.LotNo}"));
            return errors;
        }
        */

        return errors;
    }

    #endregion

    #region change kit component production station
    public async Task<MutationResult<KitComponent>> ChangeKitComponentProductionStationAsync(KitComponentProductionStationInput input) {
        MutationResult<KitComponent> result = new();
        result.Errors = await ValidateChangeKitComponentStationInputAsync(input);
        if (result.Errors.Count > 0) {
            return result;
        }

        var kitComponent = await context.KitComponents.FirstAsync(t => t.Id == input.KitComponentId);
        var productionStation = await context.ProductionStations.FirstAsync(t => t.Code == input.ProductionStationCode);

        kitComponent.ProductionStation = productionStation;
        // // save
        await context.SaveChangesAsync();
        result.Payload = kitComponent;
        return result;
    }

    public async Task<List<Error>> ValidateChangeKitComponentStationInputAsync(KitComponentProductionStationInput input) {
        var errors = new List<Error>();

        var kitComponent = await context.KitComponents.FirstOrDefaultAsync(t => t.Id == input.KitComponentId);
        if (kitComponent == null) {
            errors.Add(new Error("", $"kit component not found for {input.KitComponentId}"));
            return errors;
        }

        var productionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == input.ProductionStationCode);
        if (productionStation == null) {
            errors.Add(new Error("", $"production station not found {input.ProductionStationCode}"));
            return errors;
        }

        if (kitComponent.ProductionStationId == productionStation.Id) {
            errors.Add(new Error("", $"production station is already set to {input.ProductionStationCode}"));
            return errors;
        }

        return errors;
    }

    #endregion

    #region set kit vin

    /// <summary>
    /// Update a Kit's VIN
    /// Note: A Kit's VIN can change, but a history is kept of all VINs associated with a Kit
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<MutationResult<Kit>> UpdateKitVinAsync(KitVinInput input) {

        MutationResult<Kit> result = new();
        result.Errors = await ValidateUpdateKitVinAsync(input);
        if (result.Errors.Count > 0) {
            return result;
        }

        var kit = await context.Kits
            .Include(t => t.Lot)
            .Include(t => t.TimelineEvents)
            .Include(t => t.KitVins)
            .FirstAsync(t => t.KitNo == input.KitNo);

        kit.KitVins.Where(t => t.RemovedAt == null).ToList().ForEach(kitVin => {
            kitVin.RemovedAt = DateTime.UtcNow;
        });

        kit.KitVins.Add(new KitVin {
            VIN = input.VIN,
            CreatedAt = DateTime.UtcNow,
        });
        // set new kit.VIN
        kit.VIN = input.VIN;

        // save 
        await context.SaveChangesAsync();

        result.Payload = kit;
        return result;

    }

    // validate set kit vin
    // return Task<List<Error>> 
    // Kit not null, kit current vin is different
    // Kit.TimelineEvents latest event is not BUILD_START or greater
    public async Task<List<Error>> ValidateUpdateKitVinAsync(KitVinInput input) {
        var errors = new List<Error>();
        var kit = await context.Kits.AsNoTracking()
            .Include(t => t.Lot)
            .Include(t => t.TimelineEvents.Where(te => te.RemovedAt == null)).ThenInclude(t => t.EventType)
            .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);

        if (kit == null) {
            errors.Add(new Error("", $"kit not found for {input.KitNo}"));
            return errors;
        }

        // VIN cannot have spaces in it
        if (input.VIN.Contains(" ")) {
            errors.Add(new Error("", $"vin cannot contain spaces"));
            return errors;
        }
        // VIN must be EntityFieldLen.VIN length
        if (input.VIN.Length != EntityFieldLen.VIN) {
            errors.Add(new Error("", $"vin must be {EntityFieldLen.VIN} characters"));
            return errors;
        }

        if (kit.VIN == input.VIN) {
            errors.Add(new Error("", $"kit vin is already set to {input.VIN}"));
            return errors;
        }

        // VIN in use by another kit
        var otherKit = await context.Kits.AsNoTracking().FirstOrDefaultAsync(k => k.VIN == input.VIN);
        if (otherKit != null) {
            errors.Add(new Error("", $"VIN is already in use by kit {otherKit.KitNo}"));
            return errors;
        }

        // latest event must be PLAN_BUILD 
        var latestEvent = kit.TimelineEvents.OrderByDescending(t => t.EventType.Sequence).FirstOrDefault();
        if (latestEvent != null) {
            if (latestEvent.EventType.Code != KitTimelineCode.PLAN_BUILD) {
                errors.Add(new Error("", $"kit latest event is not PLAN_BUILD"));
                return errors;
            }
        }

        return errors;
    }

    #endregion

    #region set kit status event PartnerUpdatedAt

    public async Task<MutationResult<Kit>> SetPartnerUpdatedAtAsync(SetPartnerUpdatedInput input) {
        var result = new MutationResult<Kit>() {
            Errors = await ValidateSetPartnerUpdatedAtAsync(input)
        };
        if (result.Errors.Count > 0) {
            return result;
        }

        var kit = await context.Kits
            .Include(t => t.TimelineEvents.Where(te => te.RemovedAt == null)).ThenInclude(t => t.EventType)
            .FirstAsync(t => t.KitNo == input.kitNo);

        var kitStatusEvent = kit.TimelineEvents.First(t => t.EventType.PartnerStatusCode == input.kitStatusCode);
        kitStatusEvent.PartnerStatusUpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return result;
    }

    public async Task<List<Error>> ValidateSetPartnerUpdatedAtAsync(SetPartnerUpdatedInput input) {
        var errors = new List<Error>();
        var kit = await context.Kits
            .Include(t => t.TimelineEvents.Where(te => te.RemovedAt == null)).ThenInclude(t => t.EventType)
            .FirstOrDefaultAsync(t => t.KitNo == input.kitNo);

        if (kit == null) {
            errors.Add(new Error("", $"kit not found for {input.kitNo}"));
            return errors;
        }

        // has kit.StatusEvents has one where   kitStatusCode
        var hasKitStatusCode = kit.TimelineEvents.Any(t => t.EventType.PartnerStatusCode == input.kitStatusCode);
        if (!hasKitStatusCode) {
            errors.Add(new Error("", $"kit does not have status event for {input.kitStatusCode}"));
            return errors;
        }

        return errors;
    }

    #endregion

    #region auto generate build-start events

    /// <summary>
    /// Auto generate BUILD_START event for kits that have a PLAN_BUILD and any component serials
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<MutationResult<GenerateBuildStartEventsPayload>> GenerateBuildStartEventsAsync(GenerateBuildStartEventsInput input) {
        var result = new MutationResult<GenerateBuildStartEventsPayload>() {
            Errors = await ValidateGenerateBuildStartEventsAsync(input),
            Payload = new GenerateBuildStartEventsPayload {
                PlantCode = input.PlantCode
            }
        };

        if (result.Errors.Count > 0) {
            return result;
        }
        var kits = await GetBuildStartPendingKits();

        var buildStartEventType = await context.KitTimelineEventTypes.FirstAsync(t => t.Code == KitTimelineCode.BUILD_START);

        foreach (var kit in kits) {
            var firstComponentSerial = kit.KitComponents.SelectMany(kc => kc.ComponentSerials).OrderBy(cs => cs.CreatedAt).First();

            kit.TimelineEvents.Add(new KitTimelineEvent() {
                EventType = buildStartEventType,
                CreatedAt = DateTime.UtcNow,
                EventDate = firstComponentSerial.CreatedAt
            });

            result.Payload.KitNos.Add(kit.KitNo);
        }

        // save
        await context.SaveChangesAsync();

        return result;

        async Task<List<Kit>> GetBuildStartPendingKits() => await context.Kits
            .Include(k => k.Lot).ThenInclude(k => k.Pcv)
            .Include(k => k.TimelineEvents.Where(te => te.RemovedAt == null)).ThenInclude(k => k.EventType)
            .Include(k => k.KitComponents).ThenInclude(k => k.ComponentSerials.Where(cs => cs.RemovedAt == null))
            .Where(k => k.Lot.Plant.Code == input.PlantCode)
            .Where(k =>
                k.TimelineEvents
                .Where(e => e.RemovedAt == null)
                .OrderByDescending(e => e.EventType.Sequence)
                .Select(e => e.EventType.Code)
                .FirstOrDefault() == KitTimelineCode.PLAN_BUILD
            )
            .Where(k =>
                k.KitComponents
                    .Where(kc => kc.RemovedAt == null)
                    .SelectMany(kc => kc.ComponentSerials)
                        .Where(c => c.RemovedAt == null)
                        .Any()
            ).ToListAsync();

    }
    public async Task<List<Error>> ValidateGenerateBuildStartEventsAsync(GenerateBuildStartEventsInput input) {
        var errors = new List<Error>();

        var plant = await context.Plants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == input.PlantCode);

        if (plant == null) {
            errors.Add(new Error("", $"plant not found for {input.PlantCode}"));
            return errors;
        }

        return errors;
    }


    #endregion

    #region queries 


    /// <summary>
    /// Gets kits where status is BUILD_START status should be created
    /// Current status is PLAN_BUILD and has component serial input
    /// </summary>
    /// <param name="plantCode"></param>
    /// <returns></returns>
    public async Task<List<KitPayload>> GetBuildStartPendingKits(
        string plantCode
    ) {

        var result = await context.Kits.AsNoTracking()
            .Include(k => k.Lot).ThenInclude(k => k.Pcv)
            .Include(k => k.TimelineEvents).ThenInclude(k => k.EventType)
            .Where(k => k.Lot.Plant.Code == plantCode)
            // Where latest event is PLAN_BUILD
            .Where(k =>
                k.TimelineEvents
                .Where(e => e.RemovedAt == null)
                .OrderByDescending(e => e.EventType.Sequence)
                .Select(e => e.EventType.Code)
                .FirstOrDefault() == KitTimelineCode.PLAN_BUILD
            )
            // where has a component serial input
            .Where(k =>
                k.KitComponents
                    .Where(kc => kc.RemovedAt == null)
                    .SelectMany(kc => kc.ComponentSerials)
                        .Where(c => c.RemovedAt == null)
                        .Any()
                )
            .Select(k => KitPayload.Create(k))
            .ToListAsync();

        return result;
    }



    #endregion

}

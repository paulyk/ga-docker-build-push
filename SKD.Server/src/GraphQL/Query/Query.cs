#pragma warning disable
using HotChocolate.Execution;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Localization;
using SDK.Service;

namespace SKD.Server;

public class Query {

    IConfiguration Configuration { get; }

    public Query(IConfiguration configuration) {
        Configuration = configuration;
    }

    public AppSettings GetServerConfigSettings(
        [Service] AppSettings appSettings
    ) => appSettings;

    public string Info() => "RMA SDK Server";

    public async Task<ShipmentOverviewDTO?> GetShipmentOverview(
        [Service] ShipmentService service,
        Guid shipmentId
    ) => await service.GetShipmentOverview(shipmentId);

    public async Task<List<HandlingUnitOverview>> GetHandlingUnitOverviews(
        [Service] HandlingUnitService service,
        Guid shipmentId
    ) => await service.GetHandlingUnitOverviews(shipmentId);

    public async Task<HandlingUnitInfoPayload?> GetHandlingUnitInfo(
        [Service] HandlingUnitService service,
        string huCode
    ) => await service.GetHandlingUnitInfo(huCode);

    public async Task<Kit?> GetKitById(SkdContext context, Guid id) {
        var result = await context.Kits.AsNoTracking()
                .Include(t => t.Lot)
                .Include(t => t.Lot).ThenInclude(t => t.Pcv)
                .Include(t => t.KitComponents).ThenInclude(t => t.Component)
                .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
                .Include(t => t.KitComponents).ThenInclude(t => t.ComponentSerials)
                .Include(t => t.TimelineEvents)
                .Include(t => t.KitVins)
                .FirstOrDefaultAsync(t => t.Id == id);

        return result;
    }

    public async Task<Kit?> GetKitByKitNo(SkdContext context, string kitNo) {
        var result = await context.Kits.AsNoTracking()
                .Include(t => t.Dealer)
                .Include(t => t.Lot).ThenInclude(t => t.Pcv).ThenInclude(t => t.PcvComponents).ThenInclude(t => t.Component)
                .Include(t => t.KitComponents).ThenInclude(t => t.Component)
                .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
                .Include(t => t.KitComponents)
                    .ThenInclude(t => t.ComponentSerials)
                    .ThenInclude(t => t.DcwsResponses)
                .Include(t => t.Lot).ThenInclude(t => t.Pcv)
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .Include(t => t.KitVins)
                .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        return result;
    }
    public async Task<KitStatusDTO?> GetKitTimeline(
        SkdContext context,
        string kitNo
    ) {
        var kit = await context.Kits.AsNoTracking()
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .Include(t => t.Lot)
                .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        if (kit == null) {
            return (KitStatusDTO?)null;
        }

        var timelineEventTypes = await context.KitTimelineEventTypes.AsNoTracking()
            .OrderBy(t => t.Sequence)
            .Where(t => t.RemovedAt == null).ToListAsync();

        var dto = new KitStatusDTO {
            VIN = kit.VIN,
            KitNo = kit.KitNo,
            LotNo = kit.Lot.LotNo,
            TimelineItems = timelineEventTypes.Select(evtType => {
                var timelineEvent = kit.TimelineEvents
                    .Where(vt => vt.EventType.Code == evtType.Code)
                    .Where(vt => vt.RemovedAt == null)
                    .FirstOrDefault();

                return timelineEvent != null
                    ? new StatusEventDTO {
                        EventDate = timelineEvent.EventDate,
                        EventNote = timelineEvent.EventNote,
                        EventType = timelineEvent.EventType.Code,
                        PartnerStatusUpdatedAt = timelineEvent.PartnerStatusUpdatedAt,
                        CreatedAt = timelineEvent.CreatedAt,
                        Sequence = evtType.Sequence
                    }
                    : new StatusEventDTO {
                        EventType = evtType.Code,
                        Sequence = evtType.Sequence
                    };
            }).ToList()
        };

        return dto;
    }

    public async Task<Lot?> GetLotByLotNo(SkdContext context, string lotNo) =>
            await context.Lots.AsNoTracking()
                    .Include(t => t.Pcv)
                    .Include(t => t.Kits)
                            .ThenInclude(t => t.TimelineEvents)
                            .ThenInclude(t => t.EventType)
                    .FirstOrDefaultAsync(t => t.LotNo == lotNo);

    public async Task<LotOverviewDTO?> GetLotOverview(SkdContext context, string lotNo) {
        var lot = await context.Lots.OrderBy(t => t.LotNo).AsNoTracking()
            .Include(t => t.Kits)
                .ThenInclude(t => t.TimelineEvents)
                .ThenInclude(t => t.EventType)
            .Include(t => t.Pcv)
            .Include(t => t.Plant)
            .Include(t => t.Bom)
            .Include(t => t.ShipmentLots)
                .ThenInclude(t => t.Shipment)
            .FirstOrDefaultAsync(t => t.LotNo == lotNo);

        if (lot == null) {
            return (LotOverviewDTO?)null;
        }

        var kit = lot.Kits.FirstOrDefault();
        var timelineEvents = lot.Kits.SelectMany(t => t.TimelineEvents);

        KitTimelineEvent? customReceivedEvent = null;
        if (kit != null) {
            customReceivedEvent = kit.TimelineEvents
                .OrderByDescending(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == KitTimelineCode.CUSTOM_RECEIVED);
        }

        return new LotOverviewDTO {
            Id = lot.Id,
            LotNo = lot.LotNo,
            Note = lot.Note,
            BomId = lot.Bom.Id,
            BomSequence = lot.Bom.Sequence,
            ShipmentId = lot.ShipmentLots.Select(x => x.Shipment.Id).FirstOrDefault(),
            ShipmentSequence = lot.ShipmentLots.Select(x => x.Shipment.Sequence).FirstOrDefault(),
            PlantCode = lot.Plant.Code,
            PcvId = lot.Pcv.Id,
            PcvCode = lot.Pcv.Code,
            PcvDescription = lot.Pcv.Description,
            CreatedAt = lot.CreatedAt,
            CustomReceived = customReceivedEvent != null
                ? new StatusEventDTO {
                    EventType = KitTimelineCode.CUSTOM_RECEIVED,
                    EventDate = customReceivedEvent != null ? customReceivedEvent.EventDate : (DateTime?)null,
                    EventNote = customReceivedEvent?.EventNote,
                    CreatedAt = customReceivedEvent != null ? customReceivedEvent.CreatedAt : (DateTime?)null,
                    RemovedAt = customReceivedEvent != null ? customReceivedEvent.RemovedAt : (DateTime?)null
                }
                : null
        };
    }

    public async Task<List<LotPartDTO>> GetLotPartsByBom(
        [Service] QueryService service, Guid bomId) {
        return await service.GetLotPartsByBom(bomId);
    }

    public async Task<List<LotPartDTO>> GetLotPartsByShipment(
        [Service] QueryService service,
    Guid shipmentId) => await service.GetLotPartsByShipment(shipmentId);

    public async Task<List<Kit>> GetKitsByLot(SkdContext context, string lotNo) =>
             await context.Kits.OrderBy(t => t.Lot).AsNoTracking()
                .Where(t => t.Lot.LotNo == lotNo)
                    .Include(t => t.Lot).ThenInclude(t => t.Pcv)
                    .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .ToListAsync();

    public async Task<Component?> GetComponentById(SkdContext context, Guid id) =>
             await context.Components.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    [Obsolete("no longer used", error: true)]
    public async Task<KitComponent?> GetVehicleComponentByVinAndComponent(SkdContext context, string vin, string componentCode) =>
             await context.KitComponents.AsNoTracking()
                    .Include(t => t.Kit)
                    .Include(t => t.Component)
                    .Include(t => t.ComponentSerials)
                    .FirstOrDefaultAsync(t => t.Kit.VIN == vin && t.Component.Code == componentCode);


    [Obsolete("no longer used", error: true)]
    public async Task<ComponentSerial?> GetComponentScanById(SkdContext context, Guid id) =>
            await context.ComponentSerials.AsNoTracking()
                    .Include(t => t.KitComponent).ThenInclude(t => t.Kit)
                    .FirstOrDefaultAsync(t => t.Id == id);

    [Obsolete("no longer used", error: true)]
    public async Task<ComponentSerial?> GetExistingComponentScan(SkdContext context, Guid vehicleComponentId) =>
           await context.ComponentSerials.AsNoTracking()
                    .Include(t => t.KitComponent)
                    .FirstOrDefaultAsync(t => t.KitComponentId == vehicleComponentId && t.RemovedAt == null);


    [UsePaging(MaxPageSize = 10000)]
    [UseSorting]
    public IQueryable<KitListItemDTO> GetKitList(
        SkdContext context,
        string plantCode
    ) => context.Kits.AsNoTracking()
            .Where(t => t.Lot.Plant.Code == plantCode)
            .Select(t => new KitListItemDTO {
                Id = t.Id,
                LotNo = t.Lot.LotNo,
                KitNo = t.KitNo,
                VIN = t.VIN,
                ModelCode = t.Lot.Pcv.Code,
                ModelName = t.Lot.Pcv.Description,
                LastTimelineEvent = t.TimelineEvents
                    .Where(t => t.RemovedAt == null)
                    .OrderByDescending(t => t.EventType.Sequence)
                    .Select(t => t.EventType.Description)
                    .FirstOrDefault(),
                LastTimelineEventDate = t.TimelineEvents
                    .Where(t => t.RemovedAt == null)
                    .OrderByDescending(t => t.EventType.Sequence)
                    .Select(t => t.EventDate)
                    .FirstOrDefault(),
                ComponentCount = t.KitComponents
                    .Where(t => t.RemovedAt == null)
                    .Count(),
                ScannedComponentCount = t.KitComponents
                    .Where(t => t.RemovedAt == null)
                    .Where(t => t.ComponentSerials.Any(t => t.RemovedAt == null))
                    .Count(),
                VerifiedComponentCount = t.KitComponents
                    .Where(t => t.RemovedAt == null)
                    .Where(t => t.ComponentSerials.Any(u => u.RemovedAt == null && u.VerifiedAt != null))
                    .Count(),
                Imported = t.CreatedAt
            }).AsQueryable();


    [UsePaging(MaxPageSize = 10000)]
    [UseSorting]
    public IQueryable<BomListDTO> GetBomList(
        [Service] QueryService service,
        string plantCode
    ) => service.GetBomList(plantCode);

    public async Task<Bom?> GetBomById(SkdContext context, Guid id) =>
            await context.Boms.AsNoTracking()
                    .Include(t => t.Lots).ThenInclude(t => t.LotParts)
                    .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<BomOverviewDTO?> GetBomOverview([Service] BomService service, Guid id) =>
         await service.GetBomOverview(id);

    public async Task<List<LotListDTO>> GetLotListByBomId(SkdContext context, Guid id) =>
             await context.Lots.AsNoTracking()
                .Where(t => t.Bom.Id == id)
                .Select(t => new LotListDTO {
                    Id = t.Id,
                    PlantCode = t.Plant.Code,
                    LotNo = t.LotNo,
                    KitCount = t.Kits.Count(),
                    TimelineStatus = t.Kits
                        .SelectMany(t => t.TimelineEvents)
                        .OrderByDescending(t => t.CreatedAt)
                        .Where(t => t.RemovedAt == null)
                        .Select(t => (KitTimelineCode?)t.EventType.Code).FirstOrDefault(),
                    CreatedAt = t.CreatedAt
                }).ToListAsync();

    public async Task<List<PartQuantityDTO>> GetBomPartsQuantity(SkdContext context, Guid id) {
        var result = await context.LotParts
            .Where(t => t.Lot.Bom.Id == id)
            .GroupBy(t => new {
                PartNo = t.Part.PartNo,
                PartDesc = t.Part.PartDesc
            })
            .Select(g => new PartQuantityDTO {
                PartNo = g.Key.PartNo,
                PartDesc = g.Key.PartDesc,
                Quantity = g.Sum(u => u.BomQuantity)
            }).ToListAsync();

        return result;
    }

    public async Task<LotDTO?> GetLotInfo(
           [Service] LotPartService service,
           string lotNo
    ) => await service.GetLotInfo(lotNo);

    public async Task<LotPartDTO?> GetLotPartInfo(
           [Service] LotPartService service,
           string lotNo,
           string partNo
    ) => await service.GetLotPartInfo(lotNo, partNo);

    public async Task<List<LotPartReceivedDTO>> GetRecentLotPartsReceived(
        [Service] LotPartService service,
        int count = 100
    ) => await service.GetRecentLotPartsReceived(count);

    public async Task<BasicKitInfo?> GetBasicKitInfo(
        [Service] ComponentSerialService service,
        string vin
    ) => await service.GetBasicKitInfo(vin);

    public async Task<KitComponentSerialInfo?> GetKitComponentSerialInfo(
        [Service] ComponentSerialService service,
        string kitNo,
        string componentCode
    ) => await service.GetKitComponentSerialInfo(kitNo, componentCode);

    public async Task<bool> PingDcwsService(
        [Service] DcwsService service
    ) => await service.CanConnectToService();

    public async Task<string> GetDcwsServiceVersion(
        [Service] DcwsService service
    ) => await service.GetServiceVersion();

    public BomFile ParseBomFile(string text) =>
        new BomFileParser().ParseBomFile(text);

    public ShipFile ParseShipFile(string text) =>
        new ShipFileParser().ParseShipmentFile(text);

    public FordInterfaceFileType GetFordInterfaceFileType(string filename) {
        var result = FordInterfaceFileTypeService.GetFordInterfaceFileType(filename);
        return result;
    }

    public IQueryable<AppSetting> GetAppSettings(SkdContext context)
        => context.AppSettings.AsQueryable();

    public async Task<List<KitTimelineEvent>> GetKitTimelineEventsByDate(
        SkdContext context,
        string plantCode,
        DateTime fromDate,
        DateTime toDate,
        KitTimelineCode? timelineEventCode
    ) {
        var query = context.KitTimelineEvents
            .Where(t => t.Kit.Lot.Plant.Code == plantCode)
            .Where(t =>
                t.EventDate.Date >= fromDate.Date
                &&
                t.EventDate.Date <= toDate.Date
            )
            .Where(t => t.RemovedAt == null).AsQueryable();

        if (timelineEventCode != null) {
            query = query.Where(t => t.EventType.Code == timelineEventCode.Value);
        }

        var result = await query
            .Include(t => t.Kit).ThenInclude(t => t.Lot).ThenInclude(t => t.Plant)
            .Include(t => t.Kit).ThenInclude(t => t.Lot).ThenInclude(t => t.Pcv)
            .Include(t => t.Kit).ThenInclude(t => t.Dealer)
            .Include(t => t.EventType)
            .ToListAsync();

        return result;
    }

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Kit> GetKitsByCurrentTimelineEvent(
        [Service] QueryService service,
        string plantCode,
        KitTimelineCode? eventCode
    ) => service.GetKitsByCurrentTimelineEvent(plantCode, eventCode);

    /// <summary>
    /// Gets kits where most recent status is PLAN_BUILD & VIN is empty & PartnerStatusUpdatedAt is not null
    /// </summary>
    /// <param name="plantCode"></param>
    /// <returns></returns>
    public async Task<List<KitInfoDTO>> GetPlanBuildVinPendingKits(
        [Service] CustomQueryService service,
        string plantCode
    ) => await service.GetPlanBuildVinPendingKits(plantCode);
    
    /// <summary>
    /// Gets kits where status is BUILD_START status should be created
    /// Current status is PLAN_BUILD and has component serial input
    /// </summary>
    /// <param name="plantCode"></param>
    /// <returns></returns>
    public async Task<List<KitInfoDTO>> GetBuildStartPendingKits(
        [Service] CustomQueryService service,
        string plantCode
    )  => await service.GetBuildStartPendingKits(plantCode);


    /// <summary>
    /// Kits that have timeline event entries that have not beed synced to partner status
    /// </summary>
    /// <param name="plantCode"></param>
    /// <returns></returns>
    public async Task<List<KitInfoDTO>> GetUpdatePartnerStatusPendingKits(
        [Service] CustomQueryService service,
        string plantCode
    ) => await service.GetUpdatePartnerStatusPendingKits(plantCode);

    public async Task<KitInfoDTO> GetKitCurrentStatus(
        SkdContext context,
        string kitNo
    ) => await context.Kits.AsNoTracking()
        .Include(k => k.Lot).ThenInclude(k => k.Pcv)
        .Include(k => k.TimelineEvents).ThenInclude(k => k.EventType)
        .Where(k => k.KitNo == kitNo)
        .Select(k => KitInfoDTO.Create(k))
        .FirstOrDefaultAsync();
}


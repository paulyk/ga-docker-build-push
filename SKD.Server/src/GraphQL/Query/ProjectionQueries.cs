namespace SKD.Server;

[ExtendObjectType<Query>]
public class ProjectionQuery {

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AppSetting> GetAppSettings(SkdContext context) => context.AppSettings.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Plant> GetPlants(SkdContext context) => context.Plants.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Dealer> GetDealers(SkdContext context) => context.Dealers.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Kit> GetKits(SkdContext context) => context.Kits.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitVin> GetKitVins(SkdContext context) => context.KitVins.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitTimelineEvent> GetKitTimelineEvents(SkdContext context) => context.KitTimelineEvents.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitTimelineEventType> GetKitTimelineEventTypes(SkdContext context) => context.KitTimelineEventTypes.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Lot> GetLots(SkdContext context) => context.Lots.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Component> GetComponents(SkdContext context) => context.Components.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProductionStation> GetProductionStations(SkdContext context) => context.ProductionStations.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ComponentStation> GetComponentStations(SkdContext context) => context.ComponentStations.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PCV> GetPcvs(SkdContext context) => context.Pcvs.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvModel> GetPcvModels(SkdContext context) => context.PcvModels.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvSubmodel> GetPcvSubmodels(SkdContext context) => context.PcvSubmodels.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvSeries> GetPcvSeries(SkdContext context) => context.PcvSeries.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvEngine> GetPcvEngines(SkdContext context) => context.PcvEngines.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvTransmission> GetPcvTransmissions(SkdContext context) => context.PcvTransmissions.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvDrive> GetPcvDrives(SkdContext context) => context.PcvDrives.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvPaint> GetPcvPaint(SkdContext context) => context.PcvPaint.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvTrim> GetPcvTrim(SkdContext context) => context.PcvTrim.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PcvComponent> GetPcvComponents(SkdContext context) => context.PcvComponents.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitComponent> GetKitComponents(SkdContext context) => context.KitComponents.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ComponentSerial> GetComponentSerials(SkdContext context) => context.ComponentSerials.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DcwsResponse> GetDCWSResponses(SkdContext context) => context.DCWSResponses.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Part> GetParts(SkdContext context) => context.Parts.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Shipment> GetShipments(SkdContext context) => context.Shipments.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ShipmentLot> GetShipmentLots(SkdContext context) => context.ShipmentLots.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ShipmentInvoice> GetShipmentInvoices(SkdContext context) => context.ShipmentInvoices.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<HandlingUnit> GetHandlingUnits(SkdContext context) => context.HandlingUnits.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<HandlingUnitReceived> GetHandlingUnitReceived(SkdContext context) => context.HandlingUnitReceived.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ShipmentPart> GetShipmentParts(SkdContext context) => context.ShipmentParts.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Bom> GetBoms(SkdContext context) => context.Boms.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<LotPart> GetLotParts(SkdContext context) => context.LotParts.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<LotPartReceived> GetLotPartsReceived(SkdContext context) => context.LotPartsReceived.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitSnapshotRun> GetKitSnapshotRuns(SkdContext context) => context.KitSnapshotRuns.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<KitSnapshot> GetKitSnapshots(SkdContext context) => context.KitSnapshots.AsQueryable();

    [UsePaging(MaxPageSize = 10000)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PartnerStatusAck> GetPartnerStatusAcks(SkdContext context) => context.PartnerStatusAcks.AsQueryable();

}


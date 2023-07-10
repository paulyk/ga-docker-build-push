#nullable enable

namespace SKD.Model;

public class SkdContext : DbContext {
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<Dealer> Dealers => Set<Dealer>();
    public DbSet<Kit> Kits => Set<Kit>();
    public DbSet<KitVin> KitVins => Set<KitVin>();
    public DbSet<KitTimelineEvent> KitTimelineEvents => Set<KitTimelineEvent>();
    public DbSet<KitTimelineEventType> KitTimelineEventTypes => Set<KitTimelineEventType>();
    public DbSet<Lot> Lots => Set<Lot>();

    public DbSet<Component> Components => Set<Component>();
    public DbSet<ProductionStation> ProductionStations => Set<ProductionStation>();
    public DbSet<ComponentStation> ComponentStations => Set<ComponentStation>();

    public DbSet<PCV> Pcvs => Set<PCV>();
    public DbSet<PcvModel> PcvModels => Set<PcvModel>();
    public DbSet<PcvSubmodel> PcvSubmodels => Set<PcvSubmodel>();
    public DbSet<PcvSeries> PcvSeries => Set<PcvSeries>();
    public DbSet<PcvEngine> PcvEngines => Set<PcvEngine>();
    public DbSet<PcvTransmission> PcvTransmissions => Set<PcvTransmission>();
    public DbSet<PcvDrive> PcvDrives => Set<PcvDrive>();
    public DbSet<PcvPaint> PcvPaint => Set<PcvPaint>();
    public DbSet<PcvTrim> PcvTrim => Set<PcvTrim>();
    public DbSet<PcvComponent> PcvComponents => Set<PcvComponent>();

    public DbSet<KitComponent> KitComponents => Set<KitComponent>();
    public DbSet<ComponentSerial> ComponentSerials => Set<ComponentSerial>();
    public DbSet<DcwsResponse> DCWSResponses => Set<DcwsResponse>();
    public DbSet<Part> Parts => Set<Part>();

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLot> ShipmentLots => Set<ShipmentLot>();
    public DbSet<ShipmentInvoice> ShipmentInvoices => Set<ShipmentInvoice>();
    public DbSet<HandlingUnit> HandlingUnits => Set<HandlingUnit>();
    public DbSet<HandlingUnitReceived> HandlingUnitReceived => Set<HandlingUnitReceived>();
    public DbSet<ShipmentPart> ShipmentParts => Set<ShipmentPart>();

    public DbSet<Bom> Boms => Set<Bom>();
    public DbSet<LotPart> LotParts => Set<LotPart>();
    public DbSet<LotPartReceived> LotPartsReceived => Set<LotPartReceived>();

    public DbSet<KitSnapshotRun> KitSnapshotRuns => Set<KitSnapshotRun>();
    public DbSet<KitSnapshot> KitSnapshots => Set<KitSnapshot>();
    public DbSet<PartnerStatusAck> PartnerStatusAcks => Set<PartnerStatusAck>();
    // public DbSet<UpdatePartnerResponse> UpdatePartnerResponses => Set<UpdatePartnerResponse>();

    public SkdContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.ApplyConfigurationsFromAssembly(typeof(SkdContext).Assembly);
    }
}

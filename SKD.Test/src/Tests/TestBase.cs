
namespace SKD.Test;

using System.Security.Cryptography;
using PartQuantities = IEnumerable<(string partNo, int quantity)>;

public class TestBase {

    public string TestPlantCode = "HPUDA";
    public string EngineComponentCode = "EN";
    public string TransmissionComponentCode = "TR";
    protected SkdContext context;
    public SkdContext GetAppDbContext() {

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<SkdContext>()
                    .UseSqlite(connection)
                    .Options;

        var ctx = new SkdContext(options);

        ctx.Database.EnsureCreated();
        return ctx;
    }

    public void Gen_Baseline_Test_Seed_Data(
        bool generateLot = true,
        bool assignVin = false
    ) {
        Seed_Master_Data();

        Gen_Pcv_From_Existing_Component_And_Stations();
        var bom = Gen_Bom(context.Plants.First().Code);
        if (generateLot) {
            var pcv = context.Pcvs.First();
            Gen_Lot(bom.Id, pcv.Id, kitCount: 6, assignVin: assignVin);
        }
    }

    public void Seed_Master_Data() {
        Gen_AppSettings();
        Gen_KitTimelineEventTypes();
        Gen_Plant(TestPlantCode);
        Gen_Dealers();
        Gen_Components_and_Stations();
    }

    public void Gen_Components_and_Stations() {
        var entries = new List<(string componentCode, string name, string stationCode, bool saveCDC, int sequence)> {
            ("AXR","AXLE REAR","FRM10",true,1),
            ("B2","SEAT BELT BUCKLE ROW 2","CAB40",true,7),
            ("CUL","CATALYST UNDERBODY LEFT","FRM30",true,3),
            ("CUR","CATALYST UNDERBODY RIGHT","FRM30",true,3),
            ("DA","DRIVER AIRBAG","FIN20",true,9),
            ("DKA","DRIVER KNEE AIRBAG","FIN20",true,9),
            ("DSC","DRIVER SIDE CURTAIN AIRBAG","CAB20",true,5),
            ("EN","ENGINE","FRM20",true,2),
            ("EN","ENGINE","FIN30",false,10),
            ("EN","ENGINE","FIN10",false,8),
            ("ENL","ENGINE LEGAL VO","FRM30",true,3),
            ("EXR","EXHAUST SYSTEM REAR","FRM30",true,3),
            ("FD","FRONT DIFFERENTIAL UNIT","FRM10",true,1),
            ("FNL","FRAME NUMBER LEGAL","FRM10",true,1),
            ("FT","FUEL TANK","FRM30",true,3),
            ("IK","IGNITION KEY","FIN20",true,9),
            ("PAS","PASSENGER SEAT","FIN20",true,9),
            ("PKA","PASSENGER KNEE AIRBAG","FIN20",true,9),
            ("PSC","PASSENGER SIDE CURTAIN AIRBAG","CAB30",true,6),
            ("RS","REAR SEAT","FIN20",true,9),
            ("S1L","SEATBELT ROW 1 LEFT","CAB30",true,6),
            ("S1R","SEATBELT ROW 1 RIGHT","CAB30",true,6),
            ("S2C","SEATBELT ROW 2 CENTER","CAB30",true,6),
            ("S2L","SEATBELT ROW 2 LEFT","CAB30",true,6),
            ("S2R","SEATBELT ROW 2 RIGHT","CAB30",true,6),
            ("S3L","SEATBELT ROW 3 LEFT","CAB30",true,6),
            ("S3R","SEATBELT ROW 3 RIGHT","CAB30",true,6),
            ("SCR","SELECTIVE CATALYTIC REDUCTION","FRM30",true,3),
            ("TC","TRANSFER CASE","FRM30",true,3),
            ("TR","TRANSMISSION","FRM30",true,3),
        };

        var components = Gen_Components(entries.Select(x => x.componentCode).ToArray());
        var stations = Gen_ProductionStations(entries.Select(x => x.stationCode).ToArray());

        foreach (var entry in entries) {
            var componentStation = new ComponentStation {
                Component = components.First(t => t.Code == entry.componentCode),
                Station = stations.First(t => t.Code == entry.stationCode),
                SaveCDCComponent = entry.saveCDC
            };
            context.ComponentStations.Add(componentStation);
        }
        context.SaveChanges();
    }

    public void Gen_AppSettings() {
        var appSettings = new List<AppSetting> {
            new AppSetting { Code = AppSettingCode.PlanBuildLeadTimeDays.ToString(), Value="6"  },
            new AppSetting { Code = AppSettingCode.WholeSaleCutoffDays.ToString(), Value="7" },
            new AppSetting { Code = AppSettingCode.VerifyVinLeadTimeDays.ToString(), Value="2"  },
            new AppSetting { Code = AppSettingCode.EngineComponentCode.ToString(), Value = EngineComponentCode },
        };

        context.AppSettings.AddRange(appSettings);
        context.SaveChanges();
    }

    public Bom Gen_Plant_Bom(string plantCode = null) {
        var plant = Gen_Plant(plantCode);
        var bom = Gen_Bom(plant.Code);
        return bom;
    }

    public Dealer Gen_Dealers() {
        var dealer = new Dealer {
            Code = Util.RandomString(10),
            Name = Util.RandomString(10)
        };
        context.Dealers.Add(dealer);
        context.SaveChanges();
        return dealer;
    }
    public void Gen_Bom_Lot_and_Kits(string plantCode = null, bool assignVin = false) {
        var bom = Gen_Plant_Bom(plantCode);
        var pcv = context.Pcvs.First();
        Gen_Lot(bom.Id, pcv.Id, kitCount: 6, assignVin: assignVin);
    }

    public void Gen_Pcv_From_Existing_Component_And_Stations() {

        var component_station_mappings = context.ComponentStations
            .Include(t => t.Component)
            .Include(t => t.Station)
            .Select(t => new ValueTuple<string, string>(t.Component.Code, t.Station.Code))
            .ToList();

        var pcvCode = Gen_Pcv_Code();
        Gen_Pcv(
            pcvCode: pcvCode,
            component_stations_maps: component_station_mappings
          );
    }

    public Bom Gen_Bom(string plantCode = null) {
        var plant = context.Plants.First(t => t.Code == plantCode);

        var bom = new Bom {
            Plant = plant,
            Sequence = 1,
        };
        context.Boms.Add(bom);
        context.SaveChanges();
        return bom;
    }

    public Lot Gen_Lot(Guid bomId, Guid pcvId, int kitCount = 6, bool assignVin = false) {
        var pcv = context.Pcvs
            .Include(t => t.PcvComponents)
            .Include(t => t.PcvComponents)
            .FirstOrDefault(t => t.Id == pcvId);

        var componentStations = context.ComponentStations.ToList();
        var lotNo = Gen_NewLotNo(pcv.Code);

        var bom = context.Boms.First(t => t.Id == bomId);

        var lot = new Lot {
            Bom = bom,
            LotNo = lotNo,
            Pcv = pcv,
            PlantId = bom.Plant.Id,
            Kits = Enumerable.Range(1, kitCount)
                .Select(kitSeq => GetKitForKitSeq(pcv, kitSeq))
                .ToList()
        };
        context.Lots.Add(lot);
        context.SaveChanges();
        return lot;

        Kit GetKitForKitSeq(PCV pcv, int kitSeq) {
            var kit = new Kit {
                KitNo = Gen_KitNo(lotNo, kitSeq),
                VIN = assignVin ? Gen_VIN() : "",
            };
            // set kit.KitComponnets from componentStations filtered by the componetId in pcv.PcvComponents
            kit.KitComponents = componentStations
                .Where(t => pcv.PcvComponents.Any(c => c.ComponentId == t.Component.Id))
                .Select(t => new KitComponent {
                    ComponentId = t.ComponentId,
                    ProductionStationId = t.StationId
                })
                .ToList();
            return kit;
        }
    }

    public Plant Gen_Plant(string plantCode = null) {
        plantCode = plantCode ?? Gen_PlantCode();

        var plant = context.Plants.FirstOrDefault(t => t.Code == plantCode);
        if (plant != null) {
            return plant;
        }

        plant = new Plant {
            Code = plantCode,
            PartnerPlantCode = Gen_PartnerPLantCode(),
            PartnerPlantType = Gen_PartnerPlantType(),
            Name = $"{plantCode} name"
        };
        context.Plants.Add(plant);
        context.SaveChanges();
        return plant;
    }

    public async Task Gen_KitComponentSerial(string kitNo, string componentCode, string serial1, string serial2, bool verify) {
        var kitComponent = await context.KitComponents.FirstOrDefaultAsync(t => t.Kit.KitNo == kitNo && t.Component.Code == componentCode);
        var componentSerial = new ComponentSerial {
            Serial1 = serial1,
            Serial2 = serial2,
            VerifiedAt = verify ? DateTime.Now : (DateTime?)null,
        };
        kitComponent.ComponentSerials.Add(componentSerial);
        await context.SaveChangesAsync();
    }

    public List<ProductionStation> Gen_ProductionStations(params string[] stationCodes) {
        var currentStationCodes = context.ProductionStations.Select(t => t.Code).ToList();

        var productionStations = stationCodes.Distinct().Except(currentStationCodes).ToList().Select((code, index) => new ProductionStation {
            Code = code,
            Name = $"{code} name",
            Sequence = index + 1
        });

        context.ProductionStations.AddRange(productionStations);
        context.SaveChanges();
        return context.ProductionStations.ToList();
    }

    public List<Component> Gen_Components(params string[] componentCodes) {
        var currentComponentCodes = context.Components.Select(t => t.Code).ToList();

        var components = componentCodes.Distinct().Except(currentComponentCodes).ToList().Select(code => new Component {
            Code = code,
            Name = $"{code} name",
            ComponentSerialRule = ComponentSerialRule.ONE_OR_BOTH_SERIALS
        }).ToList();

        context.Components.AddRange(components);
        context.SaveChanges();
        return context.Components.ToList();
    }

    public PCV Gen_Pcv(
        string pcvCode,
        List<(string componentCode, string stationCode)> component_stations_maps
    ) {
        Gen_Components(component_stations_maps.Select(t => t.componentCode).ToArray());
        Gen_ProductionStations(component_stations_maps.Select(t => t.stationCode).ToArray());

        var distinctComponentCodes = component_stations_maps.Select(t => t.componentCode).Distinct().ToList();

        var pcvComponents = distinctComponentCodes
            .Select(componentCode => new PcvComponent {
                Component = context.Components.First(t => t.Code == componentCode)
            }).ToList();

        var pcv = new PCV {
            Code = pcvCode,
            Description = $"{pcvCode} name",
            PcvComponents = pcvComponents
        };

        context.Pcvs.Add(pcv);
        context.SaveChanges();
        return pcv;
    }

    public ComponentSerial Gen_ComponentScan(Guid kitComponentId) {
        var kitComponent = context.KitComponents.FirstOrDefault(t => t.Id == kitComponentId);
        var componentScan = new ComponentSerial {
            KitComponentId = kitComponentId,
            Serial1 = Util.RandomString(EntityFieldLen.ComponentSerial),
            Serial2 = ""
        };
        context.ComponentSerials.Add(componentScan);
        context.SaveChanges();
        return componentScan;
    }

    public async Task<Kit> Gen_Kit_From_PCV(
        string vin,
        string kitNo,
        string lotNo,
        string pcvCode
        ) {

        // plant
        var plantCode = Gen_PlantCode();
        var plant = new Plant { Code = plantCode };
        context.Plants.Add(plant);

        // pcv
        var pcv = await context.Pcvs
            .Include(t => t.PcvComponents)
            .FirstOrDefaultAsync(t => t.Code == pcvCode);

        var componentStations = await context.ComponentStations.ToListAsync();

        var kitComponents = pcv.PcvComponents.Select(mc => new KitComponent {
            ComponentId = mc.ComponentId,
            ProductionStationId = componentStations
                .Where(t => t.ComponentId == mc.ComponentId)
                .Select(t => t.StationId).First()
        }).ToList();

        var lot = new Lot { LotNo = lotNo, Plant = plant };
        context.Lots.Add(lot);

        var kit = new Kit {
            VIN = vin,
            Lot = lot,
            KitNo = kitNo,
            KitComponents = kitComponents
        };

        context.Kits.AddRange(kit);
        await context.SaveChangesAsync();

        return kit;
    }

    public Kit Gen_Kit_And_Pcv_From_Components(
        List<(string componentCode, string stationCode)> component_stations_maps,
        bool assignVin = false
    ) {

        // ensure component codes
        component_stations_maps.Select(t => t.componentCode).Distinct().ToList().ForEach(code => {
            if (!context.Components.Any(t => t.Code == code)) {
                context.Components.Add(new Component {
                    Code = code,
                    Name = code + " name",
                    ComponentSerialRule = ComponentSerialRule.ONE_OR_BOTH_SERIALS
                });
                context.SaveChanges();
            }
        });
        // ensure production stations
        component_stations_maps.Select(t => t.stationCode).Distinct().ToList().ForEach(code => {
            if (!context.Components.Any(t => t.Code == code)) {
                var lastSorderOrder = context.ProductionStations.OrderByDescending(t => t.Sequence)
                    .Select(t => t.Sequence)
                    .FirstOrDefault();

                context.ProductionStations.Add(new ProductionStation {
                    Code = code,
                    Name = code + " name",
                    Sequence = lastSorderOrder + 1
                });
                context.SaveChanges();
            }
        });
        // ensure component_stations
        component_stations_maps.ForEach(map => {
            if (!context.ComponentStations.Any(t => t.Component.Code == map.componentCode && t.Station.Code == map.stationCode)) {
                var component = context.Components.First(t => t.Code == map.componentCode);
                var station = context.ProductionStations.First(t => t.Code == map.stationCode);
                context.ComponentStations.Add(new ComponentStation {
                    Component = component,
                    Station = station
                });
                context.SaveChanges();
            }
        });

        var pcvCode = Gen_Pcv_Code();
        var pcv = Gen_Pcv(
            pcvCode: pcvCode,
            component_stations_maps: component_stations_maps
          );

        // create kit based on that pcv
        var bom = context.Boms.Include(t => t.Plant).First();
        var plant = bom.Plant;
        var lot = Gen_Lot(bom.Id, pcv.Id, assignVin: assignVin);

        var kit = context.Kits
            .Include(t => t.Lot)
            .First(t => t.Lot.Id == lot.Id);
        return kit;
    }

    public void SetEntityCreatedAt<T>(Guid id, DateTime date) where T : EntityBase {
        var entity = context.Find<T>(id);
        entity.CreatedAt = date;
        context.SaveChanges();
    }

    public void Gen_KitTimelineEventTypes() {
        var kitStatusCode = Enum.GetValues<PartnerStatusCode>().ToList();

        var eventTypes = Enum.GetValues<KitTimelineCode>()
            .Select((code, i) => new KitTimelineEventType() {
                Code = code,
                PartnerStatusCode = kitStatusCode[i],
                Description = code.ToString(),
                Sequence = i + 1
            }).ToList();

        foreach (var eventType in eventTypes) {
            if (!context.KitTimelineEventTypes.Any(t => t.Code == eventType.Code)) {
                context.KitTimelineEventTypes.AddRange(eventTypes);
            }
        }
        context.SaveChanges();
    }


    public async Task Gen_ShipmentLot_ForKit(string kitNo) {
        var kit = await context.Kits
            .Include(t => t.Lot)
            .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        await Gen_ShipmentLot(kit.Lot.LotNo);
    }

    public async Task Gen_ComponnentSerialScan_ForKit(string kitNo) {
        var kit = await context.Kits
            .Include(t => t.Lot)
            .Include(t => t.KitComponents).ThenInclude(t => t.ComponentSerials)
            .FirstOrDefaultAsync(t => t.KitNo == kitNo);

        ComponentSerial cs = new ComponentSerial {
            Serial1 = Util.RandomString(10),
        };
        kit.KitComponents.First().ComponentSerials.Add(cs);
        await context.SaveChangesAsync();

        await Gen_ShipmentLot(kit.Lot.LotNo);
    }

    public async Task Gen_ShipmentLot(string lotNo) {
        var lot = await context.Lots
            .Include(t => t.Bom).ThenInclude(t => t.Plant)
            .FirstOrDefaultAsync(t => t.LotNo == lotNo);

        if (await context.ShipmentLots.AnyAsync(t => t.Lot.LotNo == lot.LotNo)) {
            return;
        }

        var shipment = new Shipment {
            Plant = lot.Bom.Plant,
            Sequence = 2,
            ShipmentLots = new List<ShipmentLot> {
                    new ShipmentLot {
                        Lot = lot
                    }
                }
        };

        context.Shipments.Add(shipment);
        await context.SaveChangesAsync();
    }

    #region generators for specific entity fields
    public string Gen_Code(int len) {
        return Util.RandomString(len).ToUpper();
    }
    public string Gen_LotNo(string pcvCode, int sequence) {
        return pcvCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.Pcv_Code, '0');
    }

    public string Gen_PartnerPLantCode() {
        return Util.RandomString(EntityFieldLen.PartnerPlant_Code);
    }

    public string Gen_LotNo(int sequence) {
        var pcvCode = context.Pcvs.Select(t => t.Code).First();
        return pcvCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.Pcv_Code, '0');
    }

    public string Gen_NewLotNo(string pcvCode) {
        var sequence = 1;
        var lotNo = pcvCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.Pcv_Code, '0');
        var lotExists = context.Lots.Any(t => t.LotNo == lotNo);
        while (lotExists) {
            sequence++;
            lotNo = pcvCode + sequence.ToString().PadLeft(EntityFieldLen.LotNo - EntityFieldLen.Pcv_Code, '0');
            lotExists = context.Lots.Any(t => t.LotNo == lotNo);
        }
        return lotNo;
    }

    public AppSetting Get_AppSetting(AppSettingCode appSettingCode) {
        return context.AppSettings.Where(t => t.Code == appSettingCode.ToString()).First();
    }

    public string Gen_KitNo(string prefix = "", int kitSequence = 1) {
        var suffix = kitSequence.ToString().PadLeft(2, '0');
        return
            prefix +
            Util.RandomString(EntityFieldLen.KitNo - (prefix.Length + suffix.Length)).ToUpper() +
            suffix;
    }
    public string Gen_Pcv_Code() {
        return Util.RandomString(EntityFieldLen.Pcv_Code).ToUpper();
    }

    public string Gen_Pcv_Description() {
        return Util.RandomString(EntityFieldLen.Pcv_Description).ToUpper();
    }
    public string Gen_Pcv_Meta() {
        return Util.RandomString(EntityFieldLen.Pcv_Meta).ToUpper();
    }
    public string Gen_VIN() {
        return Util.RandomString(EntityFieldLen.VIN).ToUpper();
    }
    public string Gen_ComponentCode() {
        return Util.RandomString(EntityFieldLen.Component_Code).ToUpper();
    }
    public string Gen_ProductionStationCode() {
        return Util.RandomString(EntityFieldLen.ProductionStation_Code).ToUpper();
    }
    public string Gen_PlantCode() {
        return Util.RandomString(EntityFieldLen.Plant_Code).ToUpper();
    }
    public string Gen_PartnerPlantCode() {
        return Util.RandomString(EntityFieldLen.PartnerPlant_Code).ToUpper();
    }

    public string Gen_PartnerPlantType() {
        return Util.RandomString(EntityFieldLen.PartnerPlant_Type).ToUpper();
    }

    public string Get_PlantCode() {
        return Util.RandomString(EntityFieldLen.Plant_Code).ToUpper();
    }
    public string Gen_PartNo() {
        return Util.RandomString(EntityFieldLen.Part_No).ToUpper();
    }
    public string Gen_PartDesc() {
        return Util.RandomString(EntityFieldLen.Part_Desc).ToUpper();
    }
    public string Gen_ShipmentInvoiceNo() {
        return Util.RandomString(EntityFieldLen.Shipment_InvoiceNo).ToUpper();
    }
    public string Gen_ComponentSerialNo(string componentCode) {
        if (componentCode == EngineComponentCode) {
            return Util.RandomString(20) + " " + Util.RandomString(4) + " " + Util.RandomString(4);
        }
        return Util.RandomString(EntityFieldLen.ComponentSerial).ToUpper();
    }

    #endregion

    #region bom import 

    public BomFile Gen_BomFileInput(string plantCode, IEnumerable<string> lotNumbers, int kitCount, PartQuantities partQuantities) {
        return new BomFile() {
            PlantCode = plantCode,
            Sequence = 1,
            Filename = "test",
            LotEntries = Gen_LotEntries(lotNumbers, kitCount),
            LotParts = Gen_BomLotParts(lotNumbers, partQuantities)
        };
    }

    private List<BomFile.BomFileLot> Gen_LotEntries(IEnumerable<string> lotNumbers, int kitCount) {
        return lotNumbers.Select(lotNo => new BomFile.BomFileLot {
            LotNo = lotNo,
            Kits = Enumerable.Range(1, kitCount).Select(num => new BomFile.BomFileLot.BomFileKit {
                KitNo = Gen_KitNo(lotNo, num),
                PcvCode = lotNo.Substring(0, EntityFieldLen.Pcv_Code)
            }).ToList()
        }).ToList();
    }

    private List<BomFile.BomFileLotPart> Gen_BomLotParts(IEnumerable<string> lotNumbers, PartQuantities partQuantities) {
        if (!partQuantities.Any()) {
            return new List<BomFile.BomFileLotPart>();
        }

        return lotNumbers.SelectMany(t =>
            partQuantities.Select(lp => new BomFile.BomFileLotPart {
                LotNo = t,
                PartNo = lp.partNo,
                PartDesc = lp.partNo + " desc",
                Quantity = lp.quantity
            })
        ).ToList();
    }
    #endregion
}

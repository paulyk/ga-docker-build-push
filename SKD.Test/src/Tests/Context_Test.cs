using NuGet.Frameworks;

namespace SKD.Test;
public class ContextTest : TestBase {

    [Fact]
    public void Can_add_component() {
        using var ctx = GetAppDbContext();
        // setup
        var component = new Component() {
            Code = new String('X', EntityFieldLen.Component_Code),
            Name = new String('X', EntityFieldLen.Component_Name)
        };

        ctx.Components.Add(component);

        // test
        ctx.SaveChanges();

        // assert
        Assert.Single(ctx.Components);
    }

    [Fact]
    public void Cannot_add_duplication_component_code() {
        using var ctx = GetAppDbContext();
        // setup
        var component_1 = new Component() {
            Code = "Same_Code",
            Name = "Name1",
        };

        var component_2 = new Component() {
            Code = "Same_Code",
            Name = "Name1",
        };

        ctx.Components.Add(component_1);
        ctx.Components.Add(component_2);

        // test + assert
        Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
    }

    [Fact]
    public void Cannot_add_duplication_component_name() {
        var componentName = "SameName";

        using var ctx = GetAppDbContext();
        // setup
        var component_1 = new Component() {
            Code = "Code1",
            Name = componentName,
        };

        var component_2 = new Component() {
            Code = "Code2",
            Name = componentName,
        };

        ctx.Components.Add(component_1);
        ctx.Components.Add(component_2);

        // test + assert
        Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
    }

    [Fact]
    public void Can_add_pcv() {
        using var ctx = GetAppDbContext();
        // setup
        var pcv = new PCV() {
            Code = new String('X', EntityFieldLen.Pcv_Code),
            Description = new String('X', EntityFieldLen.Pcv_Description),
            ModelYear = new String('X', EntityFieldLen.Pcv_Meta),
        };

        ctx.Pcvs.Add(pcv);
        // test
        ctx.SaveChanges();

        // assert
        Assert.Single(ctx.Pcvs);
    }

    [Fact]
    public void Submit_model_input_twice_has_no_side_effect() {
        using var ctx = GetAppDbContext();
        // setup
        var modelCode = new String('A', EntityFieldLen.Pcv_Code);
        var pcv_1 = new PCV() {
            Code = modelCode,
            Description = new String('A', EntityFieldLen.Pcv_Description),
            ModelYear = new String('A', EntityFieldLen.Pcv_Meta),
        };

        var pcv_2 = new PCV() {
            Code = modelCode,
            Description = new String('B', EntityFieldLen.Pcv_Description),
            ModelYear = new String('B', EntityFieldLen.Pcv_Meta),
        };

        ctx.Pcvs.AddRange(pcv_1, pcv_2);

        // test + assert
        Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());

    }

    [Fact]
    public void Can_add_duplicate_vehicle_model_name() {
        using var ctx = GetAppDbContext();
        // setup
        var modelName = new String('A', EntityFieldLen.Component_Name);
        var pcv_1 = new PCV() {
            Code = new String('A', EntityFieldLen.Pcv_Code),
            Description = modelName,
            ModelYear = new String('A', EntityFieldLen.Pcv_Meta),
        };

        var pcv_2 = new PCV() {
            Code = new String('B', EntityFieldLen.Pcv_Code),
            Description = modelName,
            ModelYear = new String('B', EntityFieldLen.Pcv_Meta),
        };

        ctx.Pcvs.AddRange(pcv_1, pcv_2);
        ctx.SaveChanges();

        var count = ctx.Pcvs.Count(t => t.Description == modelName);
        Assert.Equal(2, count);
    }

    [Fact]
    public void Can_add_kit() {
        using var ctx = GetAppDbContext();
        // setup
        var pcv = new PCV() {
            Code = new String('X', EntityFieldLen.Pcv_Code),
            Description = new String('X', EntityFieldLen.Pcv_Description),
            ModelYear = new String('X', EntityFieldLen.Pcv_Meta),
        };
        ctx.Pcvs.Add(pcv);
        ctx.SaveChanges();

        // plant
        var plant = new Plant {
            Code = Gen_PlantCode(),
            PartnerPlantCode = Gen_PartnerPLantCode(),
            PartnerPlantType = Gen_PartnerPlantType()
        };
        ctx.Plants.Add(plant);

        // bom
        var bom = new Bom { Sequence = 1, Plant = plant };
        ctx.Boms.Add(bom);

        // lot
        pcv = ctx.Pcvs.First();
        var lotNo = new String('X', EntityFieldLen.LotNo);
        var lot = new Lot {
            LotNo = Gen_LotNo(pcv.Code, 1),
            Pcv = pcv,
            Bom = bom,
            Plant = plant
        };
        ctx.Lots.Add(lot);

        // kit 
        var kit = new Kit() {
            VIN = new String('X', EntityFieldLen.VIN),
            Lot = lot,
        };

        ctx.Kits.Add(kit);

        // test
        ctx.SaveChanges();

        // assert
        Assert.Single(ctx.Pcvs);
    }

    [Fact]
    public void Cannot_add_vehicle_without_model() {
        using var ctx = GetAppDbContext();
        // setup
        var vehicle = new Kit() {
            VIN = new String('X', EntityFieldLen.VIN),
        };

        ctx.Kits.Add(vehicle);

        // test + assert
        Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
    }

    [Fact]
    public void Cannot_add_vehicle_duplicate_vin() {
        using var ctx = GetAppDbContext();
        // setup
        var pcv = new PCV() {
            Code = new String('X', EntityFieldLen.Pcv_Code),
            Description = new String('X', EntityFieldLen.Pcv_Description),
            ModelYear = new String('X', EntityFieldLen.Pcv_Meta),
        };

        ctx.Pcvs.Add(pcv);

        var vehicle_1 = new Kit() {
            VIN = new String('X', EntityFieldLen.VIN),
        };

        var vehicle_2 = new Kit() {
            VIN = new String('X', EntityFieldLen.VIN),
        };

        ctx.Kits.AddRange(vehicle_1, vehicle_2);

        // test + assert
        Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
    }

    [Fact]
    public void Can_add_parts() {
        using var ctx = GetAppDbContext();
        var parts = new List<Part> {
                    new Part { PartNo = "p1", OriginalPartNo = "p1 -", PartDesc = "p1 desc"},
                    new Part { PartNo = "p2", OriginalPartNo = "p2 -", PartDesc = "p2 desc"},
                };

        ctx.Parts.AddRange(parts);
        Assert.Empty(ctx.Parts);

        ctx.SaveChanges();

        var after_count = ctx.Parts.Count();
        Assert.Equal(parts.Count, after_count);
    }

    [Fact]
    public void Can_add_dealers() {
        using var ctx = GetAppDbContext();
        var dealers = new List<Dealer> {
                    new Dealer { Code = "D1", Name = "name 1"},
                    new Dealer { Code = "D2", Name = "Name 2"},
                };

        ctx.Dealers.AddRange(dealers);
        var before_count = ctx.Dealers.Count();
        Assert.Equal(0, before_count);

        ctx.SaveChanges();

        var after_count = ctx.Dealers.Count();
        Assert.Equal(dealers.Count, after_count);
    }

    [Fact]
    public void Can_add_PcvModel_PcvSubmModel_and_PCV() {
        using var ctx = GetAppDbContext();

        //stations
        var inputStations = new List<ProductionStation> {
            new ProductionStation { Code = "FRM10", Name = "FRM10", Sequence = 1},
            new ProductionStation { Code = "FRM20", Name = "FRM20", Sequence = 2},
        };
        ctx.ProductionStations.AddRange(inputStations);
        ctx.SaveChanges();

        // components
        var inputComponents = new List<Component>() {
            new Component {
                Code= "DA", Name = "Driver Airbag", ProductionStation = ctx.ProductionStations.First()
            },
            new Component {
                Code= "PA", Name = "Passenger Airbag", ProductionStation = ctx.ProductionStations.Skip(1).First()
            },
        };

        ctx.Components.AddRange(inputComponents);
        ctx.SaveChanges();

        // pcv meta data
        ctx.PcvModels.Add(new PcvModel { Code = "Everest", Name = "Everest" });
        ctx.PcvSubmodels.Add(new PcvSubmodel { Code = "U704", Name = "U704" });
        ctx.PcvSeries.Add(new PcvSeries { Code = "SE#GJ", Name = "XLT SERIES" });
        ctx.PcvEngines.Add(new PcvEngine { Code = "EN-YN", Name = "2.0L CR TC DSL PANTHER C" });
        ctx.SaveChanges();

        // add pcv
        var pcvCode = "BPA0A11";
        ctx.Pcvs.Add(new PCV {
            Code = pcvCode,
            Body = "DBL CAB",
            Series = "WILDTRAK",
            ModelYear = "2024",
            Description = "Description",
            PcvModel = ctx.PcvModels.First(),
            PcvSubmodel = ctx.PcvSubmodels.First(),
            PcvSeries = ctx.PcvSeries.First(),
            PcvEngine = ctx.PcvEngines.First()
        });
        ctx.SaveChanges();

        // assert
        var models = ctx.PcvModels.Include(t => t.Pcvs).ToList();
        var subModels = ctx.PcvSubmodels.Include(t => t.Pcvs).ToList();
        var pcvSeries = ctx.PcvSeries.Include(t => t.Pcvs).ToList();
        var pcvEngines = ctx.PcvEngines.Include(t => t.Pcvs).ToList();
        var pcv = ctx.Pcvs
            .Include(t => t.PcvModel)
            .Include(t => t.PcvSubmodel)
            .Include(t => t.PcvEngine)
            .Include(t => t.PcvEngine)
            .First();

        Assert.Equal(pcvCode, pcv.Code);
        Assert.NotNull(pcv.PcvModel);

        Assert.Single(models);
        Assert.Equal(1, models[0].Pcvs.Count);

        Assert.Single(subModels);
        Assert.Single(subModels[0].Pcvs);

        Assert.Single(pcvSeries);
        Assert.Single(pcvSeries[0].Pcvs);

        Assert.Single(pcvEngines);
        Assert.Single(pcvEngines[0].Pcvs);


    }
}

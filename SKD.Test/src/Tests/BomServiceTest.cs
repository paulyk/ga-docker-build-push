namespace SKD.Test;

using Newtonsoft.Json.Bson;
using PartQuantities = IEnumerable<(string partNo, int quantity)>;

public class BomServiceTest : TestBase {

    public BomServiceTest() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data(generateLot: false);
    }

    [Fact]
    public async Task Can_import_bom_file() {

        // setup
        var plant = Gen_Plant();
        var pcv = await context.Pcvs.FirstOrDefaultAsync();
        var lotNo = Gen_LotNo(pcv.Code, 1);

        var partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
        };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo }, 6, partQuantities);

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        // assert no errors
        Assert.False(result.Errors.Any());

        var bom = await context.Boms
            .Include(t => t.Lots).ThenInclude(t => t.Kits).ThenInclude(t => t.KitComponents)
            .Include(t => t.Lots).ThenInclude(t => t.LotParts)
            .FirstOrDefaultAsync(t => t.Plant.Code == plant.Code);

        // assert filename set
        Assert.Equal(input.Filename, bom.Filename);

        // assert lots and kits
        var exptectedLotCount = input.LotEntries.Count();
        var lotCount = bom.Lots.Count;
        Assert.Equal(exptectedLotCount, lotCount);

        var expectedKitCount = input.LotEntries.SelectMany(t => t.Kits).Count();
        var actualKitCount = bom.Lots.Sum(t => t.Kits.Count);
        Assert.Equal(expectedKitCount, actualKitCount);

        // assert lot parts
        var expectedLotPartCount = input.LotParts.Count();
        var actualLotPartCount = bom.Lots.SelectMany(t => t.LotParts).Count();
        Assert.Equal(expectedLotPartCount, actualLotPartCount);

        // assert kit components
        var componentStations = await context.ComponentStations
            .Include(t => t.Component).Include(t => t.Station).ToListAsync();

        foreach (var kit in bom.Lots.First().Kits) {
            foreach (var mapping in componentStations) {
                var kitComponent = kit.KitComponents.FirstOrDefault(t => t.ComponentId == mapping.ComponentId && t.ProductionStationId == mapping.StationId);
                Assert.NotNull(kitComponent);
            }
        }
    }

    [Fact]
    public async Task Can_Change_Fileanme_On_Reimport() {

        var plant = Gen_Plant();
        var pcv = await context.Pcvs.FirstOrDefaultAsync();
        var lotNo = Gen_LotNo(pcv.Code, 1);

        var partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo }, 6, partQuantities);

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);
        // save LotCount, LotPartCode, PartCount, KitCount, KitComponentCount
        var bom = await context.Boms
            .Include(t => t.Lots).ThenInclude(t => t.Kits).ThenInclude(t => t.KitComponents)
            .Include(t => t.Lots).ThenInclude(t => t.LotParts)
            .FirstOrDefaultAsync(t => t.Plant.Code == plant.Code);

        // change input file name
        input.Filename = "new-file-name";
        // re-import
        result = await service.ImportBom(input);
        // asset bom filename has been changed
        var bom2 = await context.Boms
            .Include(t => t.Lots).ThenInclude(t => t.Kits).ThenInclude(t => t.KitComponents)
            .Include(t => t.Lots).ThenInclude(t => t.LotParts)
            .FirstOrDefaultAsync(t => t.Plant.Code == plant.Code);

        Assert.Equal(input.Filename, bom2.Filename);
        // assert lot count, lot part count, kit count, kit component count unchanged
        Assert.Equal(bom.Lots.Count, bom2.Lots.Count);
        Assert.Equal(bom.Lots.SelectMany(t => t.LotParts).Count(), bom2.Lots.SelectMany(t => t.LotParts).Count());
        Assert.Equal(bom.Lots.SelectMany(t => t.Kits).Count(), bom2.Lots.SelectMany(t => t.Kits).Count());
        Assert.Equal(bom.Lots.SelectMany(t => t.Kits).SelectMany(t => t.KitComponents).Count(), bom2.Lots.SelectMany(t => t.Kits).SelectMany(t => t.KitComponents).Count());
    }

    [Fact]
    public async Task Import_bom_reformats_part_no() {

        // setup
        var plant = Gen_Plant();
        var lotNumbers = new string[] { Gen_LotNo(1), Gen_LotNo(2) };

        // trim tailing [- ]* and removes spaces
        var part_numbers = new List<(string partNo, string reformattedPartNo)>() {
                ("-W716936-S442", "W716936-S442"),
                ("- W716899-  S900 -", "W716899-S900"),
                ("- EB3B-31010-  AF3ZHE -", "EB3B-31010-AF3ZHE"),
                ("       -  W500301-S437    -   ", "W500301-S437")
            };

        PartQuantities partQuantities = part_numbers.Select(pn => (pn.partNo, 6)).ToList();

        var input = Gen_BomFileInput(plant.Code, lotNumbers, 6, partQuantities);

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        var partService = new PartService(context);

        // assert 
        foreach (var entry in part_numbers) {
            var part = await context.Parts.FirstOrDefaultAsync(t => t.PartNo == entry.reformattedPartNo);
            var formatted = PartService.ReFormatPartNo(part.PartNo);
            Assert.Equal(formatted, part.PartNo);
        }
    }

    [Fact]
    public async Task Cannot_import_duplicate_bom_lot_parts_in_paylaod() {
        // setup
        var plant = Gen_Plant();
        var pcv = await context.Pcvs.FirstOrDefaultAsync();
        var lotNo1 = Gen_LotNo(pcv.Code, 1);
        var lotNo2 = Gen_LotNo(pcv.Code, 2);

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo1, lotNo2 }, 6, partQuantities);

        // create duplicae by resetting lotNo2 to lotNo1 in lot parts
        input.LotParts = input.LotParts.Select(lp => new BomFile.BomFileLotPart {
            LotNo = lotNo1,
            PartNo = lp.PartNo,
            PartDesc = lp.PartDesc,
            Quantity = lp.Quantity
        }).ToList();

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        // assert
        var expected_error_message = "duplicate Lot + Part number(s) in payload";
        var actual_error_message = result.Errors.Select(t => t.Message).FirstOrDefault();
        Assert.StartsWith(expected_error_message, actual_error_message);
    }

    [Fact]
    public async Task Cannot_import_if_no_lot_parts() {
        // setup
        var plant = Gen_Plant();
        var pcv = await context.Pcvs.FirstOrDefaultAsync();
        var lotNo1 = Gen_LotNo(pcv.Code, 1);

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo1 }, 6, partQuantities);

        // assing empty list to lot parts
        input.LotParts = new List<BomFile.BomFileLotPart>();

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        // assert
        var errorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedError = "No lot parts found";
        Assert.StartsWith(expectedError, errorMessage);
    }

    [Fact]
    public async Task Cannot_import_bom_lot_kits_if_pcv_missing() {
        // setup
        var plant = Gen_Plant();
        var pcv = Gen_Pcv_Code();
        var lotNo = Gen_LotNo(pcv, 1);
        var kitCount = 6;

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };

        var input = Gen_BomFileInput(plant.Code, new string[] { lotNo }, kitCount, partQuantities);

        // test
        var service = new BomService(context);
        var result = await service.ImportBom(input);

        // assert
        Assert.Single(result.Errors);

        var errorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedErrorMessage = "pcv codes not in system";
        Assert.StartsWith(expectedErrorMessage, errorMessage);
    }

    [Fact]
    public async Task Can_import_bom_with_previously_imported_lots() {
        // setup
        var plant = Gen_Plant();
        Gen_Pcv_From_Existing_Component_And_Stations();

        var pcv_1 = await context.Pcvs.FirstOrDefaultAsync();
        var pcv_2 = await context.Pcvs.Skip(1).FirstOrDefaultAsync();
        var lotNo_1 = Gen_LotNo(pcv_1.Code, 1);
        var lotNo_2 = Gen_LotNo(pcv_2.Code, 1);
        var kitCount = 6;

        PartQuantities partQuantities = new List<(string partNo, int quantity)>{
                ("part-1", 6),
                ("part-2", 12)
            };
        var input_1 = Gen_BomFileInput(plant.Code, new string[] { lotNo_1 }, kitCount, partQuantities);
        var input_2 = Gen_BomFileInput(plant.Code, new string[] { lotNo_1, lotNo_2 }, kitCount, partQuantities);

        // test
        var service = new BomService(context);

        var result_1 = await service.ImportBom(input_1);
        Assert.Empty(result_1.Errors);
        var result_2 = await service.ImportBom(input_2);
        Assert.Empty(result_2.Errors);

        var lotPartsCount = await context.LotParts.CountAsync();
        Assert.Equal(4, lotPartsCount);
    }


    [Fact]
    public async Task Can_set_lot_note() {
        Gen_Bom_Lot_and_Kits();
        // setup
        var lot = await context.Lots.FirstOrDefaultAsync();

        var service = new BomService(context);

        //
        var note = "The note";
        var input = new LotNoteInput(lot.LotNo, note);
        var paylaod = await service.SetLotNote(input);

        var updatedLot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

        Assert.Equal(input.Note, updatedLot.Note);
    }

}


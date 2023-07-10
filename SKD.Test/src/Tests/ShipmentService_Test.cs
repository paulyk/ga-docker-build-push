namespace SKD.Test;
public class ShipmentService_Test : TestBase {

    public ShipmentService_Test() {
        context = GetAppDbContext();
        Gen_Baseline_Test_Seed_Data();
    }

    [Fact]
    public async Task Can_import_shipment() {
        // 
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();
        var sequence = 2;

        var input = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);
        var inputMetrics = GetShipmentInputMetrics(input);

        // test
        var before_count = context.ShipmentParts.Count();
        var shipmentService = new ShipmentService(context);
        var result = await shipmentService.ImportShipment(input);

        // payload check:  plant code , sequence, count
        Assert.Equal(plant.Code, result.Payload.PlantCode);
        Assert.Equal(sequence, result.Payload.Sequence);
        Assert.Equal(inputMetrics.LotCount, result.Payload.LotCount);
        Assert.Equal(inputMetrics.InvoiceCount, result.Payload.InvoiceCount);
        Assert.Equal(inputMetrics.HandlingUnitCount, result.Payload.HandlingUnitCount);

        // shipment parts count
        var expected_shipment_parts_count = input.Lots
            .SelectMany(t => t.Invoices)
            .SelectMany(t => t.Parts)
            .Count();

        var actual_shipment_parts_count = context.ShipmentParts.Count();
        Assert.Equal(expected_shipment_parts_count, actual_shipment_parts_count);

        // imported parts count
        var expected_parts_count = input.Lots
            .SelectMany(t => t.Invoices)
            .SelectMany(t => t.Parts)
            .Select(t => t.PartNo).Distinct().Count();

        var actual_parts_count = context.Parts.Count();
        Assert.Equal(expected_parts_count, actual_parts_count);

        // lot parts count
        var actual_lot_parts = context.LotParts.Count();
        Assert.Equal(inputMetrics.LotPartCount, actual_lot_parts);

        // handling unit codes
        var handlingUnitCodes = input.Lots
            .SelectMany(t => t.Invoices).SelectMany(t => t.Parts)
            .Select(t => t.HandlingUnitCode).Distinct().ToList();

        var matchingHandlingUnits = await context.HandlingUnits.Where(t => handlingUnitCodes.Any(code => code == t.Code)).CountAsync();
        Assert.Equal(inputMetrics.HandlingUnitCount, matchingHandlingUnits);
    }

    [Fact]
    public async Task Duplicate_handling_unit_parts_are_grouped() {
        // 
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();
        var sequence = 2;

        var input = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);

        var shipmentLot = input.Lots.First();
        var shipmentInvoice = input.Lots.First().Invoices.First();
        var shipmentPart = shipmentInvoice.Parts.First();
        shipmentInvoice.Parts.Add(shipmentPart);

        // test
        var before_count = context.ShipmentParts.Count();
        var shipmentService = new ShipmentService(context);
        var result = await shipmentService.ImportShipment(input);

        // assert
        // shipment exists
        var shipment = await context.Shipments
            .Where(t => t.Plant.Code == plant.Code)
            .Where(t => t.Sequence == sequence)
            .FirstOrDefaultAsync();

        Assert.NotNull(shipment);

        var shipment_handling_unit_parts = await context.ShipmentParts
            .Where(t => t.HandlingUnit.ShipmentInvoice.ShipmentLot.Lot.LotNo == shipmentLot.LotNo)
            .Where(t => t.HandlingUnit.ShipmentInvoice.InvoiceNo == shipmentInvoice.InvoiceNo)
            .Where(t => t.Part.PartNo == shipmentPart.PartNo)
            .FirstOrDefaultAsync();

        var expecteShipmentPartdQuantity = shipmentPart.Quantity * 2;
        Assert.Equal(expecteShipmentPartdQuantity, shipment_handling_unit_parts.Quantity);

        // expect lot part quancity
        var expected_LotPart_ShipmentQuantity = shipmentLot.Invoices
            .SelectMany(t => t.Parts)
            .Where(t => t.PartNo == shipmentPart.PartNo).Sum(t => t.Quantity);

        var actual_lotPart_ShipmentQuantity = await context.LotParts
            .Where(t => t.Lot.LotNo == shipmentLot.LotNo)
            .Where(t => t.Part.PartNo == shipmentPart.PartNo)
            .Select(t => t.ShipmentQuantity).FirstOrDefaultAsync();

        Assert.Equal(expected_LotPart_ShipmentQuantity, actual_lotPart_ShipmentQuantity);
    }

    [Fact]
    public async Task Import_shpment_creates_lots_and_kits_if_not_found() {
        var plant = await context.Plants.FirstAsync();
        var pcv = await context.Pcvs.Include(p => p.PcvComponents).FirstAsync();
        // generate lot number that it no in db
        var lotNo = pcv.Code + Util.RandomString(8).ToUpper(); 

        var sequence = 2;
        var input = Gen_ShipmentInput(plant.Code, lotNo, sequence);
        var shipmentService = new ShipmentService(context);

        // test
        var result = await shipmentService.ImportShipment(input);

        // get lot
        var lot = await context.Lots
            .Include(t => t.Pcv)
            .Include(t => t.Kits).ThenInclude(t => t.KitComponents).ThenInclude(t => t.Component)
            .Where(t => t.LotNo == lotNo).FirstOrDefaultAsync();

        // asser lot not null
        Assert.NotNull(lot);
        // assert has 6 kits    
        Assert.Equal(6, lot.Kits.Count);
        
        // assert that each kit has kit components and that they match the pcv components
        foreach (var kit in lot.Kits) {
            var kitDistinctComponents = kit.KitComponents.Select(t => t.Component).Distinct().ToList();
            Assert.Equal(pcv.PcvComponents.Count, kitDistinctComponents.Count);
            foreach (var kitComponent in kitDistinctComponents) {
                var pcvComponent = pcv.PcvComponents.FirstOrDefault(t => t.ComponentId == kitComponent.Id);
                Assert.NotNull(pcvComponent);
            }
        }

    }
    

    [Fact]
    public async Task Cannot_import_shipment_with_no_pards() {
        // setup
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();

        var input = new ShipFile() {
            PlantCode = plant.Code,
            Sequence = 1,
            Lots = new List<ShipFileLot> {
                    new ShipFileLot {
                        LotNo = lot.LotNo,
                        Invoices = new List<ShipFileInvoice> {
                            new ShipFileInvoice {
                                InvoiceNo = "001",
                                Parts = new List<ShipFilePart>()
                            }
                        }
                    }
                }
        };


        var before_count = context.ShipmentParts.Count();
        // test
        var shipmentService = new ShipmentService(context);
        var result = await shipmentService.ImportShipment(input);

        // assert
        var errorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedError = "shipment invoices must have parts";
        Assert.Equal(expectedError, errorMessage);
    }

    [Fact]
    public async Task Cannot_import_shipment_invoice_with_no_parts() {
        // setup
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();
        var input = new ShipFile() {
            PlantCode = plant.Code,
            Sequence = 1,
            Lots = new List<ShipFileLot> {
                    new ShipFileLot {
                        LotNo = lot.LotNo,
                        Invoices = new List<ShipFileInvoice> {
                            new ShipFileInvoice {
                                InvoiceNo = "001",
                                Parts = new List<ShipFilePart>()
                            }
                        }
                    }
                }
        };

        var before_count = context.ShipmentParts.Count();
        // test
        var shipmentService = new ShipmentService(context);
        var result = await shipmentService.ImportShipment(input);

        // assert
        var errorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedError = "shipment invoices must have parts";
        Assert.Equal(expectedError, errorMessage);
    }

    [Fact]
    public async Task Cannot_import_shipment_lot_with_no_invoices() {
        // setup
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();
        var input = new ShipFile() {
            PlantCode = plant.Code,
            Sequence = 1,
            Lots = new List<ShipFileLot> {
                    new ShipFileLot {
                        LotNo = lot.LotNo,
                        Invoices = new List<ShipFileInvoice>()
                    }
                }
        };

        var before_count = context.ShipmentParts.Count();
        // test
        var shipmentService = new ShipmentService(context);
        var result = await shipmentService.ImportShipment(input);

        // assert
        var errorMessage = result.Errors.Select(t => t.Message).FirstOrDefault();
        var expectedError = "shipment lots must have invoices";
        Assert.Equal(expectedError, errorMessage);
    }

    [Fact]
    public async Task Shipment_lot_part_to_lotpart_input_works() {
        // setup
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();
        var input = Gen_ShipmentInput(plant.Code, lot.LotNo, 6);
        var inputMetrics = GetShipmentInputMetrics(input);

        // test
        var service = new ShipmentService(context);
        var lotPartInputList = service.Get_LotPartInputs_from_ShipmentInput(input);

        // assert
        var actual_lot_part_count = lotPartInputList.Count;
        Assert.Equal(inputMetrics.LotPartCount, actual_lot_part_count);
    }

    [Fact]
    public async Task Can_set_handling_unit_received() {
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();
        var sequence = 2;

        var shipmentInput = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);
        var inputMetrics = GetShipmentInputMetrics(shipmentInput);

        // test
        var before_count = context.ShipmentParts.Count();
        var shipmentService = new ShipmentService(context);
        var result = await shipmentService.ImportShipment(shipmentInput);

        var handlingUnitCode = shipmentInput.Lots
            .SelectMany(t => t.Invoices)
            .SelectMany(t => t.Parts).Select(t => t.HandlingUnitCode).First();

        var receiveHandlingUnitInput = new ReceiveHandlingUnitInput(handlingUnitCode, Remove: false);

        var handlingUnitService = new HandlingUnitService(context);
        var receivePayload = await handlingUnitService.SetHandlingUnitReceived(receiveHandlingUnitInput);

        var handlingUitReceived = await context.HandlingUnitReceived
            .Include(t => t.HandlingUnit)
            .FirstOrDefaultAsync(t => t.HandlingUnit.Code == handlingUnitCode);
        Assert.Equal(handlingUnitCode, handlingUitReceived.HandlingUnit.Code);
    }

    [Fact]
    public async Task Can_receive_handling_unit_if_code_zero_padded_received() {
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();
        var sequence = 2;

        var shipmentInput = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);
        var inputMetrics = GetShipmentInputMetrics(shipmentInput);

        // test
        var before_count = context.ShipmentParts.Count();
        var shipmentService = new ShipmentService(context);
        var result = await shipmentService.ImportShipment(shipmentInput);

        var handlingUnitCode = shipmentInput.Lots
            .SelectMany(t => t.Invoices)
            .SelectMany(t => t.Parts).Select(t => t.HandlingUnitCode).First();

        // strip leading zero
        handlingUnitCode = handlingUnitCode.TrimStart('0');

        var receiveHandlingUnitInput = new ReceiveHandlingUnitInput(handlingUnitCode, Remove: false);

        var handlingUnitService = new HandlingUnitService(context);
        var receivePayload = await handlingUnitService.SetHandlingUnitReceived(receiveHandlingUnitInput);

        Assert.Empty(receivePayload.Errors);
    }

    [Fact]
    public async Task Can_revoke_handling_unit_received() {
        var plant = await context.Plants.FirstAsync();
        var lot = await context.Lots.FirstAsync();
        var sequence = 2;

        var shipmentInput = Gen_ShipmentInput(plant.Code, lot.LotNo, sequence);
        var inputMetrics = GetShipmentInputMetrics(shipmentInput);

        // test
        var before_count = context.ShipmentParts.Count();
        var shipmentService = new ShipmentService(context);
        var result = await shipmentService.ImportShipment(shipmentInput);

        var handlingUnitCode = shipmentInput.Lots
            .SelectMany(t => t.Invoices)
            .SelectMany(t => t.Parts).Select(t => t.HandlingUnitCode).First();

        var handlingUnitService = new HandlingUnitService(context);

        var input_1 = new ReceiveHandlingUnitInput(handlingUnitCode, Remove: false);
        var receivePayload = await handlingUnitService.SetHandlingUnitReceived(input_1);

        var input_2 = input_1 with { Remove = true };

        var removePayload = await handlingUnitService.SetHandlingUnitReceived(input_2);

        var handlingUitReceived = await context.HandlingUnitReceived
            .Include(t => t.HandlingUnit)
            .FirstOrDefaultAsync(t => t.HandlingUnit.Code == handlingUnitCode);

        Assert.Equal(handlingUnitCode, handlingUitReceived.HandlingUnit.Code);
        Assert.NotNull(handlingUitReceived.RemovedAt);
    }

    public record ShipentInputMetrics(
        int LotCount,
        int InvoiceCount,
        int InvoicePartsCount,
        int PartCount,
        int HandlingUnitCount,
        int LotPartCount);

    public ShipentInputMetrics GetShipmentInputMetrics(ShipFile input) {

        return new ShipentInputMetrics(
           LotCount: input.Lots.Count,
           InvoiceCount: input.Lots.SelectMany(t => t.Invoices).Count(),
           InvoicePartsCount: input.Lots.SelectMany(t => t.Invoices).SelectMany(t => t.Parts).Count(),
           PartCount: input.Lots.SelectMany(t => t.Invoices).SelectMany(t => t.Parts).Select(t => t.PartNo).Distinct().Count(),
           HandlingUnitCount: input.Lots.SelectMany(t => t.Invoices).SelectMany(t => t.Parts).Select(t => t.HandlingUnitCode).Distinct().Count(),
           LotPartCount: input.Lots.Select(t => new {
               lotParts = t.Invoices
                    .SelectMany(t => t.Parts)
                    .Select(u => new { t.LotNo, u.PartNo }).Distinct()
           }).SelectMany(t => t.lotParts).Count()
        );
    }

    public ShipFile Gen_ShipmentInput(string plantCode, string lotNo, int sequence, int startInvoiceNo = 1) {
        var invoiceNo = startInvoiceNo;

        var input = new ShipFile() {
            PlantCode = plantCode,
            Sequence = sequence,
            Lots = new List<ShipFileLot> {
                    new ShipFileLot {
                        LotNo = lotNo,
                        Invoices = new List<ShipFileInvoice> {
                            new ShipFileInvoice {
                                InvoiceNo = (invoiceNo++).ToString().PadLeft(EntityFieldLen.Shipment_InvoiceNo, '0'),
                                Parts = new List<ShipFilePart> {
                                    new ShipFilePart {
                                        HandlingUnitCode = "0000001",
                                        PartNo = "part-1",
                                        CustomerPartDesc = "part 1 desc",
                                        CustomerPartNo = "cust 0001",
                                        Quantity = 1
                                    }
                                }
                            },
                            new ShipFileInvoice {
                                InvoiceNo = (invoiceNo++).ToString().PadLeft(EntityFieldLen.Shipment_InvoiceNo, '0'),
                                Parts = new List<ShipFilePart> {
                                    new ShipFilePart {
                                        HandlingUnitCode = "0000002",
                                        PartNo = "part-1",
                                        CustomerPartDesc = "part 1 desc",
                                        CustomerPartNo = "part 1 desc",
                                        Quantity = 3
                                    },
                                    new ShipFilePart {
                                        HandlingUnitCode = "0000002",
                                        PartNo = "part-2",
                                        CustomerPartDesc = "part 2 desc",
                                        CustomerPartNo = "part 2 desc",
                                        Quantity = 2
                                    },
                                    new ShipFilePart {
                                        HandlingUnitCode = "0000003",
                                        PartNo = "part-3",
                                        CustomerPartDesc = "part 3 desc",
                                        CustomerPartNo = "part 3 desc",
                                        Quantity = 4
                                    },
                                    new ShipFilePart {
                                        HandlingUnitCode = "0000003",
                                        PartNo = "part-4",
                                        CustomerPartDesc = "part 4 desc",
                                        CustomerPartNo = "part 4 desc",
                                        Quantity = 4
                                    },

                                }
                            },
                        }
                    }
                }
        };
        return input;
    }
}


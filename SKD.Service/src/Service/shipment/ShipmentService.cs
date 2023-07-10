#nullable enable

namespace SKD.Service;
public class ShipmentService {
    private readonly SkdContext context;

    public ShipmentService(SkdContext ctx) {
        this.context = ctx;
    }
    public async Task<MutationResult<ShipmentOverviewDTO>> ImportShipment(ShipFile input) {

        MutationResult<ShipmentOverviewDTO> result = new();

        result.Errors = await ValidateShipmentInput<ShipFile>(input);
        if (result.Errors.Count > 0) {
            return result;
        }

        // ensure parts
        var parts = await GetEnsureParts(input);

        // plant 
        var plant = await context.Plants.FirstAsync(t => t.Code == input.PlantCode);

        // ensure lots and kits
        var lots = await GetEnsureLots(input, plant);

        // ensure shipment
        var shipment = await EnsureShipmentCreated(input, plant, lots, parts);

        // Add / Update LotPart(s)
        await EnsureLotPartsCreated(input, lots, parts);

        // set filename
        shipment.Filename = input.Filename;
        // save
        await context.SaveChangesAsync();

        result.Payload = await GetShipmentOverview(shipment.Id);
        return result;
    }

    private async Task<List<Part>> GetEnsureParts(ShipFile input) {
        input.Lots.SelectMany(t => t.Invoices).SelectMany(t => t.Parts).ToList().ForEach(p => {
            p.PartNo = PartService.ReFormatPartNo(p.PartNo);
        });
        var partService = new PartService(context);
        List<(string, string)> inputParts = input.Lots
            .SelectMany(t => t.Invoices)
            .SelectMany(t => t.Parts)
            .Select(t => (t.PartNo, t.CustomerPartDesc)).ToList();
        return await partService.GetEnsureParts(inputParts);
    }

    private async Task<List<Lot>> GetEnsureLots(ShipFile input, Plant plant) {
        var lotNos = input.Lots.Select(t => t.LotNo).ToList();

        var lots = new List<Lot>();
        foreach (var lotNo in lotNos) {

            var lot = await context.Lots
                .Include(t => t.Kits).ThenInclude(t => t.KitComponents)
                .FirstOrDefaultAsync(t => t.LotNo == lotNo);

            if (lot == null) {

                var pcv = await context.Pcvs
                    .Include(p => p.PcvComponents).ThenInclude(p => p.Component)
                    .FirstAsync(t => t.Code == lotNo.Substring(0, 7));

                lot = new Lot {
                    LotNo = lotNo,
                    PlantId = plant.Id,
                    PcvId = pcv.Id,
                };
                lot.Kits.AddRange(await CreateKits(lotNo, pcv));
            }
            lots.Add(lot);
        }
        return lots;
    }

    private async Task<List<Kit>> CreateKits(string lotNo, PCV pcv) {
        var kits = new List<Kit>();
        for (int i = 1; i <= 6; i++) {
            var kitNo = $"{lotNo}0{i}";
            var kit = new Kit {
                KitNo = kitNo,
                KitComponents = await GetKitComponents(pcv)
            };
            kits.Add(kit);
        }
        return kits;
    }

    private async Task<List<KitComponent>> GetKitComponents(PCV pcv) {

        var componentCodes = pcv.PcvComponents
            .Where(t => t.RemovedAt == null)
            .Select(t => t.Component.Code)
            .ToList();

        // componentStations where component.Code in componentCodes
        var componentStations = await context.ComponentStations
            .Include(t => t.Component)
            .Include(t => t.Station)
            .Where(t => componentCodes.Contains(t.Component.Code))
            .ToListAsync();

        if (componentStations.Count == 0) {
            throw new Exception("No component stations found for pcv");
        }

        var kitComponents = new List<KitComponent>();
        foreach (var componentStation in componentStations) {
            var kitComponent = new KitComponent {
                Component = componentStation.Component,
                ProductionStation = componentStation.Station,
            };
            kitComponents.Add(kitComponent);
        }

        return kitComponents;
    }

    private async Task<Shipment> EnsureShipmentCreated(ShipFile input, Plant plant, List<Lot> lots, List<Part> parts) {


        // does shipment exits?
        var shipment = await context.Shipments.FirstOrDefaultAsync(t => t.PlantId == plant.Id && t.Sequence == input.Sequence);

        if (shipment == null) {
            shipment = new Shipment() {
                Plant = plant,
                Sequence = input.Sequence,
                Filename = input.Filename,
                ShipmentLots = input.Lots.Select(lotDTO => new ShipmentLot {
                    Lot = lots.First(t => t.LotNo == lotDTO.LotNo),
                    Invoices = lotDTO.Invoices.Select(invoiceDTO => new ShipmentInvoice {
                        InvoiceNo = invoiceDTO.InvoiceNo,
                        ShipDate = invoiceDTO.ShipDate,
                        HandlingUnits = invoiceDTO.Parts
                            .GroupBy(t => t.HandlingUnitCode)
                            .Select(g => new HandlingUnit {
                                Code = g.Key,
                                Parts = g.GroupBy(p => p.PartNo).Select(u => new ShipmentPart {
                                    Part = parts.FirstOrDefault(t => t.PartNo == u.Key),
                                    Quantity = u.Sum(x => x.Quantity)
                                }).ToList()
                            }).ToList(),
                    }).ToList()
                }).ToList()
            };
            context.Shipments.Add(shipment);
        }

        return shipment;
    }

    private async Task EnsureLotPartsCreated(ShipFile input, List<Lot> lots, List<Part> parts) {
        var lotPartInputs = Get_LotPartInputs_from_ShipmentInput(input);
        var lotNumbers = lotPartInputs.Select(t => t.LotNo).Distinct().ToList();

        foreach (var lotPartInput in lotPartInputs) {
            var lotPart = await context.LotParts
                .Where(t => t.Lot.LotNo == lotPartInput.LotNo)
                .Where(t => t.Part.PartNo == PartService.ReFormatPartNo(lotPartInput.PartNo))
                .FirstOrDefaultAsync();

            if (lotPart == null) {
                lotPart = new LotPart {
                    Lot = lots.First(t => t.LotNo == lotPartInput.LotNo),
                    Part = parts.First(t => t.PartNo == PartService.ReFormatPartNo(lotPartInput.PartNo)),
                };
                context.LotParts.Add(lotPart);
            }
            lotPart.ShipmentQuantity = lotPartInput.Quantity;
        }
    }

    public async Task<List<Error>> ValidateShipmentInput<T>(ShipFile input) where T : ShipFile {
        var errors = new List<Error>();

        // plant
        var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
        if (plant == null) {
            errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
            return errors;
        }

        // Duplicate shipment 
        /******
        var duplicateShipment = await context.Shipments.AnyAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);
        if (duplicateShipment) {
            errors.Add(new Error("", $"duplicate shipment plant & sequence found {input.PlantCode} sequence {input.Sequence}"));
            return errors;
        }
        */


        // missing Lots
        /*******
        var incomingLotNumbers = input.Lots.Select(t => t.LotNo).Distinct().ToList();
        var existingLotNumbers = await context.Lots
            .Where(t => incomingLotNumbers.Any(lotNo => lotNo == t.LotNo))
            .Select(t => t.LotNo)
            .ToListAsync();
        var missingLotNumbers = incomingLotNumbers.Except(existingLotNumbers).ToList();

        if (missingLotNumbers.Count > 0) {
            var missingNumbersStr = String.Join(", ", missingLotNumbers.Take(3));
            errors.Add(new Error("", $"lot number(s) not found {missingNumbersStr}..."));
            return errors;
        }
        */


        // shipment input must have lot + invoice + handling_units + parts
        if (!input.Lots.Any()) {
            errors.Add(new Error("", "shipment must have lots"));
            return errors;
        }

        // lots must have invoices
        if (input.Lots.Any(t => !t.Invoices.Any())) {
            errors.Add(new Error("", "shipment lots must have invoices"));
            return errors;
        }

        // invoices must have parts
        if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Count == 0))) {
            errors.Add(new Error("", "shipment invoices must have parts"));
            return errors;
        }

        // handling unit code required
        if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Any(p => p.HandlingUnitCode is null or "")))) {
            errors.Add(new Error("", "shipment handling unit code cannot be empty"));
            return errors;
        }

        // invoice numbers already exists
        var inputInvoiceNos = input.Lots
            .SelectMany(t => t.Invoices)
            .Select(t => t.InvoiceNo).Distinct().ToList();


        // comment to allow re-import

        /******* 
        var invoicesAlreadyImported = await context.ShipmentInvoices
            .Where(t => inputInvoiceNos.Any(invoiceNo => invoiceNo == t.InvoiceNo)).Select(t => t.InvoiceNo)
            .ToListAsync();

        if (invoicesAlreadyImported.Any()) {
            var invoiceNos = String.Join(", ", invoicesAlreadyImported.Take(5));
            errors.Add(new Error("", $"invoice numbers already imported: {invoiceNos}"));
            return errors;
        }
        */

        // handling units already exist

        /******
        var inputHandlingUnitCodes = input.Lots
            .SelectMany(t => t.Invoices)
            .SelectMany(t => t.Parts)
            .Select(p => p.HandlingUnitCode).Distinct().ToList();

        var handlingUnitCodeAlreadyImported = await context.HandlingUnits
            .Where(t => inputHandlingUnitCodes.Any(code => code == t.Code))
            .Select(t => t.Code)
            .ToListAsync();

        if (handlingUnitCodeAlreadyImported.Any()) {
            var codes = String.Join(", ", handlingUnitCodeAlreadyImported.Take(5));
            errors.Add(new Error("", $"handling units already imported: {codes}"));
            return errors;
        }
        */

        // part no required
        if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Any(p => p.PartNo is null or "")))) {
            errors.Add(new Error("", "shipment partNo cannot be empty"));
            return errors;
        }

        // quantity >= 0
        if (input.Lots.Any(t => t.Invoices.Any(u => u.Parts.Any(p => p.Quantity <= 0)))) {
            errors.Add(new Error("", "shipment part quantity cannot be <= 0"));
            return errors;
        }

        return errors;
    }

    public async Task<ShipmentOverviewDTO?> GetShipmentOverview(Guid id) {
        var result = await context.Shipments.Select(t => new ShipmentOverviewDTO {
            Id = t.Id,
            PlantCode = t.Plant.Code,
            Sequence = t.Sequence,
            BomId = t.ShipmentLots.Select(u => u.Lot.BomId).FirstOrDefault(),
            BomSequence = t.ShipmentLots.Select(u => u.Lot.Bom.Sequence).FirstOrDefault(),
            LotCount = t.ShipmentLots.Count,
            LotNumbers = t.ShipmentLots.Select(t => t.Lot.LotNo).ToList(),
            InvoiceCount = t.ShipmentLots.SelectMany(t => t.Invoices).Count(),

            HandlingUnitCount = t.ShipmentLots.SelectMany(t => t.Invoices).SelectMany(t => t.HandlingUnits).Count(),
            HandlingUnitReceivedCount = t.ShipmentLots.SelectMany(t => t.Invoices).SelectMany(t => t.HandlingUnits)
                .Where(t => t.Received.Any(t => t.RemovedAt == null)).Count(),

            LotPartCount = t.ShipmentLots.SelectMany(u => u.Lot.LotParts).Count(),
            LotPartReceivedCount = t.ShipmentLots.SelectMany(u => u.Lot.LotParts)
                .Where(u => u.Received.Any(y => y.RemovedAt == null)).Count(),

            BomShipDiffCount = t.ShipmentLots.SelectMany(u => u.Lot.LotParts)
                .Where(u => u.BomQuantity != u.ShipmentQuantity).Count(),

            LotPartReceiveBomDiffCount = t.ShipmentLots
                .SelectMany(u => u.Lot.LotParts)
                .Where(lp => lp.Received.Any(r => r.RemovedAt == null && r.Quantity != lp.BomQuantity)).Count(),

            PartCount = t.ShipmentLots
                .SelectMany(t => t.Invoices)
                .SelectMany(t => t.HandlingUnits)
                .SelectMany(t => t.Parts)
                .Distinct().Count(),
            CreatedAt = t.CreatedAt
        }).FirstOrDefaultAsync(t => t.Id == id);

        return result;
    }

    public List<LotPartQuantityDTO> Get_LotPartInputs_from_ShipmentInput(ShipFile shipmentInput) {

        var lotParts = shipmentInput.Lots.Select(t => new {
            LotParts = t.Invoices.SelectMany(u => u.Parts)
                .Select(u => new {
                    t.LotNo,
                    u.PartNo,
                    u.Quantity
                })
        }).ToList();

        var flattenedLotParts = lotParts.SelectMany(t => t.LotParts);

        var groupedLotParts = flattenedLotParts.GroupBy(t => new { t.LotNo, t.PartNo });

        var result = groupedLotParts.Select(g => new LotPartQuantityDTO {
            LotNo = g.Key.LotNo,
            PartNo = g.Key.PartNo,
            Quantity = g.Select(u => u.Quantity).Sum()
        }).ToList();

        return result;
    }
}


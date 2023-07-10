#nullable enable

using System.ComponentModel;

namespace SKD.Service;

public class BomService {
    private readonly SkdContext context;

    public BomService(SkdContext ctx) {
        this.context = ctx;
    }

    ///<summary>
    /// Import BOM lot, kits, lot parts from bom file
    ///</summary>
    public async Task<MutationResult<BomOverviewDTO>> ImportBom(BomFile input) {

        MutationResult<BomOverviewDTO> result = new() {
            Errors = await ValidateBomFileInput<BomFile>(input)
        };
        if (result.Errors.Any()) {
            return result;
        }

        // add / updaet BOM
        var bom = await GetEnsureBom(input);

        // add parts
        var parts = await GetEnsureParts(input);

        // add Lots and kits
        await EnsureLots(input, bom);

        // add Lot Part
        Ensure_LotParts(input, bom, parts);

        await context.SaveChangesAsync();

        result.Payload = await GetBomOverview(bom.Sequence);
        return result;

    }

    #region import bom lot helpers

    private async Task<Bom> GetEnsureBom(BomFile input) {
        var plant = await context.Plants.FirstAsync(t => t.Code == input.PlantCode);
        var bom = await context.Boms
            .Include(t => t.Plant)
            .Include(t => t.Lots).ThenInclude(t => t.Kits)
            .Include(t => t.Lots).ThenInclude(t => t.LotParts).ThenInclude(t => t.Part)
            .FirstOrDefaultAsync(t => t.Plant.Code == input.PlantCode && t.Sequence == input.Sequence);

        if (bom == null) {
            bom = new Bom {
                Plant = plant,
                Sequence = input.Sequence,
            };
            context.Boms.Add(bom);
        }
        bom.Filename = input.Filename;
        return bom;
    }

    private async Task EnsureLots(BomFile input, Bom bom) {
        var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
        foreach (var inputLot in input.LotEntries) {
            // Due to the unpredictable order of shipment / bom file imports, the lot may already exist        
            var lot = await context.Lots
                .Include(l => l.Kits)
                .FirstOrDefaultAsync(t => t.LotNo == inputLot.LotNo);
            if (lot == null) {
                var pcvCode = inputLot.LotNo.Substring(0, EntityFieldLen.Pcv_Code);
                lot = new Lot {
                    LotNo = inputLot.LotNo,
                    Plant = plant,
                    Pcv = await context.Pcvs.FirstOrDefaultAsync(t => t.Code == pcvCode)
                };
                foreach (var inputKit in inputLot.Kits) {
                    var kit = await CreateKit(inputKit);
                    lot.Kits.Add(kit);
                }
                bom.Lots.Add(lot);
            }
        }
    }

    private async Task<List<Part>> GetEnsureParts(BomFile input) {
        // Reformat to ensure consistent part number format
        input.LotParts.ToList().ForEach(t => {
            t.PartNo = PartService.ReFormatPartNo(t.PartNo);
        });

        var partService = new PartService(context);
        List<(string, string)> inputParts = input.LotParts
            .Select(t => (t.PartNo, t.PartDesc)).ToList();
        return await partService.GetEnsureParts(inputParts);
    }

    private void Ensure_LotParts(
        BomFile input,
        Bom bom,
        IEnumerable<Part> parts
    ) {
        // add lotPart or updat lotPart.BomQuantity
        // first loop over input.LotParts
        var bomLotParts = bom.Lots.SelectMany(t => t.LotParts).ToList();

        foreach (var inputLotPart in input.LotParts) {
            var lotPart = bomLotParts
                .Where(t => t.Lot.LotNo == inputLotPart.LotNo && t.Part.PartNo == inputLotPart.PartNo)
                .FirstOrDefault();

            if (lotPart == null) {
                lotPart = new LotPart {
                    Lot = bom.Lots.First(t => t.LotNo == inputLotPart.LotNo),
                    Part = parts.First(t => t.PartNo == inputLotPart.PartNo),
                    BomQuantity = inputLotPart.Quantity,
                };
                var lot = bom.Lots.First(t => t.LotNo == inputLotPart.LotNo);
                lot.LotParts.Add(lotPart);
            } else {
                lotPart.BomQuantity = inputLotPart.Quantity;
            }
        }
    }

    #endregion

    public async Task<List<Error>> ValidateVehicleLotPartsInput<T>(BomLotPartDTO input) where T : BomLotPartDTO {
        var errors = new List<Error>();

        var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
        if (plant == null) {
            errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
            return errors;
        }

        if (!input.LotParts.Any()) {
            errors.Add(new Error("", "No lot parts found"));
            return errors;
        }

        // duplicate lotNo + Part in payload
        var duplicateLotParts = input.LotParts.GroupBy(t => new { t.LotNo, t.PartNo })
            .Any(g => g.Count() > 1);
        if (duplicateLotParts) {
            errors.Add(new Error("", "Duplicate Lot + Part number(s) in payload"));
            return errors;
        }

        // validate lotNo format
        if (input.LotParts.Any(t => !Validator.Valid_LotNo(t.LotNo))) {
            errors.Add(new Error("", "Lot numbers with invalid format found"));
            return errors;
        }

        if (input.LotParts.Any(t => t.PartNo is null or "")) {
            errors.Add(new Error("", "entries with missing part number(s)"));
            return errors;
        }

        if (input.LotParts.Any(t => t.PartDesc is null or "")) {
            errors.Add(new Error("", "entries with missing part decription(s)"));
            return errors;
        }

        if (input.LotParts.Any(t => t.Quantity <= 0)) {
            errors.Add(new Error("", "entries with quantity <= 0"));
            return errors;
        }

        return errors;
    }

    // private async Task EnsureKits(BomFile input, Bom bom) {
    //     foreach (var inputLot in input.LotEntries) {
    //         var pcvCode = inputLot.Kits.Select(t => t.PcvCode).First();
    //         var lot = bom.Lots.FirstOrDefault(t => t.LotNo == inputLot.LotNo);
    //         // kits could have been previously imported.
    //         if (lot != null && !lot.Kits.Any()) {
    //             foreach (var inputKit in inputLot.Kits) {
    //                 var kit = await CreateKit(inputKit);
    //                 lot.Kits.Add(kit);
    //             }
    //         }
    //     }
    // }

    private async Task<Kit> CreateKit(BomFile.BomFileLot.BomFileKit input) {
        var kits = new List<Kit>();

        var pcv = await context.Pcvs
            .Include(t => t.PcvComponents).ThenInclude(t => t.Component)
            .Where(t => t.Code == input.PcvCode)
            .FirstAsync();

        var kit = new Kit {
            KitNo = input.KitNo
        };

        // get components from pcv.PcvComponents where removedAt is null
        var components = pcv.PcvComponents
            .Where(t => t.RemovedAt == null)
            .Select(t => t.Component)
            .ToList();

        // filter componentStations for entries with components in pcv.PcvComponents
        var componentStations = await context.ComponentStations
            .Include(t => t.Component)
            .Include(t => t.Station)
            .Where(t => t.RemovedAt == null)
            .Where(t => components.Contains(t.Component))
            .ToListAsync();

        // create kit.KitsComponent from componentStations
        foreach (var componentStation in componentStations) {
            var kitComponent = new KitComponent {
                Component = componentStation.Component,
                ProductionStation = componentStation.Station
            };
            kit.KitComponents.Add(kitComponent);
        }

        return kit;
    }

    public async Task<List<Error>> ValidateBomFileInput<T>(BomFile input) where T : BomFile {
        var errors = new List<Error>();

        var plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode);
        if (plant == null) {
            errors.Add(new Error("PlantCode", $"plant not found  {input.PlantCode}"));
            return errors;
        }

        if (!input.LotEntries.Any()) {
            errors.Add(new Error("", "no lots found"));
            return errors;
        }

        if (!input.LotParts.Any()) {
            errors.Add(new Error("", "No lot parts found"));
            return errors;
        }

        // kits alread imported
        // var newLotNumbers = input.LotEntries.Select(t => t.LotNo).ToList();
        // var previouslyImportedLotNumbers = await context.Lots
        //     .Where(t => newLotNumbers.Any(newLotNo => newLotNo == t.LotNo))
        //     .Select(t => t.LotNo)
        //     .ToListAsync();

        // if (previouslyImportedLotNumbers.Count() == input.LotEntries.Count()) {
        //     errors.Add(new Error("", "Lots already imported"));
        // }

        // duplicate lot number in lot enties
        var duplicate_lotNo = input.LotEntries.GroupBy(t => t.LotNo)
            .Any(g => g.Count() > 1);
        if (duplicate_lotNo) {
            errors.Add(new Error("", "duplicate Lot numbers in payload"));
            return errors;
        }

        // duplicate lot part number
        var duplicate_lotPart = input.LotParts.GroupBy(t => new { t.LotNo, t.PartNo })
            .Any(g => g.Count() > 1);
        if (duplicate_lotPart) {
            errors.Add(new Error("", "duplicate Lot + Part number(s) in payload"));
            return errors;
        }

        // validate lotNo format
        if (input.LotEntries.Any(t => !Validator.Valid_LotNo(t.LotNo))) {
            errors.Add(new Error("", "lot numbers  with invalid format found"));
            return errors;
        }

        // validate kitNo format
        if (input.LotEntries.Any(t => t.Kits.Any(k => !Validator.Valid_KitNo(k.KitNo)))) {
            errors.Add(new Error("", "kit numbers with invalid format found"));
            return errors;
        }

        // missing pcv code
        if (input.LotEntries.Any(t => t.Kits.Any(k => k.PcvCode is null or ""))) {
            errors.Add(new Error("", "kits with missing pcv code found"));
            return errors;
        }

        // pcv codes not found
        var incommingPcvCodes = input.LotEntries.SelectMany(t => t.Kits).Select(k => k.PcvCode).Distinct();
        var systemPcvCodes = await context.Pcvs
            .Where(t => t.RemovedAt == null).Select(t => t.Code).ToListAsync();

        var matchingModelCodes = incommingPcvCodes.Intersect(systemPcvCodes);
        var missingModelCodes = incommingPcvCodes.Except(matchingModelCodes);

        if (missingModelCodes.Any()) {
            errors.Add(new Error("", $"pcv codes not in system or removed: {String.Join(",", missingModelCodes)}"));
            return errors;
        }

        return errors;
    }


    public async Task<BomOverviewDTO> GetBomOverview(Guid id) {
        var bom = await context.Boms
            .Where(t => t.Id == id)
            .Select(t => new BomOverviewDTO {
                Id = t.Id,
                PlantCode = t.Plant.Code,
                Sequence = t.Sequence,
                Shipments = t.Lots.SelectMany(u => u.ShipmentLots).Select(u => new BomShipInfoDTO {
                    ShipmentId = u.ShipmentId,
                    Sequence = u.Shipment.Sequence,
                    PlantCode = t.Plant.Code
                }).ToList(),
                LotNumbers = t.Lots.Select(u => u.LotNo).ToList(),
                PcvCodes = t.Lots.Select(u => u.Pcv.Code).Distinct().ToList(),
                PartCount = t.Lots.SelectMany(u => u.LotParts).Select(u => u.Part).Distinct().Count(),
                VehicleCount = t.Lots.SelectMany(u => u.Kits).Count(),
                CreatedAt = t.CreatedAt
            })
            .FirstAsync();

        return bom;
    }

    public async Task<BomOverviewDTO> GetBomOverview(int bomSequenceNo) {
        var bom = await context.Boms
            .Where(t => t.Sequence == bomSequenceNo)
            .Select(t => new BomOverviewDTO {
                Id = t.Id,
                PlantCode = t.Plant.Code,
                Sequence = t.Sequence,
                LotNumbers = t.Lots.Select(u => u.LotNo).ToList(),
                PcvCodes = t.Lots.Select(u => u.Pcv.Code).ToList(),
                PartCount = t.Lots.SelectMany(u => u.LotParts).Select(u => u.Part).Distinct().Count(),
                VehicleCount = t.Lots.SelectMany(u => u.Kits).Count(),
                CreatedAt = t.CreatedAt
            })
            .FirstAsync();

        // Extra step because of the distinct in the query does not work with sqlite
        bom.PcvCodes = bom.PcvCodes.Distinct().ToList();

        return bom;
    }

    public async Task<BomOverviewDTO> GetBomLots(Guid id) {
        var bom = await context.Boms
            .Where(t => t.Id == id)
            .Select(t => new BomOverviewDTO {
                Id = t.Id,
                PlantCode = t.Plant.Code,
                Sequence = t.Sequence,
                LotNumbers = t.Lots.Select(u => u.LotNo).ToList(),
                PcvCodes = t.Lots.Select(u => u.Pcv.Code).Distinct().ToList(),
                PartCount = t.Lots.SelectMany(u => u.LotParts).Select(u => u.Part).Distinct().Count(),
                VehicleCount = t.Lots.SelectMany(u => u.Kits).Count(),
                CreatedAt = t.CreatedAt
            })
            .FirstAsync();

        return bom;
    }

    #region lot note
    public async Task<MutationResult<Lot>> SetLotNote(LotNoteInput input) {
        MutationResult<Lot> paylaod = new();
        paylaod.Errors = await ValidateSetLotNote(input);
        if (paylaod.Errors.Any()) {
            return paylaod;
        }
        var lot = await context.Lots.FirstAsync(t => t.LotNo == input.LotNo);
        lot.Note = input.Note;

        await context.SaveChangesAsync();
        paylaod.Payload = lot;
        return paylaod;
    }

    public async Task<List<Error>> ValidateSetLotNote(LotNoteInput input) {
        var errors = new List<Error>();
        var lot = await context.Lots.FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

        if (lot == null) {
            errors.Add(new Error("LotNo", $"Lot not found {input.LotNo}"));
        }
        return errors;
    }
    #endregion
}


#nullable enable

namespace SKD.Service;

public class ComponentStationService {

    private SkdContext context;

    public ComponentStationService(SkdContext context) {
        this.context = context;
    }

    /// <summary>
    /// Assign production stations to components
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<MutationResult<SetComponentStationMappingsPayload>> SetComponentStationMappings(
        ComponentStationMappingsInput input
    ) {
        var result = new MutationResult<SetComponentStationMappingsPayload> {
            Errors = await ValidateSetComponentStationMappings(input)
        };
        if (result.Errors.Any()) {
            return result;
        }

        var allComponentStations = await context.ComponentStations
            .Include(t => t.Component)
            .Include(t => t.Station)
            .ToListAsync();
        var allComponents = await context.Components.ToListAsync();
        var allProductionStations = await context.ProductionStations.ToListAsync();

        // remove exsiting component station mappings where component code is in input
        var componentCodes = input.Mappings.Select(m => m.ComponentCode).Distinct().ToList();
        var componentStationsToRemove = allComponentStations
            .Where(cs => componentCodes.Contains(cs.Component.Code))
            .ToList();
        context.ComponentStations.RemoveRange(componentStationsToRemove);

        // add new mappings
        foreach (var mapping in input.Mappings) {
            var component = allComponents.Single(c => c.Code == mapping.ComponentCode);
            var station = allProductionStations.Single(s => s.Code == mapping.StationCode);
            context.ComponentStations.Add(new ComponentStation {
                Component = component,
                Station = station,
                SaveCDCComponent = mapping.SaveCDCComponent,
            });
        }

        // save changes
        await context.SaveChangesAsync();
        result.Payload = new SetComponentStationMappingsPayload {
            Mappings = input.Mappings
        };
        return result;
    }


    private async Task<List<Error>> ValidateSetComponentStationMappings(ComponentStationMappingsInput input) {
        var errors = new List<Error>();
        await Task.Delay(100);

        if (input.Mappings.Count == 0) {
            errors.Add(new Error("No mappings provided"));
            return errors;
        }

        if (
            input.Mappings.Any(m => string.IsNullOrWhiteSpace(m.ComponentCode))
            || input.Mappings.Any(m => string.IsNullOrWhiteSpace(m.StationCode))
        ) {
            errors.Add(new Error("Component or Station code cannot be blank"));
        }

        var inputComponentCodes = input.Mappings.Select(m => m.ComponentCode).Distinct().ToList();
        var foundComponentCodes = await context.Components
            .Where(t => t.RemovedAt == null)
            .Where(c => inputComponentCodes.Contains(c.Code))
            .Select(c => c.Code)
            .ToListAsync();
        var missingComponentCodes = inputComponentCodes.Except(foundComponentCodes).ToList();
        // error if missing
        if (missingComponentCodes.Count > 0) {
            errors.Add(new Error($"Component codes not found: {string.Join(", ", missingComponentCodes)}"));
        }

        // missing production station codes
        var inputStationCodes = input.Mappings.Select(m => m.StationCode).Distinct().ToList();
        var foundStationCodes = await context.ProductionStations
            .Where(t => t.RemovedAt == null)
            .Where(s => inputStationCodes.Contains(s.Code))
            .Select(s => s.Code)
            .ToListAsync();
        var missingStationCodes = inputStationCodes.Except(foundStationCodes).ToList();
        // error if missing
        if (missingStationCodes.Count > 0) {
            errors.Add(new Error($"Station codes not found: {string.Join(", ", missingStationCodes)}"));
        }

        return errors;
    }

    /// <summary>
    /// Remove all component station mappings
    /// </summary>
    /// <returns></returns>
    public async Task<MutationResult<RemoveAllComponentStationMappingsPayload>> RemoveAllComponentStationMappings() {
        var result = new MutationResult<RemoveAllComponentStationMappingsPayload>();
        var componentStations = await context.ComponentStations.ToListAsync();
        context.ComponentStations.RemoveRange(componentStations);
        await context.SaveChangesAsync();
        result.Payload = new RemoveAllComponentStationMappingsPayload {
            RemovedCount = componentStations.Count
        };
        return result;
    }

    /// <summary>
    /// Update kit comoponent stations mappings to match the component station mappings template
    /// Only apply to kits with no ComponentSerial entries
    /// </summary>
    public async Task<MutationResult<SyncKitComponentsPayload>> SyncKitsWithComponentStationMappings() {
        var result = new MutationResult<SyncKitComponentsPayload>();

        // use transaction
        using var transaction = await context.Database.BeginTransactionAsync();

        // Only update kits which have no ComponentSerial entries
        var kits = await context.Kits
            .Include(t => t.Lot).ThenInclude(t => t.Pcv)
            .Where(k => k.RemovedAt == null)
            .Where(k => k.KitComponents.Any(kc => kc.RemovedAt == null))
            .Where(k => !k.KitComponents.Any(kc => kc.ComponentSerials.Any()))
            .ToListAsync();

        var pcvCodes = kits.Select(k => k.Lot.Pcv.Code).Distinct().ToList();

        // load pcv component codes for selected kits
        var pcvComponentCodes = await context.Pcvs
            .Where(t => pcvCodes.Contains(t.Code))
            .Select(t => new {
                PcvCode = t.Code,
                ComponentCodes = t.PcvComponents.Select(t => t.Component.Code)
            }).ToListAsync();

        // load component-stations including component and station
        var componentStations = await context.ComponentStations
            .Include(cs => cs.Component)
            .Include(cs => cs.Station)
            .OrderBy(t => t.Component.Code).ThenBy(t => t.Station.Sequence)
            .Where(t => t.RemovedAt == null)
            .ToListAsync();

        // Bulk delete all kitComponents for selected kits
        var kitIds = kits.Select(t => t.Id);
        await context.KitComponents.Where(t => kitIds.Contains(t.KitId)).ExecuteDeleteAsync();

        // add KitComponent mappings for each selected kit
        foreach (var kit in kits) {
            var pcvForThisKit = pcvComponentCodes.First(t => t.PcvCode == kit.Lot.Pcv.Code);

            var componentStationsForThisPcv = componentStations.Where(t => pcvForThisKit.ComponentCodes.Contains(t.Component.Code)).ToList();
            Console.WriteLine($"{componentStations.Count}    -  {componentStationsForThisPcv.Count}");
            foreach (var cs in componentStationsForThisPcv) {
                kit.KitComponents.Add(new KitComponent {
                    Component = cs.Component,
                    ProductionStation = cs.Station
                });
            };
        }

        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        result.Payload = new SyncKitComponentsPayload(kits.Select(t => t.KitNo).ToList());
        return result;
    }
}


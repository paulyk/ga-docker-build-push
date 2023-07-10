#nullable enable

namespace SKD.Service;

public class ComponentService {
    private readonly SkdContext context;

    public ComponentService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationResult<UpdateComponentPayload>> SaveComponent(ComponentInput input) {
        MutationResult<UpdateComponentPayload> result = new() {
            Errors = await ValidateCreateComponent<ComponentInput>(input)
        };
        if (result.Errors.Any()) {
            return result;
        }

        var component = await context.Components.FirstOrDefaultAsync(t => t.Id == input.Id);

        if (component == null) {
            component = new Component();
            context.Components.Add(component);
        }

        component.Code = input.Code;
        component.Name = input.Name;
        component.ProductionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == input.ProductionStationCode);
        component.ComponentSerialRule = input.DcwsSerialCaptureRule;
        component.DcwsRequired = input.DcwsRequired ?? false;

        Trim.TrimStringProperties<Component>(component);
        // save
        await context.SaveChangesAsync();
        result.Payload = new UpdateComponentPayload(await context.Components.Include(t => t.ProductionStation).FirstAsync(t => t.Code == input.Code));
        return result;
    }

    public async Task<MutationResult<UpdateComponentPayload>> SetDefaultStation(SetDefaultStationInput input) {
        var result = new MutationResult<UpdateComponentPayload> {
            Errors = await ValidateSetDefaultStation(input),
        };

        if (result.Errors.Any()) {
            return result;
        }

        var component = await context.Components.FirstAsync(t => t.Code == input.ComponentCode);
        
        if (input.ProductionStationCode is not null) {
            component.ProductionStation = await context.ProductionStations.FirstAsync(t => t.Code == input.ProductionStationCode);
        } else {
            component.ProductionStation = null;
        }        
        await context.SaveChangesAsync();
        result.Payload = new UpdateComponentPayload(await context.Components.Include(t => t.ProductionStation).FirstAsync(t => t.Code == input.ComponentCode));
        return result;
    }

    public async Task<MutationResult<UpdateComponentPayload>> RemoveComponent(Guid componentId) {
        var component = await context.Components.FirstAsync(t => t.Id == componentId);
        var result = new MutationResult<UpdateComponentPayload> {
            Errors = ValidateRemoveComponent<Component>(component),
        };
        if (result.Errors.Any()) {
            return result;
        }

        component.RemovedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        result.Payload = new UpdateComponentPayload(await context.Components.Include(t => t.ProductionStation).FirstAsync(t => t.Id == componentId));
        return result;
    }

    public async Task<MutationResult<UpdateComponentPayload>> RestoreComponent(Guid componentId) {
        var component = await context.Components.FirstAsync(t => t.Id == componentId);
        var result = new MutationResult<UpdateComponentPayload>() {
            Errors = ValidateRestoreComponent<Component>(component)
        };

        if (result.Errors.Any()) {
            return result;
        }

        component.RemovedAt = null;
        await context.SaveChangesAsync();
        result.Payload = new UpdateComponentPayload(await context.Components.Include(t => t.ProductionStation).FirstAsync(t => t.Id == componentId));
        return result;
    }

    public List<Error> ValidateRemoveComponent<T>(Component component) where T : Component {
        var errors = new List<Error>();

        if (component == null) {
            errors.Add(new Error("", "component not found"));
            return errors;
        }

        if (component.RemovedAt != null) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, "component already removed"));
        }
        return errors;
    }

    public async Task<List<Error>> ValidateSetDefaultStation(SetDefaultStationInput input) {
        var errors = new List<Error>();

        var component = await context.Components.FirstOrDefaultAsync(t => t.Code == input.ComponentCode);
        if (component is null) {
            errors.Add(new Error($"Component not found: {input.ComponentCode}"));
        } else if (component.RemovedAt is not null) {
            errors.Add(new Error($"Component {input.ComponentCode} marked removed "));
        }

        if (input.ProductionStationCode is not null) {
            var station = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == input.ProductionStationCode);
            if (station == null) {
                errors.Add(new Error($"Production station not found: {input.ProductionStationCode}"));
            } else if (station.RemovedAt is not null) {
                errors.Add(new Error($"Production station removed: {input.ProductionStationCode}"));
            }
        }
        return errors;
    }
    public List<Error> ValidateRestoreComponent<T>(Component component) where T : Component {
        var errors = new List<Error>();

        if (component == null) {
            errors.Add(new Error("", "component not found"));
            return errors;
        }

        if (component.RemovedAt == null) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, "component already active"));
        }
        return errors;
    }

    public async Task<List<Error>> ValidateCreateComponent<T>(ComponentInput input) where T : ComponentInput {
        var errors = new List<Error>();

        Component? existingComponent = null;
        if (input.Id.HasValue) {
            existingComponent = await context.Components.FirstOrDefaultAsync(t => t.Id == input.Id.Value);
            if (existingComponent == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Id, $"component not found: {input.Id}"));
                return errors;
            }
        }

        // code
        if (input.Code.Trim().Length == 0) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
        } else if (input.Code.Length > EntityFieldLen.Component_Code) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.Component_Code} characters "));
        }

        // name
        if (input.Name.Trim().Length == 0) {
            errors.Add(ErrorHelper.Create<T>(t => t.Name, "name required"));
        } else if (input.Code.Length > EntityFieldLen.Component_Name) {
            errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded name max length of {EntityFieldLen.Component_Name} characters "));
        }

        // duplicate code
        if (existingComponent != null) {
            if (await context.Components.AnyAsync(t => t.Id != input.Id && t.Code == input.Code)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
            }
        } else {
            // adding a new component, so look for duplicate
            if (await context.Components.AnyAsync(t => t.Code == input.Code)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
            }
        }

        // duplicate name
        if (existingComponent != null) {
            if (await context.Components.AnyAsync(t => t.Id != input.Id && t.Name == input.Name)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "duplicate name"));
            }
        } else {
            // adding a new component, so look for duplicate
            if (await context.Components.AnyAsync(t => t.Name == input.Name)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate name"));
            }
        }

        // production station
        if (!String.IsNullOrWhiteSpace(input.ProductionStationCode)) {
            var station = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == input.ProductionStationCode);
            if (station is null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"Production station not found {input.ProductionStationCode}"));
            } else if (station.RemovedAt != null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"Production station removed found {input.ProductionStationCode}"));
            }
        }

        return errors;
    }
}

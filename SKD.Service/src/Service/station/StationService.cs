#nullable enable

namespace SKD.Service;

public class StationService {
    private readonly SkdContext context;

    public StationService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationResult<UpdateStationPayload>> SaveStation(StationInput input) {
        MutationResult<UpdateStationPayload> result = new() {
            Errors = await ValidateStation(input)
        };

        if (result.Errors.Any()) {
            return result;
        }

        var station = await context.ProductionStations.FirstOrDefaultAsync(t => t.Id == input.Id);

        if (station is null) {
            station = new ProductionStation();
            context.ProductionStations.Add(station);
        }

        station.Code = input.Code;
        station.Name = input.Name;
        station.Sequence = input.Sequence;

        Trim.TrimStringProperties<ProductionStation>(station);

        // save
        await context.SaveChangesAsync();

        result.Payload = new UpdateStationPayload(station);
        return result;
    }

    public async Task<List<Error>> ValidateStation(StationInput input) {
        var errors = new List<Error>();

        if (input.Id is not null) {
            if (!await context.ProductionStations.AnyAsync(t => t.Id == input.Id))
                errors.AddError($"Station not found for ID {input.Id}");
        }

        if (String.IsNullOrWhiteSpace(input.Code)) {
            errors.AddError("code requred");
        } else if (input.Code.Length > EntityFieldLen.ProductionStation_Code) {
            errors.AddError($"exceeded code max length of {EntityFieldLen.ProductionStation_Code} characters ");
        }

        if (String.IsNullOrWhiteSpace(input.Name)) {
            errors.AddError("name required");
        } else if (input.Code.Length > EntityFieldLen.ProductionStation_Name) {
            errors.AddError($"exceeded name max length of {EntityFieldLen.ProductionStation_Name} characters ");
        }

        if (await context.ProductionStations.AnyAsync(t => t.Id != input.Id && t.Code == input.Code)) {
            errors.AddError("duplicate code");
        }

        if (await context.ProductionStations.AnyAsync(t => t.Id != input.Id && t.Name == input.Name)) {
            errors.AddError("duplicate name");
        }

        return errors;
    }
}


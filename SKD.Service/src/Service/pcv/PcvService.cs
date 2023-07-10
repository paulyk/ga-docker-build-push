#nullable enable

namespace SKD.Service;
public class PcvService {
    private readonly SkdContext context;

    public PcvService(SkdContext ctx) {
        this.context = ctx;
    }

    #region SavePcv
    public async Task<MutationResult<SavePcvPayload>> SavePCV(SavePcvInput input) {
        var payload = new MutationResult<SavePcvPayload> {
            Errors = await ValidateSavePcv(input)
        };
        if (payload.Errors.Any()) {
            return payload;
        }
        // get exiting pcv or create new
        var pcv = await context.Pcvs
            .Include(t => t.PcvModel)
            .Include(t => t.PcvSubmodel)
            .Include(t => t.PcvSeries)
            .Include(t => t.PcvEngine)
            .Include(t => t.PcvTransmission)
            .Include(t => t.PcvDrive)
            .Include(t => t.PcvPaint)
            .Include(t => t.PcvTrim)
            .Include(t => t.PcvComponents).ThenInclude(t => t.Component)
            .FirstOrDefaultAsync(t => t.Code == input.PcvCode);

        if (pcv == null) {
            pcv = new PCV {
                Code = input.PcvCode,
                CreatedAt = DateTime.UtcNow,
            };
            context.Pcvs.Add(pcv);
        }

        // set properties 
        pcv.ModelYear = input.ModelYear.ToString();
        if (!String.IsNullOrWhiteSpace(input.PcvModel)) {
            pcv.PcvModel = await EnsureCategory<PcvModel>(input.PcvModel);
        }
        if (!String.IsNullOrWhiteSpace(input.PcvSubmodel)) {
            pcv.PcvSubmodel = await EnsureCategory<PcvSubmodel>(input.PcvSubmodel);
        }
        if (!String.IsNullOrWhiteSpace(input.PcvSeries)) {
            pcv.PcvSeries = await EnsureCategory<PcvSeries>(input.PcvSeries);
        }
        if (!String.IsNullOrWhiteSpace(input.PcvEngine)) {
            pcv.PcvEngine = await EnsureCategory<PcvEngine>(input.PcvEngine);
        }
        if (!String.IsNullOrWhiteSpace(input.PcvTransmission)) {
            pcv.PcvTransmission = await EnsureCategory<PcvTransmission>(input.PcvTransmission);
        }
        if (!String.IsNullOrWhiteSpace(input.PcvDrive)) {
            pcv.PcvDrive = await EnsureCategory<PcvDrive>(input.PcvDrive);
        }
        if (!String.IsNullOrWhiteSpace(input.PcvPaint)) {
            pcv.PcvPaint = await EnsureCategory<PcvPaint>(input.PcvPaint);
        }
        if (!String.IsNullOrWhiteSpace(input.PcvTrim)) {
            pcv.PcvTrim = await EnsureCategory<PcvTrim>(input.PcvTrim);
        }

        // set legacy property
        pcv.Series = input.PcvSeries;
        pcv.Model = input.PcvModel;
        pcv.Description = $"{input.PcvModel}, {input.PcvSeries}, {input.PcvEngine}, {input.PcvPaint}";

        // if input.ComponentCodes not provided and input has no PcvComponentCodes, get components from thef first  PCV with the same pcvSubmodel.Code
        if (input.ComponentCodes.Count == 0 && !pcv.PcvComponents.Any()) {
            var pcvSubmodel = await context.PcvSubmodels.FirstOrDefaultAsync(t => t.Code == input.PcvSubmodel);
            if (pcvSubmodel != null) {
                var firstPcv = await context.Pcvs
                    .Include(t => t.PcvComponents).ThenInclude(t => t.Component)
                    .FirstOrDefaultAsync(t => t.PcvSubmodel != null && t.PcvSubmodel.Code == pcvSubmodel.Code);
                if (firstPcv != null) {
                    input.ComponentCodes = firstPcv.PcvComponents.Select(t => t.Component.Code).ToList();
                }
            }
        }

        List<Component> selectedComponents = input.ComponentCodes is null ? new List<Component>() :
            await context.Components.Where(t => input.ComponentCodes.Contains(t.Code)).ToListAsync();

        // add  to pcv.PcvComponents  if not in PCV.PcvComponents else remve    
        var pcvComponents = pcv.PcvComponents.ToList();
        foreach (var pcvComponent in pcvComponents) {
            if (!selectedComponents.Contains(pcvComponent.Component)) {
                pcv.PcvComponents.Remove(pcvComponent);
            }
        }
        foreach (var component in selectedComponents) {
            if (!pcvComponents.Any(t => t.Component == component)) {
                pcv.PcvComponents.Add(new PcvComponent {
                    Component = component
                });
            }
        }
        
        // save
        await context.SaveChangesAsync();

        payload.Payload = new SavePcvPayload(await GetFullPcv(pcv.Id));
        return payload;
    }

    private async Task<T> EnsureCategory<T>(string name) where T : EntityBase, ICategory, new() {
        var categoryEntry = await context.Set<T>().FirstOrDefaultAsync(t => t.Code == name);
        if (categoryEntry is null) {
            categoryEntry = new T {
                Code = name,
                Name = name
            };
            context.Set<T>().Add(categoryEntry);
        }
        return categoryEntry;
    }

    public async Task<List<Error>> ValidateSavePcv(SavePcvInput input) {
        var errors = new List<Error>();
        await Task.Delay(10);

        // code / code length
        if (String.IsNullOrWhiteSpace(input.PcvCode)) {
            errors.AddError("PCV Code required");
        } else if (input.PcvCode.Trim().Length != EntityFieldLen.Pcv_Code) {
            errors.AddError($"PCV Code must be {EntityFieldLen.Pcv_Code} characters");
        }

        // already exists
        // if (await context.Pcvs.AnyAsync(t => t.Code == input.PcvCode)) {
        //     errors.AddError($"PCV already esists {input.PcvCode}");
        // }

        // model year
        var minYear = DateTime.UtcNow.AddYears(-3).Year;
        var maxYear = DateTime.UtcNow.AddYears(2).Year;
        if (input.ModelYear < minYear || input.ModelYear > maxYear) {
            errors.AddError($"Model year out of range. {minYear} to {maxYear}");
        }

        // component codes not found in Components
        var componentCodesInDB = await context.Components.Where(t => t.RemovedAt == null).Select(t => t.Code).ToListAsync();
        var componentCodesNotInComponentsTable = input.ComponentCodes.Except(componentCodesInDB).ToList();
        if (componentCodesNotInComponentsTable.Any()) {
            errors.AddError($"Component code(s) not found in system: {String.Join(", ", componentCodesNotInComponentsTable)}");
        }

        return errors;

        // ICollection<Error> ValidateCategoryCodeAndName(string categoryName, ICategory entry) {
        //     var errors = new List<Error>();
        //     return String.IsNullOrWhiteSpace(entry.Code) || String.IsNullOrWhiteSpace(entry.Name)
        //         ? new List<Error> { new Error($"{categoryName} code and name requiured") }
        //         : new List<Error>();
        // }
    }
    #endregion

    #region DeletePcv
    public async Task<MutationResult<DeletePcvPayload>> DeletePCV(DeletePcvInput input) {
        var result = new MutationResult<DeletePcvPayload> {
            Errors = await ValidateDeletePcv(input)
        };
        if (result.Errors.Any()) {
            return result;
        }

        var pcv = await context.Pcvs
            .Include(t => t.PcvModel)
            .Include(t => t.PcvModel)
            .Include(t => t.PcvSubmodel)
            .Include(t => t.PcvSeries)
            .Include(t => t.PcvEngine)
            .Include(t => t.PcvTransmission)
            .Include(t => t.PcvDrive)
            .Include(t => t.PcvPaint)
            .Include(t => t.PcvTransmission)
            .Include(t => t.PcvComponents).FirstAsync(t => t.Code == input.PcvCode);

        result.Payload = new DeletePcvPayload {
            PcvCode = pcv.Code,
            PcvModel = pcv.PcvModel?.Code ?? ""
        };

        context.Pcvs.Remove(pcv);
        await RemovePcvMetaEntity(pcv.PcvModel);
        await RemovePcvMetaEntity(pcv.PcvSubmodel);
        await RemovePcvMetaEntity(pcv.PcvSeries);
        await RemovePcvMetaEntity(pcv.PcvEngine);
        await RemovePcvMetaEntity(pcv.PcvTransmission);
        await RemovePcvMetaEntity(pcv.PcvDrive);
        await RemovePcvMetaEntity(pcv.PcvPaint);
        await RemovePcvMetaEntity(pcv.PcvTrim);

        context.PcvComponents.RemoveRange(pcv.PcvComponents);

        await context.SaveChangesAsync();
        return result;

        async Task RemovePcvMetaEntity<T>(T? entity) where T : EntityBase, IPcvMetaCategory, new() {
            if (entity != null) {
                var e = await context.Set<T>()
                    .Include(t => t.Pcvs)
                    .FirstAsync(t => t.Code == entity.Code);

                if (e.Pcvs.Count() == 1) {
                    context.Set<T>().Remove(e);
                }
            }
        }
    }
    public async Task<List<Error>> ValidateDeletePcv(DeletePcvInput input) {
        var errors = new List<Error>();

        var pcv = await context.Pcvs
            .Include(t => t.Lots)
            .FirstOrDefaultAsync(t => t.Code == input.PcvCode);

        if (pcv is null) {
            errors.AddError($"PCV not found for {input.PcvCode}");
            return errors;
        }

        if (pcv.Lots.Any()) {
            var lotNumber = pcv.Lots.Select(t => t.LotNo).First();
            errors.AddError($"PCV already associated with lot {lotNumber} ");
            return errors;
        }
        return errors;
    }
    #endregion

    async Task<PCV> GetFullPcv(Guid id) => await context.Pcvs
        .Include(t => t.PcvModel)
        .Include(t => t.PcvSubmodel)
        .Include(t => t.PcvSeries)
        .Include(t => t.PcvEngine)
        .Include(t => t.PcvTransmission)
        .Include(t => t.PcvDrive)
        .Include(t => t.PcvPaint)
        .Include(t => t.PcvTrim)
        .Include(t => t.PcvComponents).ThenInclude(t => t.Component)
        .FirstAsync(t => t.Id == id);
}
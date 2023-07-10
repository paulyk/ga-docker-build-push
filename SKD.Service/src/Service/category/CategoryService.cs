#nullable enable

namespace SKD.Service;

public class CategorySerivce<T> where T : EntityBase, ICategory, new() {

    private SkdContext context;
    public CategorySerivce(SkdContext context) {
        this.context = context;
    }

    public async Task<MutationResult<UpdateCategoryPayload>> CreateCategory(CreateCategoryInput input) {
        var result = new MutationResult<UpdateCategoryPayload> {
            Payload = null,
            Errors = await ValidateCreateCatgory(input)
        };
        if (result.Errors.Any()) {
            return result;
        }

        var category = new T() {
            Code = input.Code,
            Name = input.Name
        };
        context.Set<T>().Add(category);

        await context.SaveChangesAsync();
        result.Payload = UpdateCategoryPayload.CreateUpdateCategoryPayload(category);

        return result;
    }

    public async Task<List<Error>> ValidateCreateCatgory(ICategory input, Guid existingID = default) {
        var setContext = context.Set<T>();
        var errors = new List<Error>();

        if (String.IsNullOrWhiteSpace(input.Code)) {
            errors.AddError($"Code required");
        }
        if (String.IsNullOrWhiteSpace(input.Code)) {
            errors.AddError($"Name required");
        }
        if (errors.Any()) {
            return errors;
        }

        if (await setContext.AnyAsync(x => x.Code == input.Code && x.Id != existingID)) {
            errors.AddError($"Duplicate code: {input.Code}");
        }

        if (await setContext.AnyAsync(x => x.Name == input.Name && x.Id != existingID)) {
            errors.AddError($"Duplicate name: {input.Name}");
        }

        return errors;
    }

    public async Task<MutationResult<UpdateCategoryPayload>> UpdateCategory(UpdateCategoryInput input) {
        var result = new MutationResult<UpdateCategoryPayload> {
            Payload = null,
            Errors = await ValidateCreateCatgory(input, existingID: input.Id)
        };
        if (result.Errors.Any()) {
            return result;
        }

        var category = await context.Set<T>().FirstAsync(t => t.Id == input.Id);
        category.Code = input.Code;
        category.Name = input.Name;

        await context.SaveChangesAsync();
        result.Payload = UpdateCategoryPayload.CreateUpdateCategoryPayload(category);

        return result;
    }
}

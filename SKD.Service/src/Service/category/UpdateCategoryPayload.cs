namespace SKD.Service;

public class UpdateCategoryPayload {
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public static UpdateCategoryPayload CreateUpdateCategoryPayload<T>(T input) where T : ICategory, IEntity {
        return new UpdateCategoryPayload {
            Id = input.Id,
            Code = input.Code,
            Name = input.Name,
            CreatedAt = input.CreatedAt,
            RemovedAt = input.RemovedAt
        };
    }
}


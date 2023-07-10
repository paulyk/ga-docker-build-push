namespace SKD.Service;

public class CreateCategoryInput: ICategory {
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

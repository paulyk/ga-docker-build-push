#nullable enable
namespace SKD.Service;

public class Error {

    public Error() { }

    public Error(string message) {
        Path = new List<string>();
        Message = message;
    }

    public Error(string path, string message) {
        Path = new List<string>() { path };
        Message = message;
    }

    public List<string> Path { get; set; } = new List<string>();
    public string Message { get; set; } = "";
}

public static class ErrorExtension {
    public static void AddError(this List<Error> errors, string message) {
        errors.Add(new Error(message));
    }
}
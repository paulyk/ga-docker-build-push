namespace SKD.KitStatusFeed;

public class KitStatusFeedResponse<T> {
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public KitStatusFeedErrorResponse? Error { get; set; }
}


public class KitStatusFeedErrorResponse {
    public object Result { get; set; } = "";
    public string MoreInformation { get; set; } = "";
    public string HttpMessage { get; set; } = "";
    public string HttpCode { get; set; } = "";
    public ErrorObject Error { get; set; } = new();
}

public class ErrorObject {
    public List<DataError> DataErrors { get; set; } = new();
    public string ErrorCode { get; set; } = "";
    public List<string> Messages { get; set; } = new();
    public object? Attributes { get; set; }
}

public class DataError {
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Message { get; set; } = "";
    public string Value { get; set; } = "";
}

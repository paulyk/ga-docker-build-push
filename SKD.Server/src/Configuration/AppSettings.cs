/// <summary>
/// SKD Server app settings section
/// </summary>
public  class AppSettings {
    public int ExecutionTimeoutSeconds { get; set; } = 30;
    public string DcwsServiceAddress { get; set; } = "";
    public bool AllowGraphqlIntrospection { get; set; }
    public string KitStatusFeedUrl { get; set; } = "";
}


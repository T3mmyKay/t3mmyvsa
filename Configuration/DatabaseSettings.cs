namespace T3mmyvsa.Configuration;

public class DatabaseSettings
{
    /// <summary>
    /// Database provider: "mssql" or "mysql"
    /// </summary>
    public string DBProvider { get; set; } = "mssql";

    /// <summary>
    /// Connection string for the database
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}

namespace TestMate.API.Settings;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string DevicesCollectionName { get; set; } = null!;

    public string UsersCollectionName { get; set; } = null!;

    public string DevelopersCollectionName { get; set; } = null!;
    public string TestRequestsCollectionName { get; set; } = null!;
}
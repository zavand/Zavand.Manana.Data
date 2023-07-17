namespace Zavand.Manana.Data.StorageSchema;

public abstract class StorageSchemaChange:IStorageSchemaChange
{
    public const string TZ_America_New_York = "America/New_York";
    public string Version { get; set; }

    StorageSchemaChange(string version)
    {
        Version = version;
    }

    protected StorageSchemaChange(DateTime date, string description):this($"{date:yyyy-MM-dd HH:mm} - {description}")
    {

    }

    protected StorageSchemaChange(int year, int month, int day, int hour, int min,string timezoneId, string description)
        : this(TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, day, hour, min, 0), TimeZoneInfo.FindSystemTimeZoneById(timezoneId)), description)
    {
    }

    public abstract Task ApplyAsync(IStorage dataStorage);
}
namespace Zavand.Manana.Data.StorageSchema;

public interface IStorageSchemaChange
{
    string Version { get; }
    Task ApplyAsync(IStorage dataStorage);
}
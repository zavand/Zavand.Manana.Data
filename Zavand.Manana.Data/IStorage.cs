using Zavand.Manana.Data.StorageSchema;

namespace Zavand.Manana.Data;

public interface IStorage : IDisposable, IAsyncDisposable
{
    void SetConnectionString(string connectionString);
    void ChangeDatabase(string databaseName);
    Task OpenAsync();
    Task CloseAsync();
    public Task<T> QueryAsync<T>(T query) where T : IStorageQuery;
    
    /// <summary>
    /// Creates database, schema, apply schema changes
    /// </summary>
    /// <returns></returns>
    Task InitAsync();

    Task CreateDatabaseAsync();
    Task ApplyStorageSchemaAsync();

    string StorageSchemaName { get; set; }
    IStorageSchemaChange[] GetSchemaStorageChanges();
}

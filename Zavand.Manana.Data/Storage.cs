using System.Data.Common;
using Zavand.Manana.Data.StorageSchema;

namespace Zavand.Manana.Data;

public abstract class Storage : IStorage
{
    protected string _connectionString;
    protected DbConnection _connection;

    public virtual void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
    }

    public abstract void ChangeDatabase(string databaseName);

    protected abstract DbConnection CreateConnection();
    protected abstract DbCommand CreateCommand();

    public virtual  async Task OpenAsync()
    {
        await CloseAsync();

        _connection = CreateConnection();
        await _connection.OpenAsync();
    }

    public virtual async Task CloseAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection = null;
        }
    }

    public void Dispose()
    {
        CloseAsync().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }

    public async Task<T> QueryAsync<T>(T query) where T:IStorageQuery
    {
        await using var cmd = CreateCommand();

        cmd.Connection = _connection;
        cmd.CommandText = query.GetSqlQuery();

        var pp = query.GetParams();
        foreach (var p in pp.Keys)
        {
            AddParameter(cmd.Parameters, p, pp[p]);
        }

        await using var r = await cmd.ExecuteReaderAsync();

        var resultSets = new Queue<ResultSet>();
        do
        {
            var rs = new ResultSet
            {
                Schema = (await r.GetColumnSchemaAsync()).ToArray()
            };
            var records = new List<object[]>();
            while (await r.ReadAsync())
            {
                var columns = new List<object>();
                for (var i = 0; i < r.FieldCount; i++)
                {
                    columns.Add(r[i]);
                }
                records.Add(columns.ToArray());
            }

            rs.Records = records.ToArray();
            
            resultSets.Enqueue(rs);
        } while (await r.NextResultAsync());
        
        query.Read(resultSets);

        return query;
    }

    public async Task InitAsync()
    {
        try
        {
            _connection = CreateConnection();
            await _connection.OpenAsync();
            await CreateDatabaseAsync();
            await ApplyStorageSchemaAsync();
        }
        finally
        {
            await CloseAsync();
        }
    }

    public Task ApplyStorageSchemaAsync()
    {
        return ApplyStorageSchemaAsync(StorageSchemaName,GetSchemaStorageChanges());
    }

    protected virtual async Task ApplyStorageSchemaAsync(string schemaName, IStorageSchemaChange[] changes)
    {
        if (String.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("SchemaName must not be empty", nameof(schemaName));

        var appliedChanges = new List<string>();
        await QueryAsync(new CustomStorageQuery(q =>
            {
                var schemaNameParam = q.AppendParam(schemaName);
                return @$"
create table if not exists schema_version
(
	name varchar(100) not null,
	version varchar(100) not null,
	date datetime not null,
    primary key (`name`,`version`)
);

select name,version,date from schema_version where name={schemaNameParam};
";
            },
            (resultSets, q) =>
            {
                ColumnValueReader.ReadAll(resultSets.Dequeue(), (columns, reader) =>
                {
                    appliedChanges.Add(reader.Get<string>("version", columns, q.Errors));
                });
            }));
        var toApply = changes
            .Where(m => !appliedChanges.Contains(m.Version))
            .OrderBy(m => m.Version)
            .ToArray();
        if (toApply.Any())
        {
            await QueryAsync(new CustomStorageQuery(q =>
            {
                var schemaNameParam = q.AppendParam(schemaName);
                var currentDateParam = q.AppendParam(DateTime.UtcNow);
                return @$"
insert into schema_version (name,version,date) values {String.Join(",\n", toApply.Select(m => $"({schemaNameParam},'{m.Version}',{currentDateParam})"))};
";
            }));
            var successfullyAppliedChanges = new List<string>();
            try
            {
                foreach (var schemaChange in toApply)
                {
                    await schemaChange.ApplyAsync(this);
                    successfullyAppliedChanges.Add(schemaChange.Version);
                }
            }
            finally
            {
                var notAppliedChanged = toApply
                    .Where(m => !successfullyAppliedChanges.Contains(m.Version))
                    .ToArray();
                await QueryAsync(new CustomStorageQuery(q =>
                {
                    var schemaNameParam = q.AppendParam(schemaName);
                    return @$"
delete from schema_version where name={schemaNameParam} and version in ({String.Join(",", notAppliedChanged.Select(m => $"'{m.Version}'"))});
";
                }));
            }
        }
    }

    public string StorageSchemaName { get; set; }

    public virtual IStorageSchemaChange[] GetSchemaStorageChanges()
    {
        return Array.Empty<IStorageSchemaChange>();
    }

    public virtual Task CreateDatabaseAsync()
    {
        return Task.CompletedTask;
    }


    protected abstract void AddParameter(DbParameterCollection parameters, string parameterName, object value);
}
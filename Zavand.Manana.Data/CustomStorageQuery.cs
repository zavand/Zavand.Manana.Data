namespace Zavand.Manana.Data;

public class CustomStorageQuery : StorageQuery
{
    private readonly GetQueryDelegate _query;
    private readonly ReadDelegate _read;

    public delegate string GetQueryDelegate(CustomStorageQuery storageQuery);

    public delegate void ReadDelegate(Queue<ResultSet> resultSets, CustomStorageQuery storageQuery);

    public CustomStorageQuery(GetQueryDelegate query, ReadDelegate read = null)
    {
        _query = query;
        _read = read;
    }

    public override string GetSqlQuery()
    {
        return _query(this);
    }

    public override void Read(Queue<ResultSet> resultSets)
    {
        _read?.Invoke(resultSets, this);
    }
}
namespace Zavand.Manana.Data;

public abstract class StorageQuery : IStorageQuery
{
    private readonly Dictionary<string,object> _params = new ();
    public IList<string> Errors { get; } = new List<string>();
    public string Name { get; set; }
    public string ParamPrefix { get; set; } = String.Empty;

    // public StorageQuery()
    // {
    //     Name = Guid.NewGuid().ToString();
    // }
    public abstract string GetSqlQuery();

    private readonly HashSet<string> _paramNames = new();

    public virtual void Read(Queue<ResultSet> resultSets)
    {
        
    }

    public Dictionary<string, object> GetParams()
    {
        return _params;
    }

    string GetFullParamName(int counter, string paramName)
    {
        var fullParamName = $"@p{ParamPrefix}_{counter}{(String.IsNullOrWhiteSpace(paramName)?"":$"_{paramName}")}";
        return fullParamName;
    }

    protected void AppendFullParam(string fullParamName, object value)
    {
        _paramNames.Add(fullParamName);
        _params.Add(fullParamName, value);
    }

    public string AppendParam(object value, string paramName=null)
    {
        int counter = 0;
        
        var fullParamName = $"@{ParamPrefix}";
        do
        {
            counter++;
            fullParamName = GetFullParamName(counter, paramName);
        } while (_paramNames.Contains(fullParamName));

        _paramNames.Add(fullParamName);
        _params.Add(fullParamName, value);
        
        return fullParamName;
    }
}
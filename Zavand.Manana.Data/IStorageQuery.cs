namespace Zavand.Manana.Data;

public interface IStorageQuery
{
    /// <summary>
    /// Storage Query name. Optional. Can be used to distinguish queries.
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Parameter prefix. Used to create unique parameter in SQL query if multiple queries used at once.
    /// </summary>
    string ParamPrefix { get; set; }
    
    string GetSqlQuery();
    Dictionary<string, object> GetParams();
    void Read(Queue<ResultSet> resultSets);
    IList<string> Errors { get; }
}
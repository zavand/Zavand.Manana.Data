namespace Zavand.Manana.Data;

public class MultiStorageQuery : StorageQuery
{
    public IStorageQuery[] Queries { get; }

    public MultiStorageQuery(IEnumerable<IStorageQuery> queries)
    {
        Queries = queries.ToArray();
        for (var i = 0; i < Queries.Length; i++)
        {
            Queries[i].ParamPrefix = i.ToString();
        }
        
    }
    
    public override string GetSqlQuery()
    {
        var q = String.Join(";\n", Queries.Select(m => m.GetSqlQuery()));
        foreach (var q1 in Queries)
        {
            var pp = q1.GetParams();
            foreach (var k in pp.Keys)
            {
                AppendFullParam(k, pp[k]);
            }
        }
        return q;
    }

    public override void Read(Queue<ResultSet> resultSets)
    {
        foreach (var q in Queries)
        {
            q.Read(resultSets);
        }
    }
}
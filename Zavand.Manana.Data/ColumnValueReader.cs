using System.Data.Common;
using System.Reflection;

namespace Zavand.Manana.Data;

public class ColumnValueReader
{
    private readonly Dictionary<string,int> _indexes;

    public ColumnValueReader(IEnumerable<DbColumn> columns)
    {
        _indexes = columns
            .Select((m, i) => new {m,i})
            .ToDictionary(m=>m.m.ColumnName.ToLower(), m=> m.i);
    }

    public object Get(Type type, string columnName, object[] values, IList<string> errors)
    {
        var index = _indexes.ContainsKey(columnName.ToLower())
            ? _indexes[columnName.ToLower()]
            : -1;
        if (index < 0)
        {
            errors?.Add($"Column '{columnName}' not found");

            return default;
        }

        var v = values[index]; 
        return v == DBNull.Value ? null : v;
    }
    
    public T Get<T>(string columnName, object[] values, IList<string> errors)
    {
        return (T)Get(typeof(T), columnName, values, errors);
    }
    
    public static T[] ReadAll<T>(ResultSet rs, IList<string> errors) where T : new()
    {
        var rr = new List<T>();
        var t = typeof(T);
        var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var cvr = new ColumnValueReader(rs.Schema);
        foreach (var r in rs.Records)
        {
            var o = new T();
            
            foreach (var p in properties)
            {
                var columnName = p.Name;
                var v = cvr.Get(p.PropertyType, columnName, r, errors);
                p.SetValue(o, v == DBNull.Value ? null : v);
            }
            
            rr.Add(o);
        }
        return rr.ToArray();
    }

    public delegate void ReadAllDelegate(object[] columns, ColumnValueReader columnValueReader);
    public static void ReadAll(ResultSet rs, ReadAllDelegate read)
    {
        var cvr = new ColumnValueReader(rs.Schema);
        foreach (var r in rs.Records)
        {
            read(r, cvr);
        }
    }
}
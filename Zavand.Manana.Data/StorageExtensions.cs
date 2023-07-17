using System.Data.Common;

namespace Zavand.Manana.Data;

public static class StorageExtensions
{
    public static string GenerateClass(this DbColumn[] schema, string className = null)
    {
        var props = String.Join("\n", schema.Select(m =>
        {
            var n = m.AllowDBNull == true ? "?" : "";

            var typesNullableByDefault = new[] { typeof(string) };
            if (typesNullableByDefault.Any(t => t == m.DataType))
            {
                n = "";
            }

            return $"    public {m.DataType}{n} {m.ColumnName} {{ get; set; }}";
        }));
        var classType = $@"public class {(String.IsNullOrWhiteSpace(className) ? "Class" : className)}
{{
{props}
}}";
        return classType;
    }
}
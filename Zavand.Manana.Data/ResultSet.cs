using System.Data.Common;

namespace Zavand.Manana.Data;

public class ResultSet
{
    public DbColumn[] Schema { get; set; }
    public object[][] Records { get; set; } = Array.Empty<object[]>();
}
using System.Data;

namespace DapperExtensions.Mapper
{
    public class Parameter
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public object Value { get; set; }
        public DbType? DbType { get; set; }
        public ParameterDirection? ParameterDirection { get; set; }
        public int? Size { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }
    }
}

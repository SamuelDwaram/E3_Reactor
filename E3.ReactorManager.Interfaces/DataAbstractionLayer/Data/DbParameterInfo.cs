using System.Data;

namespace E3.ReactorManager.Interfaces.DataAbstractionLayer.Data
{
    public class DbParameterInfo
    {
        public DbParameterInfo(string name, object value, DbType dbType)
        {
            Name = name;
            Value = value;
            DbType = dbType;
        }

        public string Name { get; set; }

        public object Value { get; set; }

        public DbType DbType { get; set; }
    }
}

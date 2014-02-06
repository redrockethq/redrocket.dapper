using System.Reflection;

namespace RedRocket.Dapper.Core.Mapper
{
    public class DtoMapper<T> : Mapper<T>
    {
        public DtoMapper()
        {
            var type = typeof(T);

            var tableAttribute = type.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
            var tableName = string.Empty;
            if (tableAttribute != null)
                tableName = tableAttribute.Name;
            
            Table(tableName);
            AutoMap();
        }
    }
}

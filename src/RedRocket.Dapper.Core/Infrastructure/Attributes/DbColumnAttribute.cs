using System;
using System.Reflection;

namespace RedRocket.Dapper.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DbColumnAttribute : Attribute
    {
        /// <summary>
        /// Database Column Name
        /// </summary>
        public string Name { get; private set; }
        public string Schema { get; set; }

        public DbColumnAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DbPrimaryKeyAttribute : DbColumnAttribute
    {
        public DbPrimaryKeyAttribute(string name)
            : base(name) { }
    }

    public static class DbColumnAttributeExtensions
    {
        public static string GetColumnName(this Type type)
        {
            var column = type.GetCustomAttribute<DbColumnAttribute>();
            return column != null ? column.Name : string.Empty;
        }
    }
}
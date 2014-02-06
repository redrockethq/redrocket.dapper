#region

using System;
using System.Reflection;

#endregion

namespace RedRocket.Dapper.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        readonly string _name;
        readonly string _schema;

        public ColumnAttribute(string name)
        {
            _name = name;
        }

        public ColumnAttribute(string name, string schema)
            : this(name)
        {
            _schema = schema;
        }

        /// <summary>
        ///     Database Column Name
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        public string Schema
        {
            get { return _schema; }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PrimaryKeyAttribute : ColumnAttribute
    {
        public PrimaryKeyAttribute(string name)
            : base(name) {}
    }

    public static class DbColumnAttributeExtensions
    {
        public static string GetColumnName(this Type type)
        {
            var column = type.GetCustomAttribute<ColumnAttribute>();
            return column != null ? column.Name : string.Empty;
        }
    }
}
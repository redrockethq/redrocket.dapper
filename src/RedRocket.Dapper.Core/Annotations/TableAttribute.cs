#region

using System;
using System.Reflection;
using FlitBit.Dto;

#endregion

namespace RedRocket.Dapper.Core
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableAttribute : DTOAttribute
    {
        readonly string _name;

        public TableAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     Database Table Name
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
    }

    public static class DbTableAttributeExtensions
    {
        public static string GetTableName(this Type type)
        {
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            return tableAttribute != null ? tableAttribute.Name : string.Empty;
        }
    }
}
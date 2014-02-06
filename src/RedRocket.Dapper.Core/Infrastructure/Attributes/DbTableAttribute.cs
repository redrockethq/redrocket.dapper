using System;
using System.Reflection;
using FlitBit.Dto;

namespace RedRocket.Dapper.Core
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DbTableAttribute : DTOAttribute
    {
        /// <summary>
        /// Database Table Name
        /// </summary>
        public string Name { get; private set; }

        public DbTableAttribute(string name)
        {
            Name = name;
        }
    }

    public static class DbTableAttributeExtensions
    {
        public static string GetTableName(this Type type)
        {
            var tableAttribute = type.GetCustomAttribute<DbTableAttribute>();
            return tableAttribute != null ? tableAttribute.Name : string.Empty;
        }
    }
}
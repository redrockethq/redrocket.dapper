#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlitBit.IoC.Meta;
using RedRocket.Dapper.Core.Mapper;
using RedRocket.Dapper.Core.Sql.Predicates;

#endregion

namespace RedRocket.Dapper.Core.Sql
{
    public interface ISqlGenerator
    {
        IRedRocketDapperConfiguration Configuration { get; }

        string Select(IMapper map, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters);
        string SelectPaged(IMapper map, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters);
        string SelectSet(IMapper map, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters);
        string Count(IMapper map, IPredicate predicate, IDictionary<string, object> parameters);

        string Insert(IMapper map);
        string Update(IMapper map, IPredicate predicate, IDictionary<string, object> parameters);
        string Delete(IMapper map, IPredicate predicate, IDictionary<string, object> parameters);
        string IdentitySql(IMapper map);
        string GetTableName(IMapper map);
        string GetColumnName(IMapper map, IPropertyMap property, bool includeAlias);
        string GetColumnName(IMapper map, string propertyName, bool includeAlias);
        bool SupportsMultipleStatements();
    }

    [ContainerRegister(typeof (ISqlGenerator), RegistrationBehaviors.Default)]
    public class SqlGeneratorImpl : ISqlGenerator
    {
        public SqlGeneratorImpl(IRedRocketDapperConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IRedRocketDapperConfiguration Configuration { get; private set; }

        public virtual string Select(IMapper map, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var sql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                                                      BuildSelectColumns(map),
                                                      GetTableName(map)));
            if (predicate != null)
            {
                sql.Append(" WHERE ")
                   .Append(predicate.GetSql(this, parameters));
            }

            if (sort != null && sort.Any())
            {
                sql.Append(" ORDER BY ")
                   .Append(sort.Select(s => GetColumnName(map, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings());
            }

            return sql.ToString();
        }

        public virtual string SelectPaged(IMapper map, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            if (sort == null || !sort.Any())
            {
                throw new ArgumentNullException("Sort", "Sort cannot be null or empty.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                                                           BuildSelectColumns(map),
                                                           GetTableName(map)));
            if (predicate != null)
            {
                innerSql.Append(" WHERE ")
                        .Append(predicate.GetSql(this, parameters));
            }

            var orderBy = sort.Select(s => GetColumnName(map, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
            innerSql.Append(" ORDER BY " + orderBy);

            var sql = Configuration.Dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters);
            return sql;
        }

        public virtual string SelectSet(IMapper map, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters)
        {
            if (sort == null || !sort.Any())
            {
                throw new ArgumentNullException("Sort", "Sort cannot be null or empty.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                                                           BuildSelectColumns(map),
                                                           GetTableName(map)));
            if (predicate != null)
            {
                innerSql.Append(" WHERE ")
                        .Append(predicate.GetSql(this, parameters));
            }

            var orderBy = sort.Select(s => GetColumnName(map, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
            innerSql.Append(" ORDER BY " + orderBy);

            var sql = Configuration.Dialect.GetSetSql(innerSql.ToString(), firstResult, maxResults, parameters);
            return sql;
        }

        public virtual string Count(IMapper map, IPredicate predicate, IDictionary<string, object> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var sql = new StringBuilder(string.Format("SELECT COUNT(*) AS {0}Total{1} FROM {2}",
                                                      Configuration.Dialect.OpenQuote,
                                                      Configuration.Dialect.CloseQuote,
                                                      GetTableName(map)));
            if (predicate != null)
            {
                sql.Append(" WHERE ")
                   .Append(predicate.GetSql(this, parameters));
            }

            return sql.ToString();
        }

        public virtual string Insert(IMapper map)
        {
            var columns = map.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));
            if (!columns.Any())
            {
                throw new ArgumentException("No columns were mapped.");
            }

            var columnNames = columns.Select(p => GetColumnName(map, p, false));
            var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.Name);

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                    GetTableName(map),
                                    columnNames.AppendStrings(),
                                    parameters.AppendStrings());

            return sql;
        }

        public virtual string Update(IMapper map, IPredicate predicate, IDictionary<string, object> parameters)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("Predicate");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var columns = map.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));
            if (!columns.Any())
            {
                throw new ArgumentException("No columns were mapped.");
            }

            var setSql =
                columns.Select(
                               p =>
                               string.Format(
                                             "{0} = {1}{2}", GetColumnName(map, p, false), Configuration.Dialect.ParameterPrefix, p.Name));

            return string.Format("UPDATE {0} SET {1} WHERE {2}",
                                 GetTableName(map),
                                 setSql.AppendStrings(),
                                 predicate.GetSql(this, parameters));
        }

        public virtual string Delete(IMapper map, IPredicate predicate, IDictionary<string, object> parameters)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("Predicate");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }

            var sql = new StringBuilder(string.Format("DELETE FROM {0}", GetTableName(map)));
            sql.Append(" WHERE ").Append(predicate.GetSql(this, parameters));
            return sql.ToString();
        }

        public virtual string IdentitySql(IMapper map)
        {
            return Configuration.Dialect.GetIdentitySql(GetTableName(map));
        }

        public virtual string GetTableName(IMapper map)
        {
            return Configuration.Dialect.GetTableName(map.SchemaName, map.TableName, null);
        }

        public virtual string GetColumnName(IMapper map, IPropertyMap property, bool includeAlias)
        {
            string alias = null;
            if (property.ColumnName != property.Name && includeAlias)
            {
                alias = property.Name;
            }

            return Configuration.Dialect.GetColumnName(GetTableName(map), property.ColumnName, alias);
        }

        public virtual string GetColumnName(IMapper map, string propertyName, bool includeAlias)
        {
            var propertyMap = map.Properties.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (propertyMap == null)
            {
                throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
            }

            return GetColumnName(map, propertyMap, includeAlias);
        }

        public virtual bool SupportsMultipleStatements()
        {
            return Configuration.Dialect.SupportsMultipleStatements;
        }

        public virtual string BuildSelectColumns(IMapper map)
        {
            var columns = map.Properties
                                  .Where(p => !p.Ignored)
                                  .Select(p => GetColumnName(map, p, true));
            return columns.AppendStrings();
        }
    }
}
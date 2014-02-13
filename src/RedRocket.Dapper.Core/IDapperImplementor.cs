using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using Dapper;
using FlitBit.IoC.Meta;
using RedRocket.Dapper.Core.Mapper;
using RedRocket.Dapper.Core.Sql;
using RedRocket.Dapper.Core.Sql.Predicates;



namespace RedRocket.Dapper.Core
{
	public interface IDapperImplementor
	{
		ISqlGenerator SqlGenerator { get; }
		T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout);
		void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout);
		dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout);
		bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout);
		bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout);
		bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout);
		IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered);
		IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered);
		IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered);
		int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout);
		//IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout);
	}

	[ContainerRegister(typeof(IDapperImplementor), RegistrationBehaviors.Default)]
	public class DapperImplementor : IDapperImplementor
	{
		public DapperImplementor(ISqlGenerator sqlGenerator)
		{
			SqlGenerator = sqlGenerator;
		}

		public ISqlGenerator SqlGenerator { get; private set; }

		public T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var predicate = GetIdPredicate(classMap, id);
			T result = GetList<T>(connection, classMap, predicate, null, transaction, commandTimeout, true).SingleOrDefault();
			return result;
		}

		public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var properties = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);

			foreach (var e in entities)
			{
				foreach (var column in properties)
				{
					if (column.KeyType == KeyType.Guid)
						column.PropertyInfo.SetValue(e, Guid.NewGuid(), null);
				}
			}

			var sql = SqlGenerator.Insert(classMap);

			connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
		}

		public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
			var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
			foreach (var column in nonIdentityKeyProperties)
			{
				if (column.KeyType == KeyType.Guid)
					column.PropertyInfo.SetValue(entity, Guid.NewGuid(), null);
			}

			IDictionary<string, object> keyValues = new ExpandoObject();
			var sql = SqlGenerator.Insert(classMap);
			if (identityColumn != null)
			{
				IEnumerable<long> result;
				if (SqlGenerator.SupportsMultipleStatements())
				{
					sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
					result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
				}
				else
				{
					connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
					sql = SqlGenerator.IdentitySql(classMap);
					result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
				}

				var identityValue = result.First();
				var identityInt = Convert.ToInt32(identityValue);
				keyValues.Add(identityColumn.Name, identityInt);
				identityColumn.PropertyInfo.SetValue(entity, identityInt, null);
			}
			else
			{
				connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
			}

			foreach (var column in nonIdentityKeyProperties)
			{
				keyValues.Add(column.Name, column.PropertyInfo.GetValue(entity, null));
			}

			if (keyValues.Count == 1)
			{
				return keyValues.First().Value;
			}

			return keyValues;
		}

		public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var predicate = GetKeyPredicate(classMap, entity);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Update(classMap, predicate, parameters);
			var dynamicParameters = new DynamicParameters();

			var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));
			foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name == property.Key)))
			{
				dynamicParameters.Add(property.Key, property.Value);
			}

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}

		public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var predicate = GetKeyPredicate(classMap, entity);
			return Delete<T>(connection, classMap, predicate, transaction, commandTimeout);
		}

		public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return Delete<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
		}

		public IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return GetList<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, true);
		}

		public IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return GetPage<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
		}

		public IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return GetSet<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
		}

		public int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout)
		{
			var classMap = SqlGenerator.Configuration.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return (int)connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single().Total;
		}

		//public IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
		//{
		//    if (SqlGenerator.SupportsMultipleStatements())
		//    {
		//        return GetMultipleByBatch(connection, predicate, transaction, commandTimeout);
		//    }

		//    return GetMultipleBySequence(connection, predicate, transaction, commandTimeout);
		//}

		protected IEnumerable<T> GetList<T>(IDbConnection connection, IMapper map, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered)
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Select(map, predicate, sort, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetPage<T>(IDbConnection connection, IMapper map, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered)
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectPaged(map, predicate, sort, page, resultsPerPage, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetSet<T>(IDbConnection connection, IMapper map, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered)
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectSet(map, predicate, sort, firstResult, maxResults, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected bool Delete<T>(IDbConnection connection, IMapper map, IPredicate predicate, IDbTransaction transaction, int? commandTimeout)
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Delete(map, predicate, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}

		protected IPredicate GetPredicate(IMapper map, object predicate)
		{
			var wherePredicate = predicate as IPredicate;
			if (wherePredicate == null && predicate != null)
			{
				wherePredicate = GetEntityPredicate(map, predicate);
			}

			return wherePredicate;
		}

		protected IPredicate GetIdPredicate(IMapper map, object id)
		{
			var isSimpleType = ReflectionHelper.IsSimpleType(id.GetType());
			var keys = map.Properties.Where(p => p.KeyType != KeyType.NotAKey);
			IDictionary<string, object> paramValues = null;
			IList<IPredicate> predicates = new List<IPredicate>();
			if (!isSimpleType)
			{
				paramValues = ReflectionHelper.GetObjectValues(id);
			}

			foreach (var key in keys)
			{
				var value = id;
				if (!isSimpleType)
				{
					value = paramValues[key.Name];
				}

				var predicateType = typeof(FieldPredicate<>).MakeGenericType(map.EntityType);

				var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = key.Name;
				fieldPredicate.Value = value;
				predicates.Add(fieldPredicate);
			}

			return predicates.Count == 1
					   ? predicates[0]
					   : new PredicateGroup
						 {
							 Operator = GroupOperator.And,
							 Predicates = predicates
						 };
		}

		protected IPredicate GetKeyPredicate<T>(IMapper map, T entity)
		{
			var whereFields = map.Properties.Where(p => p.KeyType != KeyType.NotAKey);
			if (!whereFields.Any())
			{
				throw new ArgumentException("At least one Key column must be defined.");
			}

			IList<IPredicate> predicates = (from field in whereFields
											select new FieldPredicate<T>
												   {
													   Not = false,
													   Operator = Operator.Eq,
													   PropertyName = field.Name,
													   Value = field.PropertyInfo.GetValue(entity, null)
												   }).Cast<IPredicate>().ToList();

			return predicates.Count == 1
					   ? predicates[0]
					   : new PredicateGroup
						 {
							 Operator = GroupOperator.And,
							 Predicates = predicates
						 };
		}

		protected IPredicate GetEntityPredicate(IMapper map, object entity)
		{
			var predicateType = typeof(FieldPredicate<>).MakeGenericType(map.EntityType);
			IList<IPredicate> predicates = new List<IPredicate>();
			foreach (var kvp in ReflectionHelper.GetObjectValues(entity))
			{
				var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = kvp.Key;
				fieldPredicate.Value = kvp.Value;
				predicates.Add(fieldPredicate);
			}

			return predicates.Count == 1
					   ? predicates[0]
					   : new PredicateGroup
						 {
							 Operator = GroupOperator.And,
							 Predicates = predicates
						 };
		}

		//protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
		//{
		//    Dictionary<string, object> parameters = new Dictionary<string, object>();
		//    StringBuilder sql = new StringBuilder();
		//    foreach (var item in predicate.Items)
		//    {
		//        IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
		//        IPredicate itemPredicate = item.Value as IPredicate;
		//        if (itemPredicate == null && item.Value != null)
		//        {
		//            itemPredicate = GetPredicate(classMap, item.Value);
		//        }

		//        sql.AppendLine(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters) + SqlGenerator.Configuration.Dialect.BatchSeperator);
		//    }

		//    DynamicParameters dynamicParameters = new DynamicParameters();
		//    foreach (var parameter in parameters)
		//    {
		//        dynamicParameters.Add(parameter.Key, parameter.Value);
		//    }

		//    SqlMapper.GridReader grid = connection.QueryMultiple(sql.ToString(), dynamicParameters, transaction, commandTimeout, CommandType.Text);
		//    return new GridReaderResultReader(grid);
		//}

		//protected SequenceReaderResultReader GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
		//{
		//    IList<SqlMapper.GridReader> items = new List<SqlMapper.GridReader>();
		//    foreach (var item in predicate.Items)
		//    {
		//        Dictionary<string, object> parameters = new Dictionary<string, object>();
		//        IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
		//        IPredicate itemPredicate = item.Value as IPredicate;
		//        if (itemPredicate == null && item.Value != null)
		//        {
		//            itemPredicate = GetPredicate(classMap, item.Value);
		//        }

		//        string sql = SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters);
		//        DynamicParameters dynamicParameters = new DynamicParameters();
		//        foreach (var parameter in parameters)
		//        {
		//            dynamicParameters.Add(parameter.Key, parameter.Value);
		//        }

		//        SqlMapper.GridReader queryResult = connection.QueryMultiple(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
		//        items.Add(queryResult);
		//    }

		//    return new SequenceReaderResultReader(items);
		//}
	}
}
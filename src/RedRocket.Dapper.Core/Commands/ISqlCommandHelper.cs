#region

using System;
using System.Collections.Generic;
using System.Data;
using FlitBit.IoC;
using FlitBit.IoC.Meta;
using RedRocket.Dapper.Core.Sql.Predicates;

#endregion

namespace RedRocket.Dapper.Core.Commands
{
	public interface ISqlCommandHelper : IDisposable
	{
		IExecuteStoredProcedure StoredProcedures { get; }
		IExecuteFunction Functions { get; }
		T Get<T>(dynamic id, IDbTransaction transaction, int? commandTimeout = null);
		void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout = null);
		dynamic Insert<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null);
		bool Update<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null);
		bool Delete<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null);
		bool Delete<T>(object predicate, IDbTransaction transaction = null, int? commandTimeout = null);
		IEnumerable<T> All<T>(object predicate, IList<ISort> sort, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = true);
		IEnumerable<T> Paged<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = true);
		IEnumerable<T> Set<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = true);
		int Count<T>(object predicate, IDbTransaction transaction = null, int? commandTimeout = null);
	}

	[ContainerRegister(typeof(ISqlCommandHelper), RegistrationBehaviors.Default)]
	public class SqlCommandHelper : ISqlCommandHelper
	{
		IDapperImplementor Dapper { get; set; }
		public IDbConnection Connection { get; set; }
		public IExecuteStoredProcedure StoredProcedures { get; private set; }
		public IExecuteFunction Functions { get; private set; }
		public SqlCommandHelper(IDbConnection connection)
		{
			Connection = connection;
			Dapper = Create.New<IDapperImplementor>();
		}
		public SqlCommandHelper(IExecuteStoredProcedure storedProcedures, IExecuteFunction functions)
		{
			StoredProcedures = storedProcedures;
			Functions = functions;
		}
		public T Get<T>(dynamic id, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return (T)Dapper.Get<T>(Connection, id, transaction, commandTimeout);
		}
		public void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			Dapper.Insert(Connection, entities, transaction, commandTimeout);
		}
		public dynamic Insert<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return Dapper.Insert(Connection, entity, transaction, commandTimeout);
		}
		public bool Update<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return Dapper.Update(Connection, entity, transaction, commandTimeout);
		}
		public bool Delete<T>(T entity, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return Dapper.Delete(Connection, entity, transaction, commandTimeout);
		}
		public bool Delete<T>(object predicate, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return Dapper.Delete<T>(Connection, predicate, transaction, commandTimeout);
		}
		public IEnumerable<T> All<T>(object predicate, IList<ISort> sort, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = true)
		{
			return Dapper.GetList<T>(Connection, predicate, sort, transaction, commandTimeout, buffered);
		}
		public IEnumerable<T> Paged<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = true)
		{
			return Dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
		}
		public IEnumerable<T> Set<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = true)
		{
			return Dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
		}
		public int Count<T>(object predicate, IDbTransaction transaction = null, int? commandTimeout = null)
		{
			return Dapper.Count<T>(Connection, predicate, transaction, commandTimeout);
		}

		public void Dispose()
		{
			if (Connection != null && Connection.State != ConnectionState.Closed)
			{
				Connection.Close();
				Connection = null;
			}

		}
	}
}
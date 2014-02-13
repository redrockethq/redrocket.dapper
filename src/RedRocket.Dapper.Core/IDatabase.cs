using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FlitBit.IoC;
using FlitBit.IoC.Meta;
using RedRocket.Dapper.Core.Commands;
using RedRocket.Dapper.Core.Sql;
using RedRocket.Dapper.Core.Sql.Transactions;


namespace RedRocket.Dapper.Core
{
	public interface IDatabase : IDisposable
	{
		IDbConnection Connection { get; }
		ISqlCommandHelper Commands { get; }
		ITransactionHelper Transaction { get; }
	}

	[ContainerRegister(typeof(IDatabase), RegistrationBehaviors.Default, ScopeBehavior = ScopeBehavior.InstancePerRequest)]
	public class Database : IDatabase
	{
		IDapperImplementor _dapper;
		public Database(IDbConnection connection)
		{
			Connection = connection;
			Commands = Create.New<ISqlCommandHelper>();
			Transaction = Create.New<ITransactionHelper>();

			_dapper = Create.New<IDapperImplementor>();

			if (Connection.State != ConnectionState.Open)
				Connection.Open();
		}

		public IDbConnection Connection { get; private set; }
		public ISqlCommandHelper Commands { get; private set; }
		public ITransactionHelper Transaction { get; private set; }
		public void Dispose()
		{
			if (Connection.State != ConnectionState.Closed)
			{
				if (Transaction.HasActiveTransaction)
					Transaction.Rollback();
				Connection.Dispose();
			}
		}
	}

	public static class DbConnectionExtensions
	{
		public static ISqlCommandHelper Commands(this IDbConnection connection)
		{
			return Create.NewWithParams<ISqlCommandHelper>(LifespanTracking.Automatic, Param.FromValue(connection));
		}
	}


	public static class Db
	{
		public static IDictionary<string, string> ConnectionStrings { get; private set; }

		static Db()
		{
			DefaultConnectionString = string.Empty;

			var connectionStringManager = Create.New<IConnectionStringManager>();
			if (connectionStringManager != null)
				ConnectionStrings = connectionStringManager.ConnectionStrings;

		}

		public static string DefaultConnectionString { get; set; }

		public static IDatabase Connect()
		{
			if (!string.IsNullOrEmpty(DefaultConnectionString))
				throw new NullReferenceException("The Db.DefaultConnectionString was never setup.  Please use Db.SetDefaultConnectionStringByName('npdb') for example to setup the connection string.");

			return Connect(DefaultConnectionString);
		}

		public static IDatabase Connect(string connectionStringOrConnectionStringName)
		{
			if (!string.IsNullOrEmpty(DefaultConnectionString))
				throw new NullReferenceException("The connectionString is null or empty.  Please try again with a different connection string");

			var connectionString = connectionStringOrConnectionStringName;
			if (ConnectionStrings.ContainsKey(connectionStringOrConnectionStringName.ToLower()))
				connectionString = ConnectionStrings[connectionStringOrConnectionStringName.ToLower()];

			var sqlConnection = Create.NewWithParams<IDbConnection>(LifespanTracking.Automatic, Param.FromValue(connectionString));
			return Create.NewWithParams<IDatabase>(LifespanTracking.Automatic, Param.FromValue(sqlConnection));
		}

		public static void SetDefaultConnectionStringByName(string connectionName)
		{
			if (!ConnectionStrings.ContainsKey(connectionName))
				throw new KeyNotFoundException("A connection string name {0} was not found in the Db.ConnectionStrings...  Please look at your web.config and your machine.config to verify that the key has been properly added.".P(connectionName));
			DefaultConnectionString = ConnectionStrings[connectionName];
		}
	}
}
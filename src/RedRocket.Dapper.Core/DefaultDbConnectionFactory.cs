using System.Data.Common;
using System.Data.SqlClient;
using FlitBit.IoC;
using FlitBit.IoC.Meta;
using RedRocket.Dapper.Core.Sql;

namespace RedRocket.Dapper.Core
{
	public interface IDbConnectionFactory
	{
		string DefaultConnectionString { get; set; }

		IConnectionStringManager ConnectionStringManager { get; }

		/// <summary>
		///     Creates a connection based on the given database name or connection string.
		/// </summary>
		/// <param name="nameOrConnectionString"> The database name or connection string. </param>
		/// <returns> An initialized DbConnection. </returns>
		DbConnection CreateConnection(string nameOrConnectionString);
		DbConnection CreateConnection();
	}

	[ContainerRegister(typeof(IDbConnectionFactory), RegistrationBehaviors.Default, ScopeBehavior = ScopeBehavior.Singleton)]
	public class DefaultDbConnectionFactory : IDbConnectionFactory
	{
		public string DefaultConnectionString { get; set; }

		public IConnectionStringManager ConnectionStringManager { get; private set; }

		public DbConnection CreateConnection(string nameOrConnectionString)
		{
			if (ConnectionStringManager.ConnectionStrings.ContainsKey(nameOrConnectionString))
				nameOrConnectionString = ConnectionStringManager.ConnectionStrings[nameOrConnectionString];

			return new SqlConnection(nameOrConnectionString);
		}

		public DbConnection CreateConnection()
		{
			return CreateConnection(DefaultConnectionString);
		}
	}
}
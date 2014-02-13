using System.Diagnostics;
using System.Linq;
using FlitBit.IoC;
using RedRocket.Dapper.Core.Sql;
using RedRocket.Dapper.Core.Tests.Infrastructure;
using Xunit;

namespace RedRocket.Dapper.Core.Tests
{
	public class ConnectionStringManagerTests : AbstractTests
	{
		[Fact]
		public void ConnectionStringManagerShouldNotBeNull()
		{
			using (Create.SharedOrNewContainer())
			{
				var connectionStringManager = Create.New<IConnectionStringManager>();
				Assert.NotNull(connectionStringManager);
				Assert.NotEmpty(connectionStringManager.ConnectionStrings);
				Trace.WriteLine("Total Number of Connection Strings: {0}".P(connectionStringManager.ConnectionStrings.Count()));
				foreach (var key in connectionStringManager.ConnectionStrings.Keys)
				{
					var connection = connectionStringManager.ConnectionStrings[key];
					Trace.WriteLine("{0}: {1}".P(key, connection));
				}
			}
		}

		[Fact]
		public void DbHelperShouldHaveEmptyConnectionStringWithoutInitialization()
		{
			Db.DefaultConnectionString = string.Empty;
			using (Create.SharedOrNewContainer())
			{
				Assert.Empty(Db.DefaultConnectionString);
			}
		}

		[Fact]
		public void DbHelperShouldAllowSettingTheDefaultConnectionString()
		{
			using (Create.SharedOrNewContainer())
			{
				const string connectionStringName = "npdb";
				var rawConnectionString = Db.ConnectionStrings[connectionStringName];
				Db.SetDefaultConnectionStringByName(connectionStringName);
				Assert.Equal(rawConnectionString, Db.DefaultConnectionString);
			}
		}
	}
}
using System.Data;
using FlitBit.IoC;
using RedRocket.Dapper.Core.Tests.Infrastructure;
using Xunit;

namespace RedRocket.Dapper.Core.Tests
{
	public class DbOpenConnectionTests : AbstractTests
	{
		[Fact]
		public void CanOpenConnection()
		{
			using (Create.SharedOrNewContainer())
			using (var conn = Create.New<IDbConnection>())
			{
				conn.Open();
				Assert.Equal(conn.State, ConnectionState.Open);
			}
		}


		[Fact]
		public void CanOpenConnectinFromDB()
		{
			using (Create.SharedOrNewContainer())
			{
				using (var db = Create.New<IDatabase>())
				{
					db.Connection.Open();
					Assert.Equal(db.Connection.State, ConnectionState.Open);

					db.Dispose();
				}
			}

		}
	}
}
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
			var connectionString = Db.ConnectionStrings["npdb"];
			Assert.NotEmpty(connectionString);

			using (Create.SharedOrNewContainer())
			using (var connection = Create.New<IDbConnection>())
			{
				connection.Open();
				Assert.True(connection.State == ConnectionState.Open, "Connection should have opened, however it's in the {0} state".P(connection.State));
			}
		}
	}
}
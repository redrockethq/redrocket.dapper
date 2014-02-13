using System;
using Dapper;
using FlitBit.IoC;
using RedRocket.Dapper.Core.Sql.Predicates;
using RedRocket.Dapper.Core.Tests.Infrastructure;
using RedRocket.Dapper.Core.Tests.Models;
using Xunit;

namespace RedRocket.Dapper.Core.Tests
{
    public class DbTableTests : AbstractTests
    {
        [Fact]
        public void Testing()
        {
            using (Create.SharedOrNewContainer())
            {
				var user = Create.New<IUser>();
            }
        }

        public void Blah()
        {
			var db = Create.New<IDatabase>();
            var predicate = Predicates.Field<IUser>(u => u.FirstName, Operator.Eq, "blah");
        }
    }
}
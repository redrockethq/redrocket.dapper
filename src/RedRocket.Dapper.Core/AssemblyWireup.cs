using System.Data;
using System.Data.SqlClient;
using FlitBit.IoC;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;
using RedRocket.Dapper.Core.Mapper;
using AssemblyWireup = RedRocket.Dapper.Core.AssemblyWireup;

[assembly: Wireup(typeof(AssemblyWireup))]
namespace RedRocket.Dapper.Core
{
	public class AssemblyWireup : IWireupCommand
	{
		public void Execute(IWireupCoordinator coordinator)
		{
			Container.Root
					 .ForGenericType(typeof(IMapper<>))
					 .Register(typeof(DtoMapper<>))
					 .End();

			Container.Root.ForType<IDbConnection>()
					 .Register<SqlConnection>()
					 .ResolveAnInstancePerScope()
					 .End();
		}
	}
}
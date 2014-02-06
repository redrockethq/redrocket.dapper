using Dapper;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

[assembly: Wireup(typeof(RedRocket.Dapper.Core.AssemblyWireup))]
namespace RedRocket.Dapper.Core
{
    public class AssemblyWireup : IWireupCommand
    {
        public void Execute(IWireupCoordinator coordinator)
        {
            var typeMap = SqlMapper.GetTypeMap(typeof (DefaultTypeMap));
            //typeMap.
            //SqlMapper.SetTypeMap();          
        }
    }
}

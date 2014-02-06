using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FlitBit.Core;
using FlitBit.Core.Factory;

namespace RedRocket.Dapper.Core.Configuration
{

    public class DtoTypeMap : SqlMapper.ITypeMap
    {
        readonly IFactory _factory;

        public DtoTypeMap()
        {
            _factory = FactoryProvider.Factory;
        }

        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            throw new NotImplementedException();
        }

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            throw new NotImplementedException();
        }

        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            throw new NotImplementedException();
        }

        IEnumerable<ConstructorInfo> GetConstructorInfoFromType(Type type)
        {
            return _factory.GetImplementationType(type).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}

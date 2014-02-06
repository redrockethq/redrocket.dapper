#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlitBit.IoC;
using FlitBit.IoC.Meta;
using RedRocket.Dapper.Core.Mapper;
using RedRocket.Dapper.Core.Sql;

#endregion

namespace RedRocket.Dapper.Core
{
    public interface IRedRocketDapperConfiguration
    {
        ISqlDialect Dialect { get; }
        IMapper GetMap(Type entityType);
        IMapper GetMap<T>();
    }

    [ContainerRegister(typeof(IRedRocketDapperConfiguration), RegistrationBehaviors.Default)]
    public class DefaultConfiguration : IRedRocketDapperConfiguration
    {
        public DefaultConfiguration()
        {
            Dialect = Create.New<ISqlDialect>();
        }

        public ISqlDialect Dialect { get; private set; }
        public IMapper GetMap(Type entityType)
        {
            return Create.New<IMapper>();
        }

        public IMapper GetMap<T>()
        {
            return GetMap(typeof(T));
        }
    }
}
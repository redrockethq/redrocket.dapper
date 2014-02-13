 using System.Configuration;
using System.Linq;
 using FlitBit.IoC;
 using FlitBit.IoC.Meta;
using System.Collections.Generic;

namespace RedRocket.Dapper.Core.Sql
{
	public interface IConnectionStringManager
	{
		IDictionary<string, string> ConnectionStrings { get; }
	}

	[ContainerRegister(typeof(IConnectionStringManager), RegistrationBehaviors.Default, ScopeBehavior = ScopeBehavior.Singleton)]
	public class SqlServerConnectionStringManager : IConnectionStringManager
	{
		public IDictionary<string, string> ConnectionStrings { get; private set; }
		public bool ShouldLoadFromApplicationConfig { get; private set; }
		public bool ShouldLoadFromMachineConfig { get; private set; }

		public SqlServerConnectionStringManager()
			: this(new Dictionary<string, string>())
		{
		}

		public SqlServerConnectionStringManager(IDictionary<string, string> connectionStrings, bool shouldLoadFromApplicationConfig = true, bool shouldLoadFromMachineConfig = true)
		{
			ShouldLoadFromApplicationConfig = shouldLoadFromApplicationConfig;
			ShouldLoadFromMachineConfig = shouldLoadFromMachineConfig;

			var cache = new Dictionary<string, string>(connectionStrings);
			if (ShouldLoadFromApplicationConfig)
				foreach (var setting in ApplicationConfiguration.Where(setting => !cache.ContainsKey(setting.Name.ToLower())))
					cache.Add(setting.Name.ToLower(), setting.ConnectionString);

			if (ShouldLoadFromMachineConfig)
				foreach (var setting in MachineConfiguration.Where(setting => !cache.ContainsKey(setting.Name.ToLower())))
					cache.Add(setting.Name.ToLower(), setting.ConnectionString);

			ConnectionStrings = cache;
		}

		IEnumerable<ConnectionStringSettings> ApplicationConfiguration
		{
			get
			{
				return ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>();
			}
		}

		IEnumerable<ConnectionStringSettings> MachineConfiguration
		{
			get
			{
				return ConfigurationManager.OpenMachineConfiguration().ConnectionStrings.ConnectionStrings.Cast<ConnectionStringSettings>();
			}
		}
	}
}
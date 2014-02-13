using FlitBit.IoC.Meta;

namespace RedRocket.Dapper.Core.Commands
{
    public interface IExecuteStoredProcedure
    {
        TOutput Execute<TOutput>(string storedProcedureName);
        TOutput Execute<TOutput>(string storedProcedureName, dynamic procedureArguments);
        TOutput Execute<TOutput, TInput>(string storedProcedureName, TInput input);
    }

	[ContainerRegister(typeof(IExecuteStoredProcedure), RegistrationBehaviors.Default)]
	public class DefaultExecuteStoredProcedure : IExecuteStoredProcedure {
		public TOutput Execute<TOutput>(string storedProcedureName)
		{
			throw new System.NotImplementedException();
		}

		public TOutput Execute<TOutput>(string storedProcedureName, dynamic procedureArguments)
		{
			throw new System.NotImplementedException();
		}

		public TOutput Execute<TOutput, TInput>(string storedProcedureName, TInput input)
		{
			throw new System.NotImplementedException();
		}
	}
}
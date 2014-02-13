using FlitBit.IoC.Meta;

namespace RedRocket.Dapper.Core.Commands
{
    public interface IExecuteFunction
    {
        TOutput Execute<TOutput>(string functionName);
        TOutput Execute<TOutput>(string functionName, dynamic functionParameters);
    }

	[ContainerRegister(typeof(IExecuteFunction), RegistrationBehaviors.Default)]
	public class DefaultExecuteFunction : IExecuteFunction {
		public TOutput Execute<TOutput>(string functionName)
		{
			throw new System.NotImplementedException();
		}

		public TOutput Execute<TOutput>(string functionName, dynamic functionParameters)
		{
			throw new System.NotImplementedException();
		}
	}
}
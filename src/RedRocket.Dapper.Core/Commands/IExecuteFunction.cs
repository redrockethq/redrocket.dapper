namespace RedRocket.Dapper.Core.Commands
{
    public interface IExecuteFunction
    {
        TOutput Execute<TOutput>(string functionName);
        TOutput Execute<TOutput>(string functionName, dynamic functionParameters);
    }
}
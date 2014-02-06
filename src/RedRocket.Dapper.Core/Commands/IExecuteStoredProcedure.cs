namespace RedRocket.Dapper.Core.Commands
{
    public interface IExecuteStoredProcedure
    {
        TOutput Execute<TOutput>(string storedProcedureName);
        TOutput Execute<TOutput>(string storedProcedureName, dynamic procedureArguments);
        TOutput Execute<TOutput, TInput>(string storedProcedureName, TInput input);
    }
}
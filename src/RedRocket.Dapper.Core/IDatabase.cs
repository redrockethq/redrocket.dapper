#region

using System;
using System.Data;
using FlitBit.IoC;
using FlitBit.IoC.Meta;
using RedRocket.Dapper.Core.Commands;
using RedRocket.Dapper.Core.Sql.Transactions;

#endregion

namespace RedRocket.Dapper.Core
{
    public interface IDatabase : IDisposable
    {
        IDbConnection Connection { get; }
        ISqlCommandHelper Commands { get; }
        ITransactionHelper Transactions { get; }
    }

    [ContainerRegister(typeof (IDatabase), RegistrationBehaviors.Default, ScopeBehavior = ScopeBehavior.InstancePerScope)]
    public class Database : IDatabase
    {
        IDapperImplementor _dapper;

        public Database(IDbConnection connection)
        {
            Connection = connection;
            Commands = Create.New<ISqlCommandHelper>();
            Transactions = Create.New<ITransactionHelper>();

            _dapper = Create.New<IDapperImplementor>();

            if (Connection.State != ConnectionState.Open)
                Connection.Open();
        }

        public IDbConnection Connection { get; private set; }
        public ISqlCommandHelper Commands { get; private set; }
        public ITransactionHelper Transactions { get; private set; }
        public void Dispose()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                //if (_transaction != null)
                //{
                //    _transaction.Rollback();
                //}

                Connection.Close();
            }
        }
    }
}
﻿#region

using System;
using System.Data;
using FlitBit.IoC.Meta;

#endregion

namespace RedRocket.Dapper.Core.Sql.Transactions
{
	public interface ITransactionHelper : IDisposable
	{
		bool HasActiveTransaction { get; }
		void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
		void Commit();
		void Rollback();
		void RunInTransaction(Action action);
		T RunInTransaction<T>(Func<T> func);
	}

	[ContainerRegister(typeof(ITransactionHelper), RegistrationBehaviors.Default)]
	public class TransactionHelper : ITransactionHelper
	{
		IDbConnection Connection { get; set; }

		public TransactionHelper(IDbConnection connection)
		{
			Connection = connection;
		}

		protected IDbTransaction CurrentTransaction { get; private set; }

		public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			CurrentTransaction = Connection.BeginTransaction(isolationLevel);
		}

		public void Commit()
		{
			CurrentTransaction.Commit();
			CurrentTransaction = null;
		}

		public void Rollback()
		{
			CurrentTransaction.Rollback();
			CurrentTransaction = null;
		}

		public void RunInTransaction(Action action)
		{
			BeginTransaction();
			try
			{
				action();
				Commit();
			}
			catch (Exception ex)
			{
				if (HasActiveTransaction)
					Rollback();

				throw ex;
			}
		}

		public T RunInTransaction<T>(Func<T> func)
		{
			BeginTransaction();
			try
			{
				var result = func();
				Commit();
				return result;
			}
			catch (Exception ex)
			{
				if (HasActiveTransaction)
					Rollback();

				throw ex;
			}
		}

		public bool HasActiveTransaction
		{
			get { return CurrentTransaction != null; }
		}

		public void Dispose()
		{
			if (Connection != null && Connection.State != ConnectionState.Closed)
			{
				Connection.Close();
				Connection = null;
			}

		}
	}
}
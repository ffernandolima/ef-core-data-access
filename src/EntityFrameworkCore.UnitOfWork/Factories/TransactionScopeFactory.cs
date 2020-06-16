using System;
using System.Transactions;

namespace EntityFrameworkCore.UnitOfWork.Factories
{
    public class TransactionScopeFactory
    {
        public static TransactionScope CreateTransactionScope(TransactionScopeOption transactionScopeOption = TransactionScopeOption.Required,
                                                              IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                              TimeSpan? timeout = null,
                                                              TransactionScopeAsyncFlowOption? transactionScopeAsyncFlowOption = null)
        {
            TransactionScope transactionScope;

            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = isolationLevel
            };

            if (timeout.HasValue)
            {
                transactionOptions.Timeout = timeout.Value;
            }
            else
            {
                transactionOptions.Timeout = TransactionManager.DefaultTimeout;
            }

            if (transactionScopeAsyncFlowOption.HasValue)
            {
                transactionScope = new TransactionScope(transactionScopeOption, transactionOptions, transactionScopeAsyncFlowOption.Value);
            }
            else
            {
                transactionScope = new TransactionScope(transactionScopeOption, transactionOptions);
            }

            return transactionScope;
        }
    }
}

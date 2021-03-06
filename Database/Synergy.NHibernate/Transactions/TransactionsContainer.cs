﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using NHibernate;
using Synergy.Contracts;
using Synergy.NHibernate.Engine;

namespace Synergy.NHibernate.Transactions
{
    /// <summary>
    /// Container for keeping bunch of transactions for awhile.
    /// When the object is disposed all transactions are also disposed. If <see cref="Commit"/> was called before the disposal 
    /// all the transactions will be committed otherwise everything is rolled back.
    /// </summary>
    public class TransactionsContainer : IDisposable
    {
        private readonly List<SingleTransacion> transactions = new List<SingleTransacion>(1);

        [NotNull, Pure]
        public ISession StartSession([NotNull] IDatabase database)
        {
            Fail.IfArgumentNull(database, nameof(database));

            return this.GetSession(database)
                       .Session;
        }

        public void StartTransaction([NotNull] IDatabase database, IsolationLevel isolationLevel)
        {
            Fail.IfArgumentNull(database, nameof(database));

            var container = this.GetSession(database);

            ISession session = container.Session;
            ITransaction transaction = session.Transaction;
            if (transaction.IsActive == false)
            {
                container.Transaction = session.BeginTransaction(isolationLevel);
                container.TransactionJustStarted = true;
            }
        }

        [NotNull, Pure]
        private SingleTransacion GetSession([NotNull] IDatabase database)
        {
            ISession session = database.GetSession();
            bool sessionJustCreated = false;
            if (session == null)
            {
                session = database.OpenSession();
                sessionJustCreated = true;
            }

            return this.Add(database, session, sessionJustCreated);
        }

        /// <summary>
        /// Adds a transaction to this container.
        /// </summary>
        [NotNull, Pure]
        private SingleTransacion Add(
            [NotNull] IDatabase database, 
            [NotNull] ISession session, 
            bool sessionJustCreated, 
            [CanBeNull] ITransaction transaction = null, 
            bool transactionJustStarted = false)
        {
            var singleTransacion = new SingleTransacion(database, session)
            {
                SessionJustCreated = sessionJustCreated,
                Transaction = transaction,
                TransactionJustStarted = transactionJustStarted
            };

            this.transactions.Add(singleTransacion);

            return singleTransacion;
        }

        [NotNull, ItemNotNull, Pure]
        private IEnumerable<ITransaction> JustStartedTransactions()
        {
            return this.transactions
                       .Where(t => t.TransactionJustStarted)
                       .Select(t => t.Transaction);
        }

        [NotNull, ItemNotNull, Pure]
        private ISession[] JustStartedSessions()
        {
            return this.transactions
                       .Where(t => t.SessionJustCreated)
                       .Select(t => t.Session)
                       .ToArray();
        }

        /// <summary>
        /// Commits all the transactions in this container.
        /// </summary>
        public void Commit()
        {
            foreach (ITransaction transaction in this.JustStartedTransactions())
                transaction.Commit();
        }

        /// <summary>
        /// Disposes all newly started transaction in this container.
        /// If the transactions were committed via <see cref="Commit"/> method then the disposal does nothing interesting.
        /// If the transactions were not committed, disposing this container will automatically rollback them.
        /// </summary>
        public void Dispose()
        {
            foreach (var transaction in this.JustStartedTransactions())
                transaction.Dispose();

            foreach (ISession session in this.JustStartedSessions())
                session.Dispose();

            this.transactions.Clear();
        }

        private class SingleTransacion
        {
            [NotNull]
            private IDatabase Database { get; }

            [NotNull] 
            public ISession Session { get; }

            public SingleTransacion([NotNull] IDatabase database, [NotNull] ISession session)
            {
                Fail.IfArgumentNull(database, nameof(database));
                Fail.IfArgumentNull(session, nameof(session));

                this.Database = database;
                this.Session = session;
            }

            public bool SessionJustCreated;

            [CanBeNull] 
            public ITransaction Transaction;
            public bool TransactionJustStarted;
        }

    }
}
using FetchRewardsExercise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FetchRewardsExercise.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        // Make static to keep alive
        private static List<Transaction> _transactions;

        public TransactionRepository()
        {
            if (_transactions == null)
            {
                _transactions = new List<Transaction>()
                {
                    new Transaction
                    {
                        Payer = "DANNON",
                        Points = 1000,
                        Timestamp = DateTime.Parse("2020-11-02T14:00:00Z")
                    },
                    new Transaction
                    {
                        Payer = "UNILEVER",
                        Points = 200,
                        Timestamp = DateTime.Parse("2020-10-31T11:00:00Z")
                    },
                    new Transaction
                    {
                        Payer = "DANNON",
                        Points = -200,
                        Timestamp = DateTime.Parse("2020-10-31T15:00:00Z")
                    },
                    new Transaction
                    {
                        Payer = "MILLER COORS",
                        Points = 10000,
                        Timestamp = DateTime.Parse("2020-11-01T14:00:00Z")
                    },
                    new Transaction
                    {
                        Payer = "DANNON",
                        Points = 300,
                        Timestamp = DateTime.Parse("2020-10-31T10:00:00Z")
                    }
                };
            }
        }

        public void AddRange(IEnumerable<Transaction> transactions)
        {
            _transactions.AddRange(transactions);
        }

        public void AddTransaction(Transaction transaction)
        {
            _transactions.Add(transaction);
        }

        public List<Transaction> GetTransactions()
        {
            return _transactions;
        }

    }
}

using FetchRewardsExercise.Models;
using System.Collections.Generic;

namespace FetchRewardsExercise.Repositories
{
    public interface ITransactionRepository
    {
        void AddTransaction(Transaction transaction);

        void AddRange(IEnumerable<Transaction> transactions);

        List<Transaction> GetTransactions();
    }
}
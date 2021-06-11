using FetchRewardsExercise.Models;
using System.Collections.Generic;

namespace FetchRewardsExercise.Repositories
{
    public interface ITransactionRepository
    {
        void AddTransaction(Transaction transaction);
        List<Transaction> GetTransactions();
    }
}
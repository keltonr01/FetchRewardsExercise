using FetchRewardsExercise.Models;
using System.Collections.Generic;

namespace FetchRewardsExercise.Repositories
{
    public interface IPayerRepository
    {
        void AddPayer(Payer payer);
        Payer GetPayer(string name);
        List<Payer> GetPayers();
        void UpdatePayer(Payer payer);
    }
}
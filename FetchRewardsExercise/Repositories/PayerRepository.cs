using FetchRewardsExercise.Models;
using System.Collections.Generic;
using System.Linq;

namespace FetchRewardsExercise.Repositories
{
    /// <summary>
    /// Simple respository that exists for testing purposes only.
    /// In production this would be replaced by a durable solution.
    /// </summary>
    public class PayerRepository : IPayerRepository
    {
        // Make static to keep alive.
        private static List<Payer> payers;

        public PayerRepository()
        {
            if (payers == null)
            {
                payers = new List<Payer>()
            {
                new Payer
                {
                    Name = "DANNON",
                    Points = 10000
                },
                new Payer
                {
                    Name = "UNILEVER",
                    Points = 10000
                },
                new Payer
                {
                    Name = "MILLER COORS",
                    Points = 10000
                }
            };
            }
        }

        public void AddPayer(Payer payer)
        {
            payers.Add(payer);
        }

        public List<Payer> GetPayers()
        {
            return payers;
        }

        public Payer GetPayer(string name)
        {
            return payers.Where(x => x.Name == name).SingleOrDefault();
        }

        public void UpdatePayer(Payer payer)
        {
            var payerInRepo = payers.Where(x => x.Name == payer.Name).Single();
            payerInRepo.Points = payer.Points;
        }


    }
}

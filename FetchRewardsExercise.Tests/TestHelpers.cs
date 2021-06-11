using FetchRewardsExercise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchRewardsExercise.Tests
{
    public static class TestHelpers
    {
        public static List<Transaction> GetTestTransactions()
        {
            var transactions = new List<Transaction>()
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

            return transactions;
        }

        public static List<Payer> GetTestPayers()
        {
            var payers = new List<Payer>()
            {
                new Payer
                {
                    Name = "DANNON",
                    Points = 0
                },
                new Payer
                {
                    Name = "UNILEVER",
                    Points = 0
                },
                new Payer
                {
                    Name = "MILLER COORS",
                    Points = 0
                }
            };

            return payers;
        }
    }
}

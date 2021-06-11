using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FetchRewardsExercise.Models
{
    public class Transaction
    {
        public string Payer { get; set; }

        public int Points { get; set; }

        public DateTime Timestamp { get; set; }

        public int PointsRemaining { get; set; }

        // bool to determine if a transaction has already been used when deducting points.
        public bool UsedForPointsDeduction { get; set; } = false;
    }
}

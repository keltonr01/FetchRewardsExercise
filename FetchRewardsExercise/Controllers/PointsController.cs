using FetchRewardsExercise.Models;
using FetchRewardsExercise.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace FetchRewardsExercise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointsController : ControllerBase
    {       
        private readonly ITransactionRepository _transactionRepository;

        public PointsController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        /// <summary>
        /// Spend the user's points balance.
        /// </summary>
        /// <param name="points"></param>
        /// <returns>A breakdown by <c>Payer</c> of how the points were spent.</returns>
        /// <response code="200">Returns breakdown of how points were spent.</response>
        /// <response code="400">If the user does not have enough points to spend.</response>
        /// <response code="500">If there is an inconsistency with the transactions or some other intenal error.</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult SpendPoints(Points points)
        {           
            var pointsValue = points.Value;

            var totalSum = _transactionRepository.GetTransactions().Sum(x => x.Points);
            if (totalSum < pointsValue)
            {
                return BadRequest("User does not have enough points.");
            }

            // Get the transactions in ordery by date and time.
            var transactions = _transactionRepository.GetTransactions().OrderBy(x => x.Timestamp).ToList();

            // This will become an "effective" list. 
            // i.e. will only have positive transactions, after subtracting out negative transactions.
            var effectiveTransactions = new List<Transaction>();
            // Deep copy of the list. Probably a better way to do this. ICloneable? Will investigate if time permits.
            foreach (var transaction in transactions)
            {
                effectiveTransactions.Add(new Transaction
                {
                    Payer = transaction.Payer,
                    Points = transaction.Points,
                    Timestamp = transaction.Timestamp
                });
            }

            var positiveTransactions = effectiveTransactions.OrderBy(x => x.Timestamp).Where(t => t.Points > 0).ToList();
            var negativeTransactions = effectiveTransactions.OrderBy(x => x.Timestamp).Where(t => t.Points < 0).ToList();
            // Get all the payers.

            // Get each payer and the available points to spend.
            var payers = positiveTransactions.GroupBy(t => t.Payer).Select(p =>
                new Payer()
                {
                    Name = p.FirstOrDefault().Payer,
                    Points = p.Sum(t => t.Points) // Sum the points of all transactions for this payer.
                }).ToList();


            // Create enumerators to iterate over the transaction lists.
            var oldestPositiveTransaction = positiveTransactions.GetEnumerator();
            var oldestNegativeTransaction = negativeTransactions.GetEnumerator();

            #region Calculate Previous Transactions

            int remainder = 0;

            // Iterate over the payers to operate on their transactions.
            foreach (var payer in payers)
            {
                var payerTransactions = effectiveTransactions.Where(t => t.Payer == payer.Name); // Get all the transactions from the current payer. 
                var positivePayerTransactions = payerTransactions.OrderBy(t => t.Timestamp).Where(t => t.Points > 0);
                var negativePayerTransactions = payerTransactions.OrderBy(t => t.Timestamp).Where(t => t.Points < 0);

                // Sanity check to make sure that a payer won't go to zero while doing previous calculations.
                // This souldn't be able to happen.
                if (positivePayerTransactions.Sum(x => x.Points) < negativePayerTransactions.Sum(x => x.Points))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Inconsistency found in transactions.");
                }

                var oldestPositivePayerTransaction = positivePayerTransactions.GetEnumerator();
                oldestPositivePayerTransaction.MoveNext();

                foreach (var negativeTransaction in negativePayerTransactions)
                {
                    payer.Points -= (negativeTransaction.Points * -1);

                    // Sanity check to make sure we aren't pulling points from transactions that were added after the negative transaction occurred.
                    // This souldn't be able to happen.
                    if (negativeTransaction.Timestamp < oldestPositivePayerTransaction.Current.Timestamp)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Inconsistency found in transactions.");
                    }

                    remainder = oldestPositivePayerTransaction.Current.Points - (negativeTransaction.Points * -1);

                    if (remainder > 0)
                    {
                        oldestPositivePayerTransaction.Current.Points = remainder;                 
                    }
                    else
                    {
                        // If the current positive transaction is not enough to cover the negative amount (remainder), we will nedd keep "spending" 
                        // positive transactions until it is covered.
                        while (oldestPositivePayerTransaction.Current.Points - remainder <= 0)
                        {
                            remainder -= oldestPositivePayerTransaction.Current.Points; // Subtract out the positive amount to get what is still needed.
                            effectiveTransactions.Remove(oldestPositivePayerTransaction.Current); // Remove the spent transaction.
                            oldestPositivePayerTransaction.MoveNext();

                            // Sanity check to make sure we aren't pulling points from transactions that were added after the negative transaction occurred.
                            // This souldn't be able to happen.
                            if (negativeTransaction.Timestamp < oldestPositivePayerTransaction.Current.Timestamp)
                            {
                                return StatusCode(StatusCodes.Status500InternalServerError, "Inconsistency found in transactions.");
                            }
                        }

                        oldestPositivePayerTransaction.Current.Points = remainder; // How much remains in the last needed positive transaction.
                    }

                    // Remove current negative transaction from list as it is no longer needed.
                    effectiveTransactions.Remove(negativeTransaction);
                }
            }

            #endregion

            // At this point, EffectiveTransactions should only contain positive transactions.

            List<Transaction> newTransactions = new List<Transaction>(); // Transactions that will need to be added to the database from calculating spent points.
            List<PointsReturn> pointsReturns = new List<PointsReturn>(); // Return params.

            foreach (var transaction in effectiveTransactions)
            {
                pointsValue -= transaction.Points; // Similar to before, keep "spending" points.

                if (pointsValue >= 0)
                {
                    newTransactions.Add(new Transaction
                    {
                        Payer = transaction.Payer,
                        Points = transaction.Points * -1,
                        Timestamp = DateTime.Now
                    });
                    pointsReturns.Add(new PointsReturn
                    {
                        Payer = transaction.Payer,
                        Points = transaction.Points * -1
                    });
                }
                else
                {
                    newTransactions.Add(new Transaction
                    {
                        Payer = transaction.Payer,
                        Points = (transaction.Points + pointsValue) * -1,
                        Timestamp = DateTime.Now
                    });
                    pointsReturns.Add(new PointsReturn
                    {
                        Payer = transaction.Payer,
                        Points = (transaction.Points + pointsValue) * -1
                    });
                }

                if (pointsValue <= 0)
                {
                    break;
                }
            }

            _transactionRepository.AddRange(newTransactions);

            return Ok(pointsReturns);
        }

        /// <summary>
        /// Get the balance for each payer.
        /// </summary>
        /// <returns>The balances for each payer.</returns>
        /// <response code="200">The bbalances for each payer.</response>
        [HttpGet]       
        public ActionResult<Dictionary<string, int>> GetBalance()
        {
            var payerBalances = new Dictionary<string, int>();

            var payers = _transactionRepository.GetTransactions().GroupBy(t => t.Payer).Select(p =>
                new Payer()
                {
                    Name = p.FirstOrDefault().Payer,
                    Points = p.Sum(t => t.Points) // Sum the points of all transactions for this payer.
                }).ToList();

            foreach (var payer in payers)
            {
                payerBalances.Add(payer.Name, payer.Points);
            }

            return Ok(payerBalances);
        }
    }

    public class PointsReturn
    {
        [JsonPropertyName("payer")]
        public string Payer { get; set; }

        [JsonPropertyName("points")]
        public int Points { get; set; }
    }

    public class Points
    {
        [JsonPropertyName("points")]
        public int Value { get; set; }
    }
}

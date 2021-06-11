using FetchRewardsExercise.Models;
using FetchRewardsExercise.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult SpendPoints(Points points)
        {

            //string body = string.Empty;
            //// Read the request body.
            //using (var reader = new StreamReader(Request.Body))
            //{
            //    body = reader.ReadToEnd();
            //}
            //
            //// Parse the points value to an int.
            //JObject bodyJson = JObject.Parse(body);
            //var pointsToken = bodyJson["points"];
            //int points = pointsToken.ToObject<int>();

            var pointsValue = points.Value;

            // Get the transactions in ordery by date and time (as a copy). This will become an "effective" list. 
            // i.e. will only have positive transactions, after subtracting out negative transactions.
            var effectiveTransactions = _transactionRepository.GetTransactions().OrderBy(x => x.Timestamp).ToList();
            var positiveTransactions = effectiveTransactions.OrderBy(x => x.Timestamp).Where(t => t.Points > 0).ToList();
            var negativeTransactions = effectiveTransactions.OrderBy(x => x.Timestamp).Where(t => t.Points < 0).ToList();
            // Get all the payers.
            //var payers = _payerRepository.GetPayers().Where(p => transactions.Any(t => p.Name == t.Payer));

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
                        //
                        while (oldestPositivePayerTransaction.Current.Points - remainder <= 0)
                        {
                            remainder -= oldestPositivePayerTransaction.Current.Points;
                            effectiveTransactions.Remove(oldestPositivePayerTransaction.Current);
                            //effectiveTransactions.Remove(oldestNegativeTransaction.Current);
                            oldestPositivePayerTransaction.MoveNext();

                            // Sanity check to make sure we aren't pulling points from transactions that were added after the negative transaction occurred.
                            // This souldn't be able to happen.
                            if (negativeTransaction.Timestamp < oldestPositivePayerTransaction.Current.Timestamp)
                            {
                                return StatusCode(StatusCodes.Status500InternalServerError, "Inconsistency found in transactions.");
                            }
                        }

                        oldestPositivePayerTransaction.Current.Points = remainder;
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
                pointsValue -= transaction.Points;
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

            return Ok(pointsReturns);
        }

        [HttpGet]
        public IActionResult GetBalance()
        {
            var payerBalances = new Dictionary<string, int>();

            var payers = _transactionRepository.GetTransactions().GroupBy(t => t.Payer).Select(p =>
                new Payer()
                {
                    Name = p.FirstOrDefault().Payer,
                    Points = p.Sum(t => t.Points) // Sum the points of all transactions for this payer.
                }).ToList();

            return Ok();
        }
        

        /// <summary>
        /// Small class for returning required paramaters when spending points.
        /// </summary>
        private class PointsReturn
        {
            public string Payer { get; set; }

            public int Points { get; set; }
        }



    }
}

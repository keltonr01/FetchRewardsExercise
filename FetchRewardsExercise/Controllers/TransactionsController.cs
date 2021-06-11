using FetchRewardsExercise.Models;
using FetchRewardsExercise.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace FetchRewardsExercise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionsController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        [HttpGet]
        public IActionResult GetTransactions()
        {
            JsonSerializer serializer = new JsonSerializer();
            string result = JsonConvert.SerializeObject(_transactionRepository.GetTransactions());
            return Ok(result);      
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult AddTransaction(Transaction transaction)
        {         
            // Get the total points available for the payer in the transaction.
            var pointsAvailable = _transactionRepository.GetTransactions().Where(x => x.Payer == transaction.Payer).Sum(t => t.Points);

            // If points are being subtracted, make sure they won't cause the payer's points to go negative.
            if (transaction.Points < 0 && pointsAvailable - (transaction.Points * -1) < 0)
            {
                return BadRequest("Action would result in negative point value for payer.");
            }

            _transactionRepository.AddTransaction(transaction);
           
            return Ok();
        }
    }
}

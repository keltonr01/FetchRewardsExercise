using Moq;
using System;
using Xunit;
using FetchRewardsExercise.Repositories;
using FetchRewardsExercise.Models;
using System.Collections.Generic;
using FetchRewardsExercise.Controllers;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace FetchRewardsExercise.Tests
{
    public class TransactionTests
    {      
        [Fact]
        public void AddTransaction_ReturnsOk()
        {
            // Create the test data.
            var testTransaction = new Transaction
            {
                Payer = "DANNON",
                Points = 1000,
                Timestamp = DateTime.Parse("2020-11-02T14:00:00Z")
            };

            var transactions = TestHelpers.GetTestTransactions();

            var transactionRepo = new Mock<ITransactionRepository>();

            transactionRepo.Setup(repo => repo.GetTransactions())
             .Returns(transactions);

            transactionRepo.Setup(repo => repo.AddTransaction(It.IsAny<Transaction>()))
                .Verifiable();

            var controller = new TransactionsController(transactionRepo.Object);
          
            var result = controller.AddTransaction(testTransaction);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void AddTransaction_ReturnsBadRequest_PayerBelowZero()
        {
            var testTransaction = new Transaction
            {
                Payer = "DANNON",
                Points = -5000,
                Timestamp = DateTime.Parse("2020-11-02T14:00:00Z")
            };

            var transactions = TestHelpers.GetTestTransactions();

            var transactionRepo = new Mock<ITransactionRepository>();

            transactionRepo.Setup(repo => repo.GetTransactions())
             .Returns(transactions);

            transactionRepo.Setup(repo => repo.AddTransaction(It.IsAny<Transaction>()))
                .Verifiable();

            var controller = new TransactionsController(transactionRepo.Object);

            var result = controller.AddTransaction(testTransaction);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Action would result in negative point value for payer.", badRequestResult.Value);
        }

        [Fact]
        public void AddTransactionNegative_ReturnsBadRequest_PayerDoesNotExist()
        {
            var testTransaction = new Transaction
            {
                Payer = "Test",
                Points = -1000,
                Timestamp = DateTime.Parse("2020-11-02T14:00:00Z")
            };

            var transactions = TestHelpers.GetTestTransactions();

            var transactionRepo = new Mock<ITransactionRepository>();

            transactionRepo.Setup(repo => repo.GetTransactions())
             .Returns(transactions);

            transactionRepo.Setup(repo => repo.AddTransaction(It.IsAny<Transaction>()))
                .Verifiable();

            var controller = new TransactionsController(transactionRepo.Object);

            var result = controller.AddTransaction(testTransaction);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Action would result in negative point value for payer.", badRequestResult.Value);
        }

        
    }
}

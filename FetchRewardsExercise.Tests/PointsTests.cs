using FetchRewardsExercise.Controllers;
using FetchRewardsExercise.Models;
using FetchRewardsExercise.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace FetchRewardsExercise.Tests
{
    public class PointsTests
    {      
        [Fact]
        public void SpendPoints_ReturnsOk()
        {
            var pointsToSpend = new Points { Value = 5000 };
            var transactionRepo = new Mock<ITransactionRepository>();

            var transactions = TestHelpers.GetTestTransactions();

            transactionRepo.Setup(repo => repo.AddTransaction(It.IsAny<Transaction>()))
                .Callback((Transaction transaction) => transactions.Add(transaction));

            transactionRepo.Setup(repo => repo.AddRange(It.IsAny<IEnumerable<Transaction>>()))
                .Callback((IEnumerable<Transaction> transactionsToAdd) => transactions.AddRange(transactionsToAdd));

            transactionRepo.Setup(repo => repo.GetTransactions())
                .Returns(transactions);

            var controller = new PointsController(transactionRepo.Object);

            var result = controller.SpendPoints(pointsToSpend);

            var resultObj = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void SpendPoint_ReturnsBadRequest_NotEnoughPoints()
        {
            var pointsToSpend = new Points { Value = 50000 };
            var transactionRepo = new Mock<ITransactionRepository>();

            transactionRepo.Setup(repo => repo.AddTransaction(It.IsAny<Transaction>()))
                .Verifiable();
            transactionRepo.Setup(repo => repo.GetTransactions())
                .Returns(TestHelpers.GetTestTransactions());

            var controller = new PointsController(transactionRepo.Object);

            var result = controller.SpendPoints(pointsToSpend);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal("User does not have enough points.", badRequestResult.Value);
        }

        [Fact]
        public void GetPointsBalance_ReturnsOk_AfterSpendPoints()
        {
            var pointsToSpend = new Points { Value = 5000 };

            var transactionRepo = new Mock<ITransactionRepository>();

            var transactions = TestHelpers.GetTestTransactions();

            transactionRepo.Setup(repo => repo.AddTransaction(It.IsAny<Transaction>()))
                .Callback((Transaction transaction) => transactions.Add(transaction));

            transactionRepo.Setup(repo => repo.AddRange(It.IsAny<IEnumerable<Transaction>>()))
                .Callback((IEnumerable<Transaction> transactionsToAdd) => transactions.AddRange(transactionsToAdd));

            transactionRepo.Setup(repo => repo.GetTransactions())
                .Returns(transactions);

            var expectedPayerBalances = new Dictionary<string, int>()
            {
                { "DANNON", 1000 },
                { "UNILEVER", 0 },
                { "MILLER COORS", 5300 }
            };

            var controller = new PointsController(transactionRepo.Object);

            controller.SpendPoints(pointsToSpend);

            var result = controller.GetBalance().Result;

            var resultObj = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedPayerBalances, (Dictionary<string, int>)resultObj.Value);
        }

    }
}

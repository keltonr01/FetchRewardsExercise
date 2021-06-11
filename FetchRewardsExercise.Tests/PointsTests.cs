using FetchRewardsExercise.Controllers;
using FetchRewardsExercise.Models;
using FetchRewardsExercise.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FetchRewardsExercise.Tests
{
    public class PointsTests
    {
        private readonly ITestOutputHelper output;

        public PointsTests(ITestOutputHelper testOutputHelper)
        {
            output = testOutputHelper;
        }
         
        [Fact]
        public void SpendPoints_ReturnsOk()
        {
            var pointsToSpend = new Points { Value = 5000 };
            var transactionRepo = new Mock<ITransactionRepository>();

            transactionRepo.Setup(repo => repo.AddTransaction(It.IsAny<Transaction>()))
                .Verifiable();
            transactionRepo.Setup(repo => repo.GetTransactions())
                .Returns(TestHelpers.GetTestTransactions());

            var controller = new PointsController(transactionRepo.Object);

            var result = controller.SpendPoints(pointsToSpend);
            
            var resultObj = Assert.IsType<OkObjectResult>(result);
        }
    }
}

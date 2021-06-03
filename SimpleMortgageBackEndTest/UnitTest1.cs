using Newtonsoft.Json;
using SimpleMortgageBackend.Models;
using SimpleMortgageBackend.Data;
using SimpleMortgageBackend.Controllers;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace SimpleMortgageBackEndTest
{
    public class UnitTest1
    {
        private readonly SimpleMortgageDbContext _context;
        public UnitTest1()
        {
            _context = new SimpleMortgageDbContext(new DbContextOptionsBuilder<SimpleMortgageDbContext>()
                           .UseInMemoryDatabase(databaseName: "SimpleMortgageDatabase")
                           .Options);

            /* create some testing data */
            _context.Products.AddRange(
                                         new Product
                                         {
                                             Lender = "Bank A",
                                             InterestRate = 2,
                                             InterestTerm = "Variale",
                                             LTV = 60
                                         },
                                         new Product
                                         {
                                             Lender = "Bank B",
                                             InterestRate = 3,
                                             InterestTerm = "Fixed",
                                             LTV = 60
                                         },
                                         new Product
                                         {
                                             Lender = "Bank C",
                                             InterestRate = 4,
                                             InterestTerm = "Variale",
                                             LTV = 90
                                         },
                                         new Product
                                         {
                                             Lender = "Bank D",
                                             InterestRate = 5,
                                             InterestTerm = "Fixed",
                                             LTV = 80
                                         }
                                    );
            _context.SaveChanges();
        }


        [Fact]
        public async Task TestCreateApplicantIfNotExists()
        {
            // Arrange
            ApplicantsController controller = new ApplicantsController(_context);

            // Act
            var applicant = new Applicant
            {
                FirstName = "Peter",
                LastName = "Lansley",
                Email = "peter.lansley@xmail.xom",
                DOB = new DateTime(2020, 2, 11)                
            };
            
            var actionResult = await controller.PostApplicant(applicant);
            var result = actionResult.Result as CreatedAtActionResult;
            var model = result.Value as Applicant;

            // Assert
            Assert.Equal(1, model.Id);
            Assert.Equal(applicant.FirstName, model.FirstName);
            Assert.Equal(applicant.LastName, model.LastName);
            Assert.Equal(applicant.Email, model.Email);
            Assert.Equal(applicant.DOB, model.DOB);

            var actionResult2 = await controller.PostApplicant(applicant);
            var result2 = actionResult2.Result as ObjectResult;            
            var model2 = result2.Value as Applicant;

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result2.StatusCode);
            Assert.Equal(1, model2.Id);
            Assert.Equal(applicant.FirstName, model2.FirstName);
            Assert.Equal(applicant.LastName, model2.LastName);
            Assert.Equal(applicant.Email, model2.Email);
            Assert.Equal(applicant.DOB, model2.DOB);
        }

        [Fact]
        public async Task TestApplicantUnder18()
        {
            // Arrange
            ApplicantsController controller = new ApplicantsController(_context);

            // Act
            var applicant = new Applicant
            {
                FirstName = "Mary",
                LastName = "Lansley",
                Email = "mary.lansley@xmail.xom",
                DOB = new DateTime(2010, 1, 1)
            };

            var actionResult = await controller.PostApplicant(applicant);
            var objectResult = actionResult.Result as CreatedAtActionResult;
            var model = objectResult.Value as Applicant;

            // Assert
            Assert.Equal(applicant.FirstName, model.FirstName);
            Assert.Equal(applicant.LastName, model.LastName);
            Assert.Equal(applicant.Email, model.Email);
            Assert.Equal(applicant.DOB, model.DOB);

            int propertyval = 100;
            int depositamt = 20;

            var actionResult2 = await controller.GetApplicant(model.Id, propertyval, depositamt);
            var objectResult2 = actionResult2.Result as ObjectResult;
            var result2 = objectResult2.Value as ValidationProblemDetails;
            
            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult2.StatusCode);
            Console.WriteLine("TestApplicantUnder18: Error Message.");
            Console.WriteLine(JsonConvert.SerializeObject(result2.Errors, Formatting.Indented));
            Console.WriteLine("***********************************");
        }

        [Fact]
        public async Task TestLoanToValueNotExceed90()
        {
            // Arrange
            ApplicantsController controller = new ApplicantsController(_context);

            // Act
            var applicant = new Applicant
            {
                FirstName = "John",
                LastName = "Lansley",
                Email = "john.lansley@xmail.xom",
                DOB = new DateTime(2000, 1, 1)
            };

            var actionResult = await controller.PostApplicant(applicant);
            var objectResult = actionResult.Result as CreatedAtActionResult;
            var model = objectResult.Value as Applicant;

            // Assert
            Assert.Equal(applicant.FirstName, model.FirstName);
            Assert.Equal(applicant.LastName, model.LastName);
            Assert.Equal(applicant.Email, model.Email);
            Assert.Equal(applicant.DOB, model.DOB);

            int propertyval = 100;
            int depositamt = 5;

            var actionResult2 = await controller.GetApplicant(model.Id, propertyval, depositamt);
            var objectResult2 = actionResult2.Result as ObjectResult;
            var result2 = objectResult2.Value as ValidationProblemDetails;

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult2.StatusCode);
            Console.WriteLine("TestLoanToValueNotExceed90: Error Message.");
            Console.WriteLine(JsonConvert.SerializeObject(result2.Errors, Formatting.Indented));
            Console.WriteLine("***********************************");

        }
    }
}

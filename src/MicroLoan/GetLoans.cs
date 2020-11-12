using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using MicroLoan.Models;

namespace MicroLoan
{
    public partial class LoanOperations
    {
        [FunctionName(nameof(GetAllLoans))]
        public IActionResult GetAllLoans(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "loans/")] HttpRequest req,
            [CosmosDB(
                databaseName: "microloandb",
                collectionName: "loanbundles",
                ConnectionStringSetting = "MicroLoanConnection",
                SqlQuery = "SELECT * FROM loanbundles"
            )] IEnumerable<LoanBundle> documents,
            ILogger log)
        {
            if (!documents.Any())
            {
                log.LogInformation($"No loan bundle found.");
                return new NotFoundResult();
            }

            var doc = documents.ToList();
            return new OkObjectResult(doc);
        }

        [FunctionName(nameof(GetLoanById))]
        public IActionResult GetLoanById(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "loans/{Id}")] HttpRequest req,
            [CosmosDB(
                databaseName: "microloandb",
                collectionName: "loanbundles",
                ConnectionStringSetting = "MicroLoanConnection",
                //SqlQuery = "SELECT * FROM loanbundles lb WHERE LOWER(lb.loanName) = LOWER({loanName})"
                SqlQuery = "SELECT * FROM loanbundles lb WHERE lb.id = {Id}"
            )] IEnumerable<LoanBundle> documents,
            string Id,
            ILogger log)
        {
            if (!documents.Any())
            {
                log.LogInformation($"Bundle for {Id} not found.");
                return new NotFoundResult();
            }

            LoanBundle doc = documents.Single();
            return new OkObjectResult(doc);
        }

        [FunctionName(nameof(GetLoansForUser))]
        public IActionResult GetLoansForUser(
           [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "loans/user/{userId}")] HttpRequest req,
           [CosmosDB(
                 databaseName: "microloandb",
                collectionName: "loanbundles",
                ConnectionStringSetting = "MicroLoanConnection",
                SqlQuery = "SELECT c.userId, c.loanName, c.description FROM c where c.userId = {userId}"
            )] IEnumerable<Document> documents,
           string userId,
           ILogger log)
        {
            string twitterHandle = GetAccountInfo().HashedID;
            if (string.IsNullOrEmpty(twitterHandle) || twitterHandle != userId)
            {
                log.LogInformation("Client is not authorized");
                return new UnauthorizedResult();
            }

            if (!documents.Any())
            {
                log.LogInformation($"No loans for user: '{userId}'  found.");

                return new NotFoundResult();
            }
            var results = documents.Select(d => new
            {
                userId = d.GetPropertyValue<string>("userId"),
                loanName = d.GetPropertyValue<string>("loanName"),
                description = d.GetPropertyValue<string>("description"),
            });
            return new OkObjectResult(results);
        }
    }
}
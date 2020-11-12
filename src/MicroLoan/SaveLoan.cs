using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MicroLoan.Models;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MicroLoan
{
    public partial class LoanOperations
    {
        [FunctionName(nameof(SaveLoan))]
        public async Task<IActionResult> SaveLoan(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "loans")] HttpRequest req,
            [CosmosDB(
                databaseName: "microloandb",
                collectionName: "loanbundles",
                ConnectionStringSetting = "MicroLoanConnection",
                CreateIfNotExists = true
            )] IAsyncCollector<LoanBundle> documents,
            Binder binder,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var loanDocument = JsonConvert.DeserializeObject<LoanBundle>(requestBody);

                if (!ValidatePayLoad(loanDocument, req, out ProblemDetails problems))
                {
                    log.LogError(problems.Detail);
                    return new BadRequestObjectResult(problems);
                }

                string handle = GetAccountInfo().HashedID;
                loanDocument.UserId = handle;

                await documents.AddAsync(loanDocument);

                string payload = req.Host + loanDocument.LoanName;

                //await GenerateQRCodeAsync(linkDocument, req, binder);

                return new CreatedResult($"/{loanDocument.LoanName}", loanDocument);
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                log.LogError(ex, ex.Message);

                ProblemDetails exceptionDetail = new ProblemDetails
                {
                    Title = "Could not create loan bundle",
                    Detail = "Loan name already in use",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/microloan/clientissue",
                    Instance = req.Path
                };
                return new BadRequestObjectResult(exceptionDetail);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        

        private static bool ValidatePayLoad(LoanBundle loanDocument, HttpRequest req, out ProblemDetails problems)
        {
            bool isValid = (loanDocument != null);
            problems = null;

            if (!isValid)
            {
                problems = new ProblemDetails()
                {
                    Title = "Payload is invalid",
                    Detail = "No links provided",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/microloan/clientissue",
                    Instance = req.Path
                };
            }
            return isValid;
        }
    }
}
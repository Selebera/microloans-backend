using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroLoan.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MicroLoan.Helpers;


namespace MicroLoan
{
    public partial class LoanOperations
    {
        protected IHttpContextAccessor _contextAccessor;
        //protected const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        protected Hasher _hasher;
        protected TelemetryClient _telemetryClient;
 
        public LoanOperations(IHttpContextAccessor contextAccessor, Hasher hasher)
        {
            _contextAccessor = contextAccessor;
            _hasher = hasher;
            TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.TelemetryInitializers.Add(new HeaderTelemetryInitializer(contextAccessor));
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        protected UserInfo GetAccountInfo()
        {
            var socialIdentities = _contextAccessor.HttpContext.User
                  .Identities.Where(id => !id.AuthenticationType.Equals("WebJobsAuthLevel", StringComparison.InvariantCultureIgnoreCase));

            if (socialIdentities.Any())
            {
                var provider = _contextAccessor.HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].FirstOrDefault();

                var primaryIdentity = socialIdentities.First();
                var email = primaryIdentity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userInfo = new UserInfo(provider, _hasher.HashString(email));

                var evt = new EventTelemetry("UserInfo Retrieved");
                evt.Properties.Add("Provider", provider);
                evt.Properties.Add("EmailAquired", (string.IsNullOrEmpty(email).ToString()));
                _telemetryClient.TrackEvent(evt);

                return userInfo;
            }

            return UserInfo.Empty; ;
        }
    }
}
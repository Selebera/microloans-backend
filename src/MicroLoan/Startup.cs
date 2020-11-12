using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.Extensibility;
using MicroLoan.Helpers;

[assembly: FunctionsStartup(typeof(MicroLoan.Startup))]
namespace MicroLoan
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<ITelemetryInitializer, HeaderTelemetryInitializer>();
            builder.Services.AddSingleton<Hasher>();
        }
    }
}

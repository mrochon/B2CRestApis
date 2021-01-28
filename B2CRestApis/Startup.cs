using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace B2CRestApis
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfidentialClientApplication>((svc) =>
            {
                ConfidentialClientApplicationOptions options = new ConfidentialClientApplicationOptions();
                Configuration.Bind("AAD", options);
                return ConfidentialClientApplicationBuilder
                    .CreateWithApplicationOptions(options)
                    .Build();
            });
            services.AddHttpClient("graph", (s,c) =>
            {
                var msal = s.GetService<IConfidentialClientApplication>();
                var tokens = msal.AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" }).ExecuteAsync().Result;
                c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
                c.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient("O365", (s, c) =>
            {
                var options = new ConfidentialClientApplicationOptions();
                Configuration.Bind("O365", options);
                var msal = ConfidentialClientApplicationBuilder
                    .CreateWithApplicationOptions(options)
                    .Build();
                var tokens = msal.AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" }).ExecuteAsync().Result;
                c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
                c.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

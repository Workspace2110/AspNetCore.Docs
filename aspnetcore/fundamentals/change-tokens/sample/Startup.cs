using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using static ChangeTokenSample.Utilities.Utilities;

namespace ChangeTokenSample
{
    public class Startup
    {
        private byte[] _appsettingsHash = new byte[20];
        private byte[] _appsettingsEnvHash = new byte[20];

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            #region snippet1
            services.AddSingleton<IConfigurationMonitor, ConfigurationMonitor>();
            #endregion

            #region snippet4
            services.AddMemoryCache();
            services.AddSingleton<FileService>();
            #endregion

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }
        public void Configure(IApplicationBuilder app, IConfiguration config, IHostingEnvironment env)
        {
            #region snippet2
            ChangeToken.OnChange(
                () => config.GetReloadToken(),
                (state) => InvokeChanged(state),
                env);
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvc();
        }

        #region snippet3
        private void InvokeChanged(IHostingEnvironment env)
        {
            byte[] appsettingsHash = ComputeHash("appSettings.json");
            byte[] appsettingsEnvHash = 
                ComputeHash($"appSettings.{env.EnvironmentName}.json");

            if (!_appsettingsHash.SequenceEqual(appsettingsHash) || 
                !_appsettingsEnvHash.SequenceEqual(appsettingsEnvHash))
            {
                _appsettingsHash = appsettingsHash;
                _appsettingsEnvHash = appsettingsEnvHash;

                WriteConsole("Configuration changed (Simple Startup Change Token)");
            }
        }
        #endregion
    }
}

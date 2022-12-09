using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Blazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DXApplication1.Blazor.Server.Services;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DXApplication1.Module.BusinessObjects;
using DevExpress.ExpressApp.Core;
using DXApplication1.Module;
using DXApplication1.Module.Blazor;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using Microsoft.EntityFrameworkCore;
using DevExpress.ExpressApp.ApplicationBuilder;

namespace DXApplication1.Blazor.Server {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            services.AddScoped<CircuitHandler, CircuitHandlerProxy>();

            services.AddXaf(Configuration, builder => {
                builder.UseApplication<DXApplication1BlazorApplication>();
                builder.Modules
                    .Add<DXApplication1BlazorModule>()
                    .Add<DXApplication1Module>();
                builder.ObjectSpaceProviders
                    .AddSecuredEFCore().WithDbContext<DXApplication1EFCoreDbContext>((serviceProvider, options) => {
                        // Uncomment this code to use an in-memory database. This database is recreated each time the server starts. With the in-memory database, you don't need to make a migration when the data model is changed.
                        // Do not use this code in production environment to avoid data loss.
                        // We recommend that you refer to the following help topic before you use an in-memory database: https://docs.microsoft.com/en-us/ef/core/testing/in-memory
                        //options.UseInMemoryDatabase("InMemory");
                        string connectionString = null;
                        if (Configuration.GetConnectionString("ConnectionString") != null) {
                            connectionString = Configuration.GetConnectionString("ConnectionString");
                        }
#if EASYTEST
                    if(Configuration.GetConnectionString("EasyTestConnectionString") != null) {
                        connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                    }
#endif
                        ArgumentNullException.ThrowIfNull(connectionString);
                        options.UseSqlServer(connectionString);
                        options.UseChangeTrackingProxies();
                        options.UseObjectSpaceLinkProxies();
                        options.UseLazyLoadingProxies();
                    })
                    .AddNonPersistent();

                builder.Security
                    .UseIntegratedMode(options => {
                        options.RoleType = typeof(PermissionPolicyRole);
                        options.UserType = typeof(ApplicationUser);
                        options.UserLoginInfoType = typeof(ApplicationUserLoginInfo);
                        options.SupportNavigationPermissionsForTypes = false;
                        options.Events.OnSecurityStrategyCreated = securityStrategy => {
                            ((SecurityStrategy)securityStrategy).AnonymousAllowedTypes.Add(typeof(ApplicationUser));
                            ((SecurityStrategy)securityStrategy).AnonymousAllowedTypes.Add(typeof(PermissionPolicyRole));
                            ((SecurityStrategy)securityStrategy).AnonymousAllowedTypes.Add(typeof(ApplicationUserLoginInfo));
                        };
                    })
                    .AddPasswordAuthentication(options => options.IsSupportChangePassword = true);
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
                options.LoginPath = "/LoginPage";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseXaf();
            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}

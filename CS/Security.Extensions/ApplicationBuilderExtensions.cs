using DevExpress.ExpressApp.ApplicationBuilder;
using Microsoft.Extensions.DependencyInjection;
using Security.Extensions;
using Security.Extensions.Services;
using System;

namespace Security.Extensions {
    public class SecurityExtensionsOptions {
        public CreateSecuritySystemUser CreateSecuritySystemUser { get; set; }
    }
}

namespace DevExpress.ExpressApp.Blazor.ApplicationBuilder {
    public static class ApplicationBuilderExtensions {
        // Adds the SecurityExtensionsModule to the application and configures the required services.
        public static IModuleBuilder<IBlazorApplicationBuilder> AddSecurityExtensions(this IModuleBuilder<IBlazorApplicationBuilder> builder,
            Action<SecurityExtensionsOptions> configureOptions) {
            SecurityExtensionsOptions options = new();
            configureOptions.Invoke(options);
            ArgumentNullException.ThrowIfNull(options.CreateSecuritySystemUser);

            builder.Add<SecurityExtensionsModule>();
            builder.Context.Services.Configure<SecurityExtensionsOptions>(o => o.CreateSecuritySystemUser = options.CreateSecuritySystemUser);
            builder.Context.Services.AddScoped<RestorePasswordService>();
            builder.Context.Services.AddScoped<UserRegistrationService>();
            return builder;
        }
    }
}

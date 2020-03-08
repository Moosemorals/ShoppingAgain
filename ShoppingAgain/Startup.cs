using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Events;

namespace ShoppingAgain
{
    public class Startup
    {
        private static readonly RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.Env = env;

            using var shopingDB = new ShoppingContext();
            shopingDB.Database.EnsureCreated();

            using var eventsDB = new EventContext();
            eventsDB.Database.EnsureCreated();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Setup DI
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<EventService>();

            services.AddScoped<ShoppingContext>();
            services.AddScoped<ShoppingService>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                        .AddCookie(options =>
            {
                options.LoginPath = Names.UserLoginPath;
                options.AccessDeniedPath = Names.UserDeniedPath;
                options.LogoutPath = Names.UserLogoutPath;

                options.Cookie = new CookieBuilder
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    IsEssential = true,

                };
                options.SlidingExpiration = true;
            });
            services.AddHttpContextAccessor();

            // Setup MVC
            IMvcBuilder builder = services.AddMvc(options => options.EnableEndpointRouting = false);
            if (Env.IsDevelopment())
            {
                builder.AddRazorRuntimeCompilation();
            }

            // Setup Database
            services.AddEntityFrameworkSqlite()
                .AddDbContext<ShoppingContext>()
                .AddDbContext<EventContext>();
        }

        private string BuildNonce(int chars)
        {
            byte[] raw = new byte[chars];
            RNG.GetBytes(raw);

            return System.Convert.ToBase64String(raw);
        }

        public void Configure(IApplicationBuilder app)
        {

            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Add authentication
            app.UseAuthentication();

            // Add CSP header
            app.Use(async (ctx, next) =>
            {
                string n = BuildNonce(12);
                ctx.Items.Add("script-nonce", n);
                ctx.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'nonce-" + n + "';");
                await next();
            });

            app.UseMiddleware<ListsMiddleware>();

            // Serve static files from wwwroot
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}

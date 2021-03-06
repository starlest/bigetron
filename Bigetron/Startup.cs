﻿namespace Bigetron
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using AspNet.Security.OpenIdConnect.Primitives;
    using AutoMapper;
    using Core.Domain.Users;
    using Data;
    using Data.Abstract;
    using Data.Repositories;
    using FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.IdentityModel.Tokens;
    using Services.Articles;
    using Microsoft.AspNetCore.Rewrite;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add a reference to the Configuration object for DI
            services.AddSingleton<IConfiguration>(c => Configuration);

            // Add framework services.
            services.AddMvc().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            // Add EntityFramework's Identity support.
            services.AddEntityFramework();

            // Add Identity Services & Stores
            services.AddIdentity<User, IdentityRole>(config =>
                {
                    config.User.RequireUniqueEmail = true;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Cookies.ApplicationCookie.AutomaticChallenge = false;
                }).AddEntityFrameworkStores<BTRDbContext>()
                .AddDefaultTokenProviders();

            // Add ApplicationDbContext.
            services.AddDbContext<BTRDbContext>(
                options =>
                {
                    options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"],
                        b => b.MigrationsAssembly("Bigetron"));
                    // Register the entity sets needed by OpenIddict.
                    // Note: use the generic overload if you need
                    // to replace the default OpenIddict entities.
                    options.UseOpenIddict();
                });

            // Add Repository
            services.AddScoped<IRepository, EntityFrameworkRepository<BTRDbContext>>();

            // Add Services
            services.AddScoped<IArticleService, ArticleService>();

            // Add Message Service for DI
            services.AddSingleton<MessageService>();

            // Register the OpenIddict services.
            services.AddOpenIddict()
                // Register the Entity Framework stores.
                .AddEntityFrameworkCoreStores<BTRDbContext>()
                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                .AddMvcBinders()
                // Use Json Web Tokens (JWT)
                .UseJsonWebTokens()
                // Set a custom token endpoint (default is /connect/token)
                .EnableTokenEndpoint(Configuration["Authentication:OpenIddict:TokenEndPoint"])
                // Set a custom auth endpoint (default is connect/authorize)
                .EnableAuthorizationEndpoint(Configuration["Authentication:OpenIddict:AuthorizationEndPoint"])
                // Allow client applications to use the grant_type=password flow.
                .AllowPasswordFlow()
                // Enable support for both authorization and implicit flows
                .AllowAuthorizationCodeFlow()
                .AllowImplicitFlow()
                // Allow the client to refresh tokens.
                .AllowRefreshTokenFlow()
                // Set the refresh tokens lifetime
                .SetRefreshTokenLifetime(TimeSpan.FromDays(1))
                // Disable the HTTPS requirement (not recommended in production)
                .DisableHttpsRequirement()
                // Register a new ephemeral key for development.
                // We will register a X.509 certificate in production.
                .AddEphemeralSigningKey();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Prerender: Add Options.
            services.AddOptions();

            // Add ApplicationDbContext's DbSeeder
            services.AddSingleton<DbSeeder>();

            // Add AutoMapper for DI
            services.AddAutoMapper();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            DbSeeder dbSeeder)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Configure a rewrite rule to auto-lookup for standard files such index.html.
            app.UseDefaultFiles();

            // Serve static files (html, css, js, images & more)
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    // Disable caching for all static files.
                    context.Context.Response.Headers["Cache-Control"] =
                        Configuration["StaticFiles:Headers:Cache-Control"];
                    context.Context.Response.Headers["Pragma"] = Configuration["StaticFiles:Headers:Pragma"];
                    context.Context.Response.Headers["Expires"] = Configuration["StaticFiles:Headers:Expires"];
                }
            });

            // Rewrite requests
            app.UseRewriter(new RewriteOptions().AddIISUrlRewrite(env.ContentRootFileProvider, "web.config"));

            // Enable CORS for middleware
            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            );

            // Add the AspNetCore.Identity middleware (required for external auth providers)
            // IMPORTANT: This must be placed *BEFORE* OpenIddict and any external provider's middleware
            app.UseIdentity();

            // Add OpenIddict middleware
            // Note: UseOpenIddict() must be registered after app.UseIdentity() and the external social providers
            app.UseOpenIddict();

            // Add the Jwt Bearer Header Authentication to validate Tokens
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,
                Authority = Configuration["Authentication:OpenIddict:Authority"],
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false
                }
            });

            // Add MVC to the pipeline
            app.UseMvc();

            // Seed the database (if needed)
            try
            {
                dbSeeder.SeedAsync().Wait();
            }
            catch (AggregateException e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}
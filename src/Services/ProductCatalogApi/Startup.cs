using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductCatalogApi.Data;

namespace ProductCatalogApi
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
            services.Configure<CatalogSettings>(Configuration);

            //var hostname = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "mssqlserver";
            //var password = Environment.GetEnvironmentVariable("SA_PASSWORD") ?? "MyProduct!123";
            //var connectionString = $"Server={hostname};Database=CatalogDb;User ID=sa;Password={password};";

            var dataBaseServer = Configuration["DatabaseServer"] ?? @"(localdb)\mssqllocaldb";
            var databasePort   = Configuration["DataBasePort"] != null ?  "," + Configuration["DataBasePort"] : "";
            var dataBaseName   = Configuration["DatabaseName"] ?? "Catalog_Db";
            var user           = Configuration["DatabaseUser"] ?? "sa";
            var password       = Configuration["DatabasePassword"] ?? "1234";

            //var connectionString = $"Server={dataBaseServer}{databasePort};Database={dataBaseName};User={user};Password={password}";
            var connectionString = $"Server=192.168.99.100,1445;Database=CatalogDb;User ID=sa;Password=ProductApi@;";


            //Configuring Connection Resiliency: 
            //https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
            services.AddDbContext<CatalogContext>(options =>
            {
                options.UseSqlServer(connectionString,
                            sqlServerOptionsAction: sqlOptions =>
                            {
                                sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                            });
                // Changing default behavior when client evaluation occurs to throw. 
                // Default in EF Core would be to log a warning when client evaluation is performed.
                //Check Client vs. Server evaluation: https://docs.microsoft.com/en-us/ef/core/querying/client-eval
                options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            });

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "ShoesOnContainers - ProductCatalog HTTP API",
                    Version = "v1",
                    Description = "The Catalog Microservice HTTP API. This is a Data-Driven/CRUD microservice sample",
                    TermsOfService = "Terms Of Service"
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc();

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<CatalogContext>();
                // Seed the database.
                CatalogDataSeed.SeedAsync(context).Wait();
            }
        }
    }
}

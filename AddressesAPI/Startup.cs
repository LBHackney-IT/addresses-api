using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AddressesAPI.Infrastructure;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase;
using AddressesAPI.V1.UseCase.Interfaces;
using AddressesAPI.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AddressesAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private static List<ApiVersionDescription> _apiVersions { get; set; }
        private const string ApiName = "Addresses API";

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    }
                )
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0); ;
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true; // assume that the caller wants the default version if they don't specify
                o.ApiVersionReader = new UrlSegmentApiVersionReader(); // read the version number from the url segment header)
            });

            services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Token",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Your Hackney API Key",
                        Name = "X-Api-Key",
                        Type = SecuritySchemeType.ApiKey
                    });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Token" }
                        },
                        new List<string>()
                    }
                });

                //Looks at the APIVersionAttribute [ApiVersion("x")] on controllers and decides whether or not
                //to include it in that version of the swagger document
                //Controllers must have this [ApiVersion("x")] to be included in swagger documentation!!
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    apiDesc.TryGetMethodInfo(out var methodInfo);

                    var versions = methodInfo?
                        .DeclaringType?.GetCustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions).ToList();

                    return versions?.Any(v => $"{v.GetFormattedApiVersion()}" == docName) ?? false;
                });

                //Get every ApiVersion attribute specified and create swagger docs for them
                foreach (var apiVersion in _apiVersions)
                {
                    var version = $"v{apiVersion.ApiVersion.ToString()}";
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = $"{ApiName}-api {version}",
                        Version = version,
                        Description = $"{ApiName} version {version}. Please check older versions for depreciated endpoints."
                    });
                }

                c.CustomSchemaIds(x => x.FullName);
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });
            ConfigureDbContext(services);
            RegisterV1Gateways(services);
            RegisterV2Gateways(services);
            RegisterV1UseCases(services);
            RegisterV2UseCases(services);
        }

        private static void ConfigureDbContext(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? "Host=;Database=;";

            services.AddDbContext<AddressesContext>(
                opt => opt.UseNpgsql(connectionString));
        }

        private static void RegisterV1Gateways(IServiceCollection services)
        {
            services.AddScoped<IAddressesGateway, AddressesGateway>();
            services.AddScoped<ICrossReferencesGateway, CrossReferencesGateway>();
        }

        private static void RegisterV2Gateways(IServiceCollection services)
        {
            services.AddScoped<V2.Gateways.IAddressesGateway, V2.Gateways.AddressesGateway>();
            services.AddScoped<V2.Gateways.ICrossReferencesGateway, V2.Gateways.CrossReferencesGateway>();
        }

        private static void RegisterV1UseCases(IServiceCollection services)
        {
            services.AddScoped<ISearchAddressValidator, SearchAddressValidator>();
            services.AddScoped<ISearchAddressUseCase, SearchAddressUseCase>();
            services.AddScoped<IGetAddressRequestValidator, GetAddressRequestValidator>();
            services.AddScoped<IGetCrossReferenceRequestValidator, GetCrossReferenceRequestValidator>();
            services.AddScoped<IGetSingleAddressUseCase, GetSingleAddressUseCase>();
            services.AddScoped<IGetAddressCrossReferenceUseCase, GetAddressCrossReferenceUseCase>();
        }

        private static void RegisterV2UseCases(IServiceCollection services)
        {
            services.AddScoped<V2.UseCase.Interfaces.ISearchAddressValidator, V2.UseCase.SearchAddressValidator>();
            services.AddScoped<V2.UseCase.Interfaces.ISearchAddressUseCase, V2.UseCase.SearchAddressUseCase>();
            services.AddScoped<V2.UseCase.Interfaces.IGetAddressRequestValidator, V2.UseCase.GetAddressRequestValidator>();
            services.AddScoped<V2.UseCase.Interfaces.IGetCrossReferenceRequestValidator, V2.UseCase.GetCrossReferenceRequestValidator>();
            services.AddScoped<V2.UseCase.Interfaces.IGetSingleAddressUseCase, V2.UseCase.GetSingleAddressUseCase>();
            services.AddScoped<V2.UseCase.Interfaces.IGetAddressCrossReferenceUseCase, V2.UseCase.GetAddressCrossReferenceUseCase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithMethods("GET"));

        if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //Get All ApiVersions,
            var api = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
            _apiVersions = api.ApiVersionDescriptions.ToList();

            //Swagger ui to view the swagger.json file
            app.UseSwaggerUI(c =>
            {
                foreach (var apiVersionDescription in _apiVersions)
                {
                    //Create a swagger endpoint for each swagger version
                    c.SwaggerEndpoint($"{apiVersionDescription.GetFormattedApiVersion()}/swagger.json",
                        $"{ApiName}-api {apiVersionDescription.GetFormattedApiVersion()}");
                }
            });
            app.UseSwagger();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // SwaggerGen won't find controllers that are routed via this technique.
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}


using IdentityModel.Client;
using SDK.Service;
using SKD.KitStatusFeed;

public static class DependencyInjection {
    public static IServiceCollection SetupSkdContext(this IServiceCollection services, string connectionString) {
        return services
            .AddPooledDbContextFactory<SkdContext>(
                options => options.UseSqlServer(connectionString, sqlOptions => {
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }))
            .AddScoped<SkdContext>(p => p.GetRequiredService<IDbContextFactory<SkdContext>>().CreateDbContext());
    }

    public static IServiceCollection SetupScopedServices(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        AppSettings appSettings
    ) {
        return services
            .AddScoped<KitService>(sp =>
                new KitService(sp.GetRequiredService<SkdContext>(), currentDate: DateTime.Now))
            .AddScoped<PcvService>()
            .AddScoped<ComponentService>()
            .AddScoped<DCWSResponseService>()
            .AddSingleton<DcwsService>(sp => new DcwsService(appSettings.DcwsServiceAddress))
            .AddScoped<StationService>()
            .AddScoped<ComponentSerialService>()
            .AddScoped<ShipmentService>()
            .AddScoped<ShipFileParser>()
            .AddScoped<BomService>()
            .AddScoped<PlantService>()
            .AddScoped<LotPartService>()
            .AddScoped<HandlingUnitService>()
            .AddScoped<QueryService>()
            .AddScoped<CustomQueryService>()
            .AddScoped<SummaryQueryService>()
            .AddScoped<VerifySerialService>()
            .AddScoped<ComponentStationService>()
            .AddScoped<KitStatusFeedService>()
            .AddScoped<PartnerStatusService>()
            .AddScoped<PcvXlsxParserService>();
    }

    public static IServiceCollection SetupSingletonServices(
        this IServiceCollection services,
        AppSettings appSettings
    ) => services.AddSingleton<AppSettings>(sp => appSettings);

    public static IServiceCollection SetupGraphqlServer(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        AppSettings appSettings
    ) {
        services
            .AddGraphQLServer()
            .RegisterDbContext<SkdContext>(DbContextKind.Pooled)
            .AddQueryType<Query>()
                .AddTypeExtension<ProjectionQuery>()
                .AddTypeExtension<SummaryQuery>()
                .AddTypeExtension<PartnerStatusQuery>()
            .AddMutationType<Mutation>()
                .AddTypeExtension<PartnerStatusMutation>()
                .AddTypeExtension<KitMutation>()
            .AddType<UploadType>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddInMemorySubscriptions()
            .ModifyRequestOptions(opt => {
                opt.IncludeExceptionDetails = environment.IsDevelopment();
                opt.ExecutionTimeout = TimeSpan.FromSeconds(appSettings.ExecutionTimeoutSeconds);
            })
            .AllowIntrospection(appSettings.AllowGraphqlIntrospection)
            .InitializeOnStartup();

        return services;
    }

    public static IServiceCollection SetupCors(this IServiceCollection services) {
        return services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()));
    }

    public static IServiceCollection SetupKitStatusFeedService(this IServiceCollection services, IConfiguration configuration) {
        var accessTokenOptions = configuration
            .GetSection("KitStatusFeed:AccessToken")
            .Get<AccessTokenOptions>() ?? throw new Exception("Failed to get KsfAccessTokenOptions");
        var kitStatusFeedUrl = configuration["KitStatusFeed:KitStatusFeedUrl"] ?? "";

        services.AddSingleton<AccessTokenOptions>(accessTokenOptions);

        services.AddHttpClient<KitStatusFeedService>("KitStatusFeedService", client => {
            client.BaseAddress = new Uri(kitStatusFeedUrl);
        }).AddClientAccessTokenHandler();

        services.AddAccessTokenManagement(options => {
            options.Client.Clients.Add("identity", new ClientCredentialsTokenRequest {
                Address = accessTokenOptions.TokenGenerationEndpoint,
                ClientId = accessTokenOptions.ClientId,
                ClientSecret = accessTokenOptions.ClientSecret,
                Scope = accessTokenOptions.Scope,
                GrantType = accessTokenOptions.GrantType
            });
        });


        return services;
    }
}

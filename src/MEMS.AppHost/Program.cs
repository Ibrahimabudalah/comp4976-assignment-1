var builder = DistributedApplication.CreateBuilder(args);

// JWT Secret Key for API token authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MEMS_Development_Only_Secure_Random_Key_COMP4976_Assign2_2026!";

// SQL Server instance orchestrated by Aspire with consistent dev password
var sqlPassword = builder.AddParameter("sql-password", "P@$$w0rd123!", secret: true);
var sqlServer = builder.AddSqlServer("sqlserver", sqlPassword)
    .WithDataVolume();

var sqlDb = sqlServer.AddDatabase("memsdb");

// ASP.NET WebAPI
var api = builder.AddProject<Projects.MEMS_Api>("api")
    .WithReference(sqlDb)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithHttpHealthCheck("/health")
    .WaitFor(sqlDb);

// Blazor Server Admin application
var admin = builder.AddProject<Projects.MEMS_Admin>("admin")
    .WithReference(sqlDb)
    .WithReference(api)
    .WithHttpHealthCheck("/health")
    .WaitFor(sqlDb)
    .WaitFor(api);

// Blazor WebAssembly Public site
builder.AddProject<Projects.MEMS_Public>("public")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();

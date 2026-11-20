var builder = DistributedApplication.CreateBuilder(args);

// JWT signing key: a secret parameter, never stored in source. Aspire generates a random key each run
// unless Parameters:jwt-key is configured, so tokens issued before a restart stop working (log in again).
var jwtKey = builder.AddParameter("jwt-key", new GenerateParameterDefault { MinLength = 64, Special = false }, secret: true);

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

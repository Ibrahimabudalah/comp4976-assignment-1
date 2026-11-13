var builder = DistributedApplication.CreateBuilder(args);

// SQL Server instance orchestrated by Aspire
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

var sqlDb = sqlServer.AddDatabase("memsdb");

// ASP.NET WebAPI
var api = builder.AddProject<Projects.MEMS_Api>("api")
    .WithReference(sqlDb)
    .WaitFor(sqlDb);

// Blazor Server Admin application
var admin = builder.AddProject<Projects.MEMS_Admin>("admin")
    .WithReference(sqlDb)
    .WithReference(api)
    .WaitFor(sqlDb)
    .WaitFor(api);

// Blazor WebAssembly Public site
builder.AddProject<Projects.MEMS_Public>("public")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();

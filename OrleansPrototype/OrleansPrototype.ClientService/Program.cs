using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Orleans.Runtime;
using SharedLibraries.SensorDataParser;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.UseOrleansClient(client =>
{
    //mclient.UseLocalhostClustering();

    client.UseAdoNetClustering(opts =>
    {
        opts.Invariant = "Microsoft.Data.SqlClient";
        opts.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;" +
            "Initial Catalog=mpr_orleans_a;Integrated Security=False;User Id=mpr_orleans_a_user;Password=p4ssw0rd;" +
            "Pooling=False;Max Pool Size=200;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True";
    });

    //client.UseMongoDBClient("mongodb://localhost:27017/");
    //client.UseMongoDBClustering(opts =>
    //{
    //    opts.DatabaseName = "mpr_orleans_a";
    //    opts.Strategy = Orleans.Providers.MongoDB.Configuration.MongoDBMembershipStrategy.SingleDocument;
    //});
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder => builder.AddRuntimeInstrumentation().AddAspNetCoreInstrumentation().AddHttpClientInstrumentation())
    .WithTracing(builder => builder.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation())
    .WithLogging();

builder.Services.AddSingleton<IDataParser, DataParser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

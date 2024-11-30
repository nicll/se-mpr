using SharedLibraries.Plots;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseOrleans(silo =>
{
    silo.UseLocalhostClustering();

    // configure ADO.NET SQL server clustering
    //silo.UseAdoNetClustering(opts =>
    //{
    //    opts.Invariant = "System.Data.SqlClient";
    //    opts.ConnectionString = @"Data Source=(localdb)\SQLEXPRESS;" +
    //        "Initial Catalog=mpr_orleans_a;Integrated Security=True;" +
    //        "Pooling=False;Max Pool Size=200;" +
    //        "Asynchronous Processing=True;MultipleActiveResultSets=True";
    //});

    // configure MongoDB client
    //silo.UseMongoDBClient("mongodb://localhost:27017/");

    // configure Orleans to use MongoDB client
    //silo.UseMongoDBClustering(opts =>
    //{
    //    opts.DatabaseName = "mpr_orleans_a";
    //    opts.Strategy = Orleans.Providers.MongoDB.Configuration.MongoDBMembershipStrategy.SingleDocument;
    //});

    silo.ConfigureLogging(log => log.AddConsole());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPlotGenerator, PlotGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

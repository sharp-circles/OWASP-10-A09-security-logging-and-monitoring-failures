using owasp._09.log4net.core.UseCases.CreateRoutine.Contracts;
using owasp._09.log4net.core.UseCases.CreateRoutine;
using System.Reflection;

var logRepository = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());
log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
var logger = log4net.LogManager.GetLogger(typeof(Program));

logger.Info("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();

    builder.Logging.AddLog4Net();

    builder.Services.AddControllers();

    builder.Services.AddSwaggerGen();

    builder.Services.AddScoped<ICreateRoutineHandler, CreateRoutineHandler>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    logger.Info("Starting web application...");
    app.Run();
}
catch (Exception ex)
{
    logger.Error("Application terminated unexpectedly", ex);
}
finally
{
    log4net.LogManager.Shutdown();
}
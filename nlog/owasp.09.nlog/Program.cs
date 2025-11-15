using NLog;
using NLog.Web;
using owasp._09.nlog.core.UseCases.CreateRoutine.Contracts;
using owasp._09.nlog.core.UseCases.CreateRoutine;

var logger = NLog.LogManager.Setup()
                           .LoadConfigurationFromAppSettings()
                           .GetCurrentClassLogger();

logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();

    builder.Host.UseNLog();

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
    logger.Error(ex, "Application terminated unexpectedly");
}
finally
{
    NLog.LogManager.Shutdown();
}
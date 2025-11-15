using Microsoft.Extensions.Logging;
using owasp._09.nlog.core.UseCases.CreateRoutine.Contracts;

namespace owasp._09.nlog.core.UseCases.CreateRoutine;

public sealed class CreateRoutineHandler : ICreateRoutineHandler
{
    private readonly ILogger<CreateRoutineHandler> _logger;

    public CreateRoutineHandler(ILogger<CreateRoutineHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CreateRoutineInput request)
    {
        // Hypothetical creation of routine here

        // Log statements
        _logger.LogInformation("Let's put this log in risk");
    }
}

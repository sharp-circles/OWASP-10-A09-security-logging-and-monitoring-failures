using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using owasp._09.serilog.core.UseCases.CreateRoutine.Contracts;

namespace owasp._09.serilog.core.UseCases.CreateRoutine;

public sealed class CreateRoutineHandler : ICreateRoutineHandler
{
    private readonly ILogger<CreateRoutineHandler> _logger;

    public CreateRoutineHandler(ILogger<CreateRoutineHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CreateRoutineInput request)
    {
        foreach (var exercise in request.Exercises)
        {
            _logger.LogInformation("Description: {Description}", exercise.Description);
        }
    }
}

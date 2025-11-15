using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using owasp._09.log4net.core.UseCases.CreateRoutine.Contracts;

namespace owasp._09.log4net.api.Endpoints.CreateRoutine;

[ApiController]
[Route("[controller]")]
public class RoutinesController : ControllerBase
{
    private readonly ICreateRoutineHandler _handler;

    public RoutinesController(ICreateRoutineHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRoutine(int id, [FromBody] CreateRoutineRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CreateRoutineInput()
        {
            Name = request.Name,
            Type = request.Type,
            Exercises = request.Exercises.Select(x =>
                new core.UseCases.CreateRoutine.Contracts.ExerciseDetails()
                {
                    Name = x.Name,
                    Region = x.Region,
                    Repetitions = x.Repetitions,
                    Description = x.Description
                }
            ).ToList()
        };

        await _handler.Handle(input);

        return new OkObjectResult(id);
    }
}

using Microsoft.AspNetCore.Mvc;

namespace owasp._09.serilog.api.Endpoints.CreateRoutine;

public record CreateRoutineRequest
{
    [FromBody]
    public string Name { get; set; }
    [FromBody]
    public string Type { get; set; }
    [FromBody]
    public List<ExerciseDetails> Exercises  { get; set; }
}

public record ExerciseDetails
{
    [FromBody]
    public string Name { get; set; }
    [FromBody]
    public string Region { get; set; }
    [FromBody]
    public int Repetitions { get; set; }
    [FromBody]
    public string Description { get; set; }
}
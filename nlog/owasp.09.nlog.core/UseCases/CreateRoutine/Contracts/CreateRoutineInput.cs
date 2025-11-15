namespace owasp._09.nlog.core.UseCases.CreateRoutine.Contracts;

public record CreateRoutineInput
{
    public string Name { get; set; }
    public string Type { get; set; }
    public List<ExerciseDetails> Exercises { get; set; }
}

public record ExerciseDetails
{
    public string Name { get; set; }
    public string Region { get; set; }
    public int Repetitions { get; set; }
    public string Description { get; set; }
}

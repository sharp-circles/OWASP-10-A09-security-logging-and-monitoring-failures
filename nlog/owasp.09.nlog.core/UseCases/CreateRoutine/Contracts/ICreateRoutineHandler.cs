namespace owasp._09.nlog.core.UseCases.CreateRoutine.Contracts;

public interface ICreateRoutineHandler
{
    Task Handle(CreateRoutineInput request);
}

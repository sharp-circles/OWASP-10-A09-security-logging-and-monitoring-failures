namespace owasp._09.log4net.core.UseCases.CreateRoutine.Contracts;

public interface ICreateRoutineHandler
{
    Task Handle(CreateRoutineInput request);
}

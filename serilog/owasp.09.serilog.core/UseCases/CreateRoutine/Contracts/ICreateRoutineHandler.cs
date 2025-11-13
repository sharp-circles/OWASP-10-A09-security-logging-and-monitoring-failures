using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace owasp._09.serilog.core.UseCases.CreateRoutine.Contracts;

public interface ICreateRoutineHandler
{
    Task Handle(CreateRoutineInput request);
}

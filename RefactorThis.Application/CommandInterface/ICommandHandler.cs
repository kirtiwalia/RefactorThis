using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Application.CommandInterface
{
    public interface ICommandHandler<TCommand, TResult>
    {
        TResult Handle(TCommand command);
    }
}

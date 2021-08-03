using EventsApi.Commands;
using System.Threading.Tasks;

namespace EventsApi.CommandHandlers
{
    public interface ISmsSendCommandHandler
    {
        Task<bool> Handle(SmsSendCommand command);
    }
}
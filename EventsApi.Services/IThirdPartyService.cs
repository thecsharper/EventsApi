using System.Threading.Tasks;

using EventsApi.Commands;

namespace EventsApi.Services
{
    public interface IThirdPartyService
    {
        public Task<ThirdPartyResponse> SendMessage(SmsSendCommand command);
    }
}

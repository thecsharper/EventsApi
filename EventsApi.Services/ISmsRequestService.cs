using System;

namespace EventsApi.Services
{
    public interface ISmsRequestService
    {
        void AddSmsId(Guid smsId);

        bool GetSmsId(Guid smsId);

        void RemoveSmsId(Guid smsId);
    }
}
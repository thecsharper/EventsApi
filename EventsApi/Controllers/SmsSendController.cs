using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Polly;

using EventsApi.CommandHandlers;
using EventsApi.Commands;
using EventsApi.Events;
using EventsApi.Messages;
using EventsApi.Models;

namespace EventsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SmsSendController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly ISmsSendCommandHandler _smsSendCommandHandler;

        public SmsSendController(ILogger<SmsSendController> logger,
                                 IEventBus eventBus,
                                 ISmsSendCommandHandler smsSendCommandHandler)
        {
            _logger = logger;
            _eventBus = eventBus;
            _smsSendCommandHandler = smsSendCommandHandler;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SmsResponse), 201)]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(void), 400)]
        public async Task<IActionResult> SmsSend(SmsMessage smsMessage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(smsMessage);
            }

            var smssSendCommand = new SmsSendCommand(smsMessage.PhoneNumber, smsMessage.Message, smsMessage.MessageId);
            var smsSentResult = await _smsSendCommandHandler.Handle(smssSendCommand);

            if (!smsSentResult)
            {
                _logger.LogWarning("Message '{MessageId}' not sent to third party service.", smsMessage.MessageId);

                return NoContent();
            }

            var smsSentEvent = new SmsSentEvent(smssSendCommand.PhoneNumber, smssSendCommand.MessageText, smssSendCommand.MessageId);

            QueueMessage(smsSentEvent, smsMessage.MessageId);

            return new ObjectResult(new SmsResponse(smsSentEvent.Id)) { StatusCode = StatusCodes.Status201Created };
        }

        private void QueueMessage(SmsSentEvent smsSentEvent, Guid messageId)
        {
            _ = Policy.Handle<AggregateException>()
                                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                (exception, timeSpan, context) =>
                                {
                                    _logger.LogWarning("Message '{MessageId}' not published: {Exception}", messageId, exception);
                                }).ExecuteAsync(() => _eventBus.Publish(smsSentEvent));

            _logger.LogInformation("Message '{MessageId}' published", messageId);
        }
    }
}

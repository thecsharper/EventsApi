using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Xunit;
using Moq;
using FluentAssertions;

using EventsApi.Events;
using EventsApi.Commands;
using EventsApi.Controllers;
using EventsApi.Services;
using EventsApi.Models;
using EventsApi.CommandHandlers;

namespace EventsApi.UnitTests
{
    public class SmsSendControllerTests
    {
        private readonly Mock<ILogger<SmsSendController>> _logger;
        private readonly Mock<IEventBus> _eventBus;
        private readonly Mock<IThirdPartyService> _thirdPartyService;
        private readonly Mock<ISmsRequestService> _smsRequestService;
        private readonly Mock<ISmsSendCommandHandler> _smsSendCommandHandler;

        public SmsSendControllerTests()
        {
            _logger = new Mock<ILogger<SmsSendController>>();
            _eventBus = new Mock<IEventBus>();
            _thirdPartyService = new Mock<IThirdPartyService>();
            _smsRequestService = new Mock<ISmsRequestService>();
            _smsSendCommandHandler = new Mock<ISmsSendCommandHandler>();
        }

        [Fact]
        public async Task SmsSend_returns_http_201_response()
        {
            var smsId = Guid.NewGuid();
            var messsage = new SmsMessage(smsId, string.Empty, string.Empty);

            _thirdPartyService.Setup(x => x.SendMessage(It.IsAny<SmsSendCommand>()))
                              .ReturnsAsync(new ThirdPartyResponse(System.Net.HttpStatusCode.OK));
            _smsSendCommandHandler.Setup(x => x.Handle(It.IsAny<SmsSendCommand>())).ReturnsAsync(true);

            var controller = new SmsSendController(_logger.Object, _eventBus.Object, _smsSendCommandHandler.Object);

            var result = await controller.SmsSend(messsage) as ObjectResult;

            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task SmsSend_returns_http_204_response()
        {
            var smsId = Guid.NewGuid();
            var messsage = new SmsMessage(smsId, string.Empty, string.Empty);

            _smsRequestService.Setup(x => x.GetSmsId(It.IsAny<Guid>())).Returns(true);
            _thirdPartyService.Setup(x => x.SendMessage(It.IsAny<SmsSendCommand>()))
                              .ReturnsAsync(new ThirdPartyResponse(System.Net.HttpStatusCode.OK));

            var controller = new SmsSendController(_logger.Object, _eventBus.Object, _smsSendCommandHandler.Object);

            var result = await controller.SmsSend(messsage) as NoContentResult;

            result.StatusCode.Should().Be(204);
        }
    }
}

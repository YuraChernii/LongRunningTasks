using LongRunningTasks.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using ProducerApi.Services;

namespace ProducerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController : ControllerBase
    {

        private readonly ILogger<ItemController> _logger;
        private readonly IMessageProducer _messageProducer;


        public ItemController(
            ILogger<ItemController> logger,
            IMessageProducer messageProducer)
        {
            _logger = logger;
            _messageProducer = messageProducer;
        }

        [HttpPost]
        public IActionResult CreatingItem(string message)
        {
            _messageProducer.SendingMessage(message);

            _logger.LogInformation("Item was sent with correlation id: ...");

            return Ok();
        }
    }
}
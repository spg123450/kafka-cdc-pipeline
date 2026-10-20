using CDCTarget.Data;
using CDCTarget.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using CDCTarget.Hubs;

namespace CDCTarget.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CDCTargetApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<DashboardHub> _hubContext;

        public CDCTargetApiController(AppDbContext db, IHubContext<DashboardHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        [HttpGet("events")]
        public async Task<ActionResult<ResponseDto>> GetRecentEvents()
        {
            try
            {
                // Fetch the 50 most recent CDC operations
                var recentEvents = await _db.TopicEvents
                    .OrderByDescending(e => e.Timestamp)
                    .Take(50)
                    .Select(e => new TopicDto
                    {
                        DocumentId = e.DocumentId,
                        OperationType = e.OperationType,
                        PayloadData = e.PayloadData
                    })
                    .ToListAsync();

                return Ok(new ResponseDto
                {
                    IsSuccess = true,
                    Result = recentEvents,
                    Message = "Successfully retrieved recent CDC events."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpGet("logs")]
        public async Task<ActionResult<ResponseDto>> GetSystemLogs()
        {
            try
            {
                // Fetch recent connection and system logs
                var logs = await _db.SubscriptionEvents
                    .OrderByDescending(e => e.Timestamp)
                    .Take(50)
                    .Select(e => new TopicSubscriptionDto
                    {
                        TopicName = e.Source,
                        Status = e.Message
                    })
                    .ToListAsync();

                return Ok(new ResponseDto
                {
                    IsSuccess = true,
                    Result = logs,
                    Message = "Successfully retrieved system logs."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToTopic([FromBody] SubscriptionRequest request)
        {
            try
            {
                // 1. Create a formal subscription log
                var subscriptionLog = new Models.SubscriptionEvent
                {
                    Source = request.TopicName,
                    Message = $"Subscription activated for external client: {request.ClientName}",
                    Timestamp = DateTime.UtcNow
                };

                // 2. Save it to the SQLite Database
                _db.SubscriptionEvents.Add(subscriptionLog);
                await _db.SaveChangesAsync();

                // 3. Return the success confirmation to the Downstream Consumer
                return Ok(new Models.Dto.ResponseDto
                {
                    IsSuccess = true,
                    Message = $"Successfully subscribed '{request.ClientName}' to topic '{request.TopicName}'. Real-time event consumption authorized."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Models.Dto.ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveFromKafka([FromBody] Topics.CdcEvent payload, [FromServices] MessageBus.IMessageBus messageBus)
        {
            // 1. Receive data from the ChangeStreamManager window
            // 2. Publish it to the internal bus using the Async method
            await messageBus.PublishAsync("cdc_events_topic", payload);

            // 3. Broadcast the live data to the Migration Dashboard!
            await _hubContext.Clients.All.SendAsync("ReceivePipelineEvent", payload);

            return Ok();
        }
    }

    public class SubscriptionRequest
    {
        public string ClientName { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
    }
}
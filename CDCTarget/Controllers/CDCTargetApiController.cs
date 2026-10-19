using CDCTarget.Data;
using CDCTarget.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CDCTarget.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CDCTargetApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CDCTargetApiController(AppDbContext db)
        {
            _db = db;
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
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using plumsail_testtask.Server.Data;
using plumsail_testtask.Server.Models;
using System.Text.Json;

namespace plumsail_testtask.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubmissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("{formType}")]
        public async Task<IActionResult> CreateSubmission(
            string formType,
            [FromBody] JsonElement formData)
        {
            if (string.IsNullOrWhiteSpace(formType))
            {
                return BadRequest(new { error = "type is required" });
            }
            if (formData.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return BadRequest(new { error = "data is required" });
            }

            try
            {
                var jsonString = formData.GetRawText();

                if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "{}")
                {
                    return BadRequest(new { error = "data cannot be empty" });
                }

                var submission = new FormSubmission
                {
                    Id = Guid.NewGuid(),
                    FormType = formType,
                    SubmittedAt = DateTime.UtcNow,
                    DataJson = jsonString
                };

                _context.FormSubmissions.Add(submission);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    id = submission.Id,
                    message = "submitted successfully",
                    submittedAt = submission.SubmittedAt
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubmissions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var totalCount = await _context.FormSubmissions.CountAsync();

                var submissions = await _context.FormSubmissions
                    .OrderByDescending(s => s.SubmittedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var mappedData = submissions.Select(MapToDto).ToList();

                return Ok(new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    data = mappedData
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Failed to retrieve" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchSubmissions([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllSubmissions();
            }

            try
            {
                var submissions = await _context.FormSubmissions
                    .Where(s => s.DataJson.Contains(query))
                    .OrderByDescending(s => s.SubmittedAt)
                    .ToListAsync();

                var result = submissions.Select(MapToDto).ToList();

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Failed to search" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubmissionById(Guid id)
        {
            try
            {
                var submission = await _context.FormSubmissions.FindAsync(id);

                if (submission == null)
                {
                    return NotFound(new { error = "Submission not found" });
                }

                return Ok(MapToDto(submission));
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Failed to retrieve" });
            }
        }

        private object MapToDto(FormSubmission submission)
        {
            return new
            {
                id = submission.Id,
                formType = submission.FormType,
                submittedAt = submission.SubmittedAt,
                data = JsonSerializer.Deserialize<object>(submission.DataJson)
            };
        }
    }
}
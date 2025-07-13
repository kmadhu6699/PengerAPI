using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PengerAPI.Data;
using PengerAPI.DTOs;

namespace PengerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        [HttpGet]
        public ActionResult<HealthCheckResponse> GetHealth()
        {
            try
            {
                var response = new HealthCheckResponse
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                };

                return Ok(ApiResponse<HealthCheckResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");

                var response = new HealthCheckResponse
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    Error = ex.Message
                };

                return StatusCode(503, ApiResponse<HealthCheckResponse>.ErrorResult("Service unhealthy", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Detailed health check including database connectivity
        /// </summary>
        [HttpGet("detailed")]
        public async Task<ActionResult<HealthCheckResponse>> GetDetailedHealth()
        {
            var response = new HealthCheckResponse
            {
                Status = "Checking",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };

            try
            {
                // Test database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    response.Status = "Unhealthy";
                    response.Error = "Cannot connect to database";
                    return StatusCode(503, ApiResponse<HealthCheckResponse>.ErrorResult("Database connection failed"));
                }

                // Get basic statistics
                var userCount = await _context.Users.CountAsync();
                var accountCount = await _context.Accounts.CountAsync();
                var currencyCount = await _context.Currencies.CountAsync();
                var accountTypeCount = await _context.AccountTypes.CountAsync();

                response.Status = "Healthy";
                response.Details = new Dictionary<string, object>
                {
                    { "DatabaseConnected", true },
                    { "UserCount", userCount },
                    { "AccountCount", accountCount },
                    { "CurrencyCount", currencyCount },
                    { "AccountTypeCount", accountTypeCount },
                    { "ServerTime", DateTime.UtcNow },
                    { "Uptime", Environment.TickCount64 }
                };

                return Ok(ApiResponse<HealthCheckResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detailed health check failed");

                response.Status = "Unhealthy";
                response.Error = ex.Message;
                response.Details = new Dictionary<string, object>
                {
                    { "DatabaseConnected", false },
                    { "Error", ex.Message }
                };

                return StatusCode(503, ApiResponse<HealthCheckResponse>.ErrorResult("Service unhealthy", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Readiness probe for Kubernetes/Docker deployments
        /// </summary>
        [HttpGet("ready")]
        public async Task<ActionResult> GetReadiness()
        {
            try
            {
                // Check if database is accessible
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(503, ApiResponse<HealthCheckResponse>.ErrorResult("Database not ready"));
                }

                var response = new HealthCheckResponse
                {
                    Status = "Ready",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                };

                return Ok(ApiResponse<HealthCheckResponse>.SuccessResult(response, "Service is ready"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                var errorResponse = new HealthCheckResponse
                {
                    Status = "Not Ready",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    Error = ex.Message
                };
                return StatusCode(503, ApiResponse<HealthCheckResponse>.ErrorResult("Service not ready"));
            }
        }

        /// <summary>
        /// Liveness probe for Kubernetes/Docker deployments
        /// </summary>
        [HttpGet("live")]
        public ActionResult GetLiveness()
        {
            // Simple liveness check - if this endpoint responds, the service is alive
            var response = new HealthCheckResponse
            {
                Status = "Alive",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };

            return Ok(ApiResponse<HealthCheckResponse>.SuccessResult(response, "Service is alive"));
        }
    }
}

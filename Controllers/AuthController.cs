using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PengerAPI.DTOs;
using PengerAPI.Services;

namespace PengerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.ValidationError(validationResult.Errors));
            }

            var result = await _authService.RegisterAsync(registerDto);
            return StatusCode(
                result.Success ? StatusCodes.Status201Created : StatusCodes.Status400BadRequest,
                result
            );
        }

        /// <summary>
        /// Authenticate user and get JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.ValidationError(validationResult.Errors));
            }

            var result = await _authService.LoginAsync(loginDto);
            return result.Success ? Ok(result) : Unauthorized(result);
        }
    }
}

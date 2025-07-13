using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PengerAPI.Data.Repositories;
using PengerAPI.DTOs;
using PengerAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace PengerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<UserSummaryDto>>> GetUsers([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var users = await _unitOfWork.Users.GetPagedAsync(
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                var userDtos = _mapper.Map<List<UserSummaryDto>>(users.Items);
                
                var response = new PagedResponse<UserSummaryDto>(
                    userDtos,
                    users.TotalCount,
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                return Ok(ApiResponse<PagedResponse<UserSummaryDto>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve users"));
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdWithAccountsAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                }

                var userDto = _mapper.Map<UserDto>(user);
                return Ok(ApiResponse<UserDto>.SuccessResult(userDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve user"));
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("User with this email already exists"));
                }

                // Check if username is taken
                var existingUsername = await _unitOfWork.Users.GetByUsernameAsync(createUserDto.Username);
                if (existingUsername != null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Username is already taken"));
                }

                var user = _mapper.Map<User>(createUserDto);
                user.Password = HashPassword(createUserDto.Password);

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, 
                    ApiResponse<UserDto>.SuccessResult(userDto, "User created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to create user"));
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                }

                // Check if email is being changed and if it's already taken
                if (updateUserDto.Email != user.Email)
                {
                    var existingUser = await _unitOfWork.Users.GetByEmailAsync(updateUserDto.Email);
                    if (existingUser != null)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Email is already in use"));
                    }
                }

                _mapper.Map(updateUserDto, user);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "User updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to update user"));
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                }

                // Check if user has accounts
                var hasAccounts = await _unitOfWork.Accounts.ExistsAsync(a => a.UserId == id);
                if (hasAccounts)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Cannot delete user with existing accounts"));
                }

                await _unitOfWork.Users.DeleteAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(null, "User deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to delete user"));
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("{id}/change-password")]
        public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("User not found"));
                }

                // Verify current password
                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.Password))
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Current password is incorrect"));
                }

                user.Password = HashPassword(changePasswordDto.NewPassword);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(null, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to change password"));
            }
        }

        /// <summary>
        /// Search users by email or username
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<PagedResponse<UserSummaryDto>>> SearchUsers([FromQuery] SearchParams searchParams)
        {
            try
            {
                var users = await _unitOfWork.Users.SearchAsync(
                    searchParams.Query,
                    searchParams.PageNumber,
                    searchParams.PageSize
                );

                var userDtos = _mapper.Map<List<UserSummaryDto>>(users.Items);
                
                var response = new PagedResponse<UserSummaryDto>(
                    userDtos,
                    users.TotalCount,
                    searchParams.PageNumber,
                    searchParams.PageSize
                );

                return Ok(ApiResponse<PagedResponse<UserSummaryDto>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with query: {Query}", searchParams.Query);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to search users"));
            }
        }

        #region Private Methods

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        #endregion
    }
}

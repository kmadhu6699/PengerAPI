using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PengerAPI.Data.Repositories;
using PengerAPI.DTOs;
using PengerAPI.Models;

namespace PengerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OTPController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OTPController> _logger;

        public OTPController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OTPController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Generate OTP for a user
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<OTPGeneratedDto>> GenerateOTP([FromBody] GenerateOTPDto generateOTPDto)
        {
            try
            {
                // Verify user exists
                var user = await _unitOfWork.Users.GetByIdAsync(generateOTPDto.UserId);
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("User not found"));
                }

                // Check if there's an active OTP for this user and purpose
                var existingOTP = await _unitOfWork.OTPs.GetActiveOTPAsync(generateOTPDto.UserId, generateOTPDto.Purpose);
                if (existingOTP != null)
                {
                    // Invalidate existing OTP
                    existingOTP.IsUsed = true;
                    await _unitOfWork.OTPs.UpdateAsync(existingOTP);
                }

                // Generate new OTP
                var otp = new OTP
                {
                    UserId = generateOTPDto.UserId,
                    Purpose = generateOTPDto.Purpose,
                    Code = GenerateOTPCode(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5 minutes expiry
                    IsUsed = false
                };

                await _unitOfWork.OTPs.AddAsync(otp);
                await _unitOfWork.SaveChangesAsync();

                var response = new OTPGeneratedDto
                {
                    OTPId = otp.Id,
                    Code = otp.Code,
                    ExpiresAt = otp.ExpiresAt,
                    Purpose = otp.Purpose
                };

                _logger.LogInformation("OTP generated for user {UserId} with purpose {Purpose}", generateOTPDto.UserId, generateOTPDto.Purpose);

                return Ok(ApiResponse<OTPGeneratedDto>.SuccessResult(response, "OTP generated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for user {UserId}", generateOTPDto.UserId);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to generate OTP"));
            }
        }

        /// <summary>
        /// Verify OTP
        /// </summary>
        [HttpPost("verify")]
        public async Task<ActionResult<OTPVerificationDto>> VerifyOTP([FromBody] VerifyOTPDto verifyOTPDto)
        {
            try
            {
                var otp = await _unitOfWork.OTPs.GetByCodeAsync(verifyOTPDto.Code);
                if (otp == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid OTP code"));
                }

                // Check if OTP belongs to the user
                if (otp.UserId != verifyOTPDto.UserId)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid OTP for this user"));
                }

                // Check if OTP matches the purpose
                if (otp.Purpose != verifyOTPDto.Purpose)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("OTP purpose mismatch"));
                }

                // Check if OTP is already used
                if (otp.IsUsed)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("OTP has already been used"));
                }

                // Check if OTP is expired
                if (otp.ExpiresAt < DateTime.UtcNow)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("OTP has expired"));
                }

                // Mark OTP as used
                otp.IsUsed = true;
                await _unitOfWork.OTPs.UpdateAsync(otp);
                await _unitOfWork.SaveChangesAsync();

                var response = new OTPVerificationDto
                {
                    IsValid = true,
                    Purpose = otp.Purpose,
                    VerifiedAt = DateTime.UtcNow
                };

                _logger.LogInformation("OTP verified successfully for user {UserId} with purpose {Purpose}", verifyOTPDto.UserId, verifyOTPDto.Purpose);

                return Ok(ApiResponse<OTPVerificationDto>.SuccessResult(response, "OTP verified successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to verify OTP"));
            }
        }

        /// <summary>
        /// Resend OTP
        /// </summary>
        [HttpPost("resend")]
        public async Task<ActionResult<OTPGeneratedDto>> ResendOTP([FromBody] ResendOTPDto resendOTPDto)
        {
            try
            {
                // Verify user exists
                var user = await _unitOfWork.Users.GetByIdAsync(resendOTPDto.UserId);
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("User not found"));
                }

                // Check rate limiting - prevent too frequent OTP requests
                var recentOTP = await _unitOfWork.OTPs.GetRecentOTPAsync(resendOTPDto.UserId, resendOTPDto.Purpose, TimeSpan.FromMinutes(1));
                if (recentOTP != null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Please wait before requesting a new OTP"));
                }

                // Invalidate any existing active OTP
                var existingOTP = await _unitOfWork.OTPs.GetActiveOTPAsync(resendOTPDto.UserId, resendOTPDto.Purpose);
                if (existingOTP != null)
                {
                    existingOTP.IsUsed = true;
                    await _unitOfWork.OTPs.UpdateAsync(existingOTP);
                }

                // Generate new OTP
                var otp = new OTP
                {
                    UserId = resendOTPDto.UserId,
                    Purpose = resendOTPDto.Purpose,
                    Code = GenerateOTPCode(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5 minutes expiry
                    IsUsed = false
                };

                await _unitOfWork.OTPs.AddAsync(otp);
                await _unitOfWork.SaveChangesAsync();

                var response = new OTPGeneratedDto
                {
                    OTPId = otp.Id,
                    Code = otp.Code,
                    ExpiresAt = otp.ExpiresAt,
                    Purpose = otp.Purpose
                };

                _logger.LogInformation("OTP resent for user {UserId} with purpose {Purpose}", resendOTPDto.UserId, resendOTPDto.Purpose);

                return Ok(ApiResponse<OTPGeneratedDto>.SuccessResult(response, "OTP resent successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP for user {UserId}", resendOTPDto.UserId);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to resend OTP"));
            }
        }

        /// <summary>
        /// Get OTP history for a user (admin endpoint)
        /// </summary>
        [HttpGet("user/{userId}/history")]
        public async Task<ActionResult<PagedResponse<OTPSummaryDto>>> GetUserOTPHistory(int userId, [FromQuery] PaginationParams paginationParams)
        {
            try
            {
                // Verify user exists
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("User not found"));
                }

                var otps = await _unitOfWork.OTPs.GetByUserIdPagedAsync(userId, paginationParams.PageNumber, paginationParams.PageSize);
                var otpDtos = _mapper.Map<List<OTPSummaryDto>>(otps.Items);

                var response = new PagedResponse<OTPSummaryDto>(
                    otpDtos,
                    otps.TotalCount,
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                return Ok(ApiResponse<PagedResponse<OTPSummaryDto>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OTP history for user {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve OTP history"));
            }
        }

        /// <summary>
        /// Clean up expired OTPs (admin endpoint)
        /// </summary>
        [HttpDelete("cleanup-expired")]
        public async Task<ActionResult> CleanupExpiredOTPs()
        {
            try
            {
                var deletedCount = await _unitOfWork.OTPs.DeleteExpiredOTPsAsync();
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired OTPs", deletedCount);

                var response = new OTPCleanupResponse
                {
                    DeletedCount = deletedCount,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(ApiResponse<OTPCleanupResponse>.SuccessResult(response, $"Cleaned up {deletedCount} expired OTPs"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired OTPs");
                return StatusCode(500, ApiResponse<OTPCleanupResponse>.ErrorResult("Failed to cleanup expired OTPs"));
            }
        }

        #region Private Methods

        private static string GenerateOTPCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-digit OTP
        }

        #endregion
    }
}

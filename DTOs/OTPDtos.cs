using System;
using FluentValidation;

namespace PengerAPI.DTOs
{
    // Response DTOs
    public class OTPDto : BaseDto
    {
        public int UserId { get; set; }
        public string Code { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;
        public UserSummaryDto User { get; set; }
    }

    public class OTPSummaryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;
        public DateTime CreatedAt { get; set; }
    }

    // Request DTOs
    public class GenerateOTPDto
    {
        public int UserId { get; set; }
        public int ExpiryMinutes { get; set; } = 5; // Default 5 minutes

        public string Purpose { get; set; } = "Verification"; // Default purpose
    }

    public class VerifyOTPDto
    {
        public int UserId { get; set; }
        public string Code { get; set; }

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5); // Default expiry time of 5 minutes

        public string  Purpose { get; set; }
    }

    public class ResendOTPDto
    {
        public int UserId { get; set; }
        public string Purpose { get; set; } = "Verification"; // Default purpose
    }

    // Response DTOs for OTP operations
    public class OTPGeneratedDto
    {
        public int OTPId { get; set; }

        public string Purpose { get; set; } = "Verification"; // Default purpose
        public string Code { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ExpiryMinutes { get; set; }
        public string Message { get; set; } = "OTP generated successfully";
    }

    public class OTPVerificationDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public DateTime? VerifiedAt { get; set; }
        
        public string Purpose { get; set; } = "Verification"; // Default purpose
    }

    // Validators
    public class GenerateOTPDtoValidator : AbstractValidator<GenerateOTPDto>
    {
        public GenerateOTPDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");

            RuleFor(x => x.ExpiryMinutes)
                .InclusiveBetween(1, 60).WithMessage("Expiry minutes must be between 1 and 60");
        }
    }

    public class VerifyOTPDtoValidator : AbstractValidator<VerifyOTPDto>
    {
        public VerifyOTPDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("OTP code is required")
                .Length(4, 8).WithMessage("OTP code must be between 4 and 8 characters")
                .Matches("^[0-9]+$").WithMessage("OTP code must contain only numbers");
        }
    }

    public class ResendOTPDtoValidator : AbstractValidator<ResendOTPDto>
    {
        public ResendOTPDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");
        }
    }
}

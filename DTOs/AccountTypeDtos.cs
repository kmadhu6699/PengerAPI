using FluentValidation;

namespace PengerAPI.DTOs
{
    // Response DTOs
    public class AccountTypeDto : BaseDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class AccountTypeSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    // Request DTOs
    public class CreateAccountTypeDto : BaseCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAccountTypeDto : BaseUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    // Validators
    public class CreateAccountTypeDtoValidator : AbstractValidator<CreateAccountTypeDto>
    {
        public CreateAccountTypeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Account type name is required")
                .Length(2, 50).WithMessage("Account type name must be between 2 and 50 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }

    public class UpdateAccountTypeDtoValidator : AbstractValidator<UpdateAccountTypeDto>
    {
        public UpdateAccountTypeDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid account type ID");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Account type name is required")
                .Length(2, 50).WithMessage("Account type name must be between 2 and 50 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}

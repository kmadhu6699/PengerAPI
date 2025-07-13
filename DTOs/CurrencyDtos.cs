using FluentValidation;

namespace PengerAPI.DTOs
{
    // Response DTOs
    public class CurrencyDto : BaseDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool IsActive { get; set; }
    }

    public class CurrencySummaryDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool IsActive { get; set; }
    }

    // Request DTOs
    public class CreateCurrencyDto : BaseCreateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCurrencyDto : BaseUpdateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool IsActive { get; set; }
    }

    // Validators
    public class CreateCurrencyDtoValidator : AbstractValidator<CreateCurrencyDto>
    {
        public CreateCurrencyDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Currency code is required")
                .Length(3, 3).WithMessage("Currency code must be exactly 3 characters")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be 3 uppercase letters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Currency name is required")
                .Length(2, 100).WithMessage("Currency name must be between 2 and 100 characters");

            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("Currency symbol is required")
                .Length(1, 5).WithMessage("Currency symbol must be between 1 and 5 characters");
        }
    }

    public class UpdateCurrencyDtoValidator : AbstractValidator<UpdateCurrencyDto>
    {
        public UpdateCurrencyDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid currency ID");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Currency code is required")
                .Length(3, 3).WithMessage("Currency code must be exactly 3 characters")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be 3 uppercase letters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Currency name is required")
                .Length(2, 100).WithMessage("Currency name must be between 2 and 100 characters");

            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("Currency symbol is required")
                .Length(1, 5).WithMessage("Currency symbol must be between 1 and 5 characters");
        }
    }
}

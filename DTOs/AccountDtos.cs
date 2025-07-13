using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace PengerAPI.DTOs
{
    // Response DTOs
    public class AccountDto : BaseDto
    {
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public int AccountTypeId { get; set; }
        
        // Navigation properties
        public UserSummaryDto User { get; set; }
        public CurrencyDto Currency { get; set; }
        public AccountTypeDto AccountType { get; set; }
    }

    public class AccountSummaryDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string AccountTypeName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AccountBalanceDto
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string FormattedBalance => $"{CurrencySymbol}{Balance:N2}";
    }

    // Request DTOs
    public class CreateAccountDto : BaseCreateDto
    {
        public int UserId { get; set; }
        public int CurrencyId { get; set; }
        public int AccountTypeId { get; set; }
        public decimal InitialBalance { get; set; } = 0;
    }

    public class UpdateAccountDto : BaseUpdateDto
    {
        public int CurrencyId { get; set; }
        public int AccountTypeId { get; set; }
    }

    public class TransferDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    public class DepositWithdrawDto
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    // Validators
    public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
    {
        public CreateAccountDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Valid user ID is required");

            RuleFor(x => x.CurrencyId)
                .GreaterThan(0).WithMessage("Valid currency ID is required");

            RuleFor(x => x.AccountTypeId)
                .GreaterThan(0).WithMessage("Valid account type ID is required");

            RuleFor(x => x.InitialBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Initial balance cannot be negative");
        }
    }

    public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
    {
        public UpdateAccountDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid account ID");

            RuleFor(x => x.CurrencyId)
                .GreaterThan(0).WithMessage("Valid currency ID is required");

            RuleFor(x => x.AccountTypeId)
                .GreaterThan(0).WithMessage("Valid account type ID is required");
        }
    }

    public class TransferDtoValidator : AbstractValidator<TransferDto>
    {
        public TransferDtoValidator()
        {
            RuleFor(x => x.FromAccountId)
                .GreaterThan(0).WithMessage("Valid source account ID is required");

            RuleFor(x => x.ToAccountId)
                .GreaterThan(0).WithMessage("Valid destination account ID is required")
                .NotEqual(x => x.FromAccountId).WithMessage("Source and destination accounts cannot be the same");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Transfer amount must be greater than zero");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }

    public class DepositWithdrawDtoValidator : AbstractValidator<DepositWithdrawDto>
    {
        public DepositWithdrawDtoValidator()
        {
            RuleFor(x => x.AccountId)
                .GreaterThan(0).WithMessage("Valid account ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}

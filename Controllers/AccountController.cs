using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PengerAPI.Data.Repositories;
using PengerAPI.DTOs;
using PengerAPI.Models;

namespace PengerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AccountController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all accounts with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<AccountSummaryDto>>> GetAccounts([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var accounts = await _unitOfWork.Accounts.GetPagedAsync(
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                var accountDtos = _mapper.Map<List<AccountSummaryDto>>(accounts.Items);

                var response = new PagedResponse<AccountSummaryDto>(
                    accountDtos,
                    accounts.TotalCount,
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                return Ok(ApiResponse<PagedResponse<AccountSummaryDto>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve accounts"));
            }
        }

        /// <summary>
        /// Get account by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDto>> GetAccount(int id)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdWithDetailsAsync(id);
                if (account == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account not found"));
                }

                var accountDto = _mapper.Map<AccountDto>(account);
                return Ok(ApiResponse<AccountDto>.SuccessResult(accountDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account {AccountId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve account"));
            }
        }

        /// <summary>
        /// Get accounts by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AccountSummaryDto>>> GetAccountsByUser(int userId)
        {
            try
            {
                var accounts = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
                var accountDtos = _mapper.Map<List<AccountSummaryDto>>(accounts);

                return Ok(ApiResponse<IEnumerable<AccountSummaryDto>>.SuccessResult(accountDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts for user {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve user accounts"));
            }
        }

        /// <summary>
        /// Create a new account
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto createAccountDto)
        {
            try
            {
                // Verify user exists
                var user = await _unitOfWork.Users.GetByIdAsync(createAccountDto.UserId);
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("User not found"));
                }

                // Verify currency exists and is active
                var currency = await _unitOfWork.Currencies.GetByIdAsync(createAccountDto.CurrencyId);
                if (currency == null || !currency.IsActive)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid or inactive currency"));
                }

                // Verify account type exists and is active
                var accountType = await _unitOfWork.AccountTypes.GetByIdAsync(createAccountDto.AccountTypeId);
                if (accountType == null || !accountType.IsActive)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Invalid or inactive account type"));
                }

                var account = _mapper.Map<Account>(createAccountDto);
                account.AccountNumber = await GenerateAccountNumber();

                await _unitOfWork.Accounts.AddAsync(account);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                account = await _unitOfWork.Accounts.GetByIdWithDetailsAsync(account.Id);
                var accountDto = _mapper.Map<AccountDto>(account);

                return CreatedAtAction(nameof(GetAccount), new { id = account.Id },
                    ApiResponse<AccountDto>.SuccessResult(accountDto, "Account created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to create account"));
            }
        }

        /// <summary>
        /// Update account information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AccountDto>> UpdateAccount(int id, [FromBody] UpdateAccountDto updateAccountDto)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account not found"));
                }

                // Verify currency exists and is active if being changed
                if (updateAccountDto.CurrencyId != account.CurrencyId)
                {
                    var currency = await _unitOfWork.Currencies.GetByIdAsync(updateAccountDto.CurrencyId);
                    if (currency == null || !currency.IsActive)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Invalid or inactive currency"));
                    }
                }

                // Verify account type exists and is active if being changed
                if (updateAccountDto.AccountTypeId != account.AccountTypeId)
                {
                    var accountType = await _unitOfWork.AccountTypes.GetByIdAsync(updateAccountDto.AccountTypeId);
                    if (accountType == null || !accountType.IsActive)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Invalid or inactive account type"));
                    }
                }

                _mapper.Map(updateAccountDto, account);
                await _unitOfWork.Accounts.UpdateAsync(account);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                account = await _unitOfWork.Accounts.GetByIdWithDetailsAsync(account.Id);
                var accountDto = _mapper.Map<AccountDto>(account);

                return Ok(ApiResponse<AccountDto>.SuccessResult(accountDto, "Account updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account {AccountId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to update account"));
            }
        }

        /// <summary>
        /// Delete account
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAccount(int id)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account not found"));
                }

                // Check if account has balance
                if (account.Balance != 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Cannot delete account with non-zero balance"));
                }

                await _unitOfWork.Accounts.DeleteAsync(account);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(null, "Account deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account {AccountId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to delete account"));
            }
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        [HttpGet("{id}/balance")]
        public async Task<ActionResult<AccountBalanceDto>> GetAccountBalance(int id)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdWithDetailsAsync(id);
                if (account == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account not found"));
                }

                var balanceDto = _mapper.Map<AccountBalanceDto>(account);
                return Ok(ApiResponse<AccountBalanceDto>.SuccessResult(balanceDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance for account {AccountId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve account balance"));
            }
        }

        /// <summary>
        /// Deposit money to account
        /// </summary>
        [HttpPost("{id}/deposit")]
        public async Task<ActionResult<AccountBalanceDto>> Deposit(int id, [FromBody] DepositWithdrawDto depositDto)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdWithDetailsAsync(id);
                if (account == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account not found"));
                }

                account.Balance += depositDto.Amount;
                await _unitOfWork.Accounts.UpdateAsync(account);
                await _unitOfWork.SaveChangesAsync();

                var balanceDto = _mapper.Map<AccountBalanceDto>(account);
                return Ok(ApiResponse<AccountBalanceDto>.SuccessResult(balanceDto,
                    $"Successfully deposited {depositDto.Amount:C} to account"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error depositing to account {AccountId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to process deposit"));
            }
        }

        /// <summary>
        /// Withdraw money from account
        /// </summary>
        [HttpPost("{id}/withdraw")]
        public async Task<ActionResult<AccountBalanceDto>> Withdraw(int id, [FromBody] DepositWithdrawDto withdrawDto)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdWithDetailsAsync(id);
                if (account == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account not found"));
                }

                if (account.Balance < withdrawDto.Amount)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Insufficient funds"));
                }

                account.Balance -= withdrawDto.Amount;
                await _unitOfWork.Accounts.UpdateAsync(account);
                await _unitOfWork.SaveChangesAsync();

                var balanceDto = _mapper.Map<AccountBalanceDto>(account);
                return Ok(ApiResponse<AccountBalanceDto>.SuccessResult(balanceDto,
                    $"Successfully withdrew {withdrawDto.Amount:C} from account"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing from account {AccountId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to process withdrawal"));
            }
        }

        /// <summary>
        /// Transfer money between accounts
        /// </summary>
        [HttpPost("transfer")]
        public async Task<ActionResult> Transfer([FromBody] TransferDto transferDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var fromAccount = await _unitOfWork.Accounts.GetByIdWithDetailsAsync(transferDto.FromAccountId);
                    var toAccount = await _unitOfWork.Accounts.GetByIdWithDetailsAsync(transferDto.ToAccountId);

                    if (fromAccount == null || toAccount == null)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("One or both accounts not found"));
                    }

                    if (fromAccount.Balance < transferDto.Amount)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Insufficient funds in source account"));
                    }

                    if (fromAccount.CurrencyId != toAccount.CurrencyId)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Currency mismatch between accounts"));
                    }

                    // Perform transfer
                    fromAccount.Balance -= transferDto.Amount;
                    toAccount.Balance += transferDto.Amount;

                    var response = new TransferResponse
                    {
                        FromAccount = fromAccount.AccountNumber,
                        ToAccount = toAccount.AccountNumber,
                        Amount = transferDto.Amount,
                        Currency = fromAccount.Currency?.Code ?? "USD"
                    };

                    await _unitOfWork.Accounts.UpdateAsync(fromAccount);
                    await _unitOfWork.Accounts.UpdateAsync(toAccount);
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(ApiResponse<TransferResponse>.SuccessResult(response,
                        $"Successfully transferred {transferDto.Amount:C} from account {fromAccount.AccountNumber} to {toAccount.AccountNumber}"));
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transfer");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to process transfer"));
            }
        }

        #region Private Methods

        private async Task<string> GenerateAccountNumber()
        {
            string accountNumber;
            bool exists;

            do
            {
                // Generate a 10-digit account number
                var random = new Random();
                accountNumber = random.Next(1000000000, int.MaxValue).ToString();
                exists = await _unitOfWork.Accounts.ExistsAsync(a => a.AccountNumber == accountNumber);
            }
            while (exists);

            return accountNumber;
        }

        #endregion
    }
}

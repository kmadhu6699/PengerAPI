using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PengerAPI.Data.Repositories;
using PengerAPI.DTOs;
using PengerAPI.Models;

namespace PengerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountTypeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountTypeController> _logger;

        public AccountTypeController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AccountTypeController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all account types with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<AccountTypeSummaryDto>>> GetAccountTypes([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var accountTypes = await _unitOfWork.AccountTypes.GetPagedAsync(
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                var accountTypeDtos = _mapper.Map<List<AccountTypeSummaryDto>>(accountTypes.Items);
                
                var response = new PagedResponse<AccountTypeSummaryDto>(
                    accountTypeDtos,
                    accountTypes.TotalCount,
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                return Ok(ApiResponse<PagedResponse<AccountTypeSummaryDto>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account types");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve account types"));
            }
        }

        /// <summary>
        /// Get active account types only
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<AccountTypeSummaryDto>>> GetActiveAccountTypes()
        {
            try
            {
                var accountTypes = await _unitOfWork.AccountTypes.GetActiveAsync();
                var accountTypeDtos = _mapper.Map<List<AccountTypeSummaryDto>>(accountTypes);
                
                return Ok(ApiResponse<IEnumerable<AccountTypeSummaryDto>>.SuccessResult(accountTypeDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active account types");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve active account types"));
            }
        }

        /// <summary>
        /// Get account type by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountTypeDto>> GetAccountType(int id)
        {
            try
            {
                var accountType = await _unitOfWork.AccountTypes.GetByIdAsync(id);
                if (accountType == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account type not found"));
                }

                var accountTypeDto = _mapper.Map<AccountTypeDto>(accountType);
                return Ok(ApiResponse<AccountTypeDto>.SuccessResult(accountTypeDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account type {AccountTypeId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve account type"));
            }
        }

        /// <summary>
        /// Get account type by name
        /// </summary>
        [HttpGet("name/{name}")]
        public async Task<ActionResult<AccountTypeDto>> GetAccountTypeByName(string name)
        {
            try
            {
                var accountType = await _unitOfWork.AccountTypes.GetByNameAsync(name);
                if (accountType == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account type not found"));
                }

                var accountTypeDto = _mapper.Map<AccountTypeDto>(accountType);
                return Ok(ApiResponse<AccountTypeDto>.SuccessResult(accountTypeDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account type with name {AccountTypeName}", name);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve account type"));
            }
        }

        /// <summary>
        /// Create a new account type
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AccountTypeDto>> CreateAccountType([FromBody] CreateAccountTypeDto createAccountTypeDto)
        {
            try
            {
                // Check if account type name already exists
                var existingAccountType = await _unitOfWork.AccountTypes.GetByNameAsync(createAccountTypeDto.Name);
                if (existingAccountType != null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Account type with this name already exists"));
                }

                var accountType = _mapper.Map<AccountType>(createAccountTypeDto);

                await _unitOfWork.AccountTypes.AddAsync(accountType);
                await _unitOfWork.SaveChangesAsync();

                var accountTypeDto = _mapper.Map<AccountTypeDto>(accountType);
                return CreatedAtAction(nameof(GetAccountType), new { id = accountType.Id }, 
                    ApiResponse<AccountTypeDto>.SuccessResult(accountTypeDto, "Account type created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account type");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to create account type"));
            }
        }

        /// <summary>
        /// Update account type information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AccountTypeDto>> UpdateAccountType(int id, [FromBody] UpdateAccountTypeDto updateAccountTypeDto)
        {
            try
            {
                var accountType = await _unitOfWork.AccountTypes.GetByIdAsync(id);
                if (accountType == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account type not found"));
                }

                // Check if account type name is being changed and if it already exists
                if (updateAccountTypeDto.Name != accountType.Name)
                {
                    var existingAccountType = await _unitOfWork.AccountTypes.GetByNameAsync(updateAccountTypeDto.Name);
                    if (existingAccountType != null)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Account type name is already in use"));
                    }
                }

                _mapper.Map(updateAccountTypeDto, accountType);
                await _unitOfWork.AccountTypes.UpdateAsync(accountType);
                await _unitOfWork.SaveChangesAsync();

                var accountTypeDto = _mapper.Map<AccountTypeDto>(accountType);
                return Ok(ApiResponse<AccountTypeDto>.SuccessResult(accountTypeDto, "Account type updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account type {AccountTypeId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to update account type"));
            }
        }

        /// <summary>
        /// Delete account type
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAccountType(int id)
        {
            try
            {
                var accountType = await _unitOfWork.AccountTypes.GetByIdAsync(id);
                if (accountType == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account type not found"));
                }

                // Check if account type is being used by any accounts
                var hasAccounts = await _unitOfWork.Accounts.ExistsAsync(a => a.AccountTypeId == id);
                if (hasAccounts)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Cannot delete account type that is being used by accounts"));
                }

                await _unitOfWork.AccountTypes.DeleteAsync(accountType);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(null, "Account type deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account type {AccountTypeId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to delete account type"));
            }
        }

        /// <summary>
        /// Toggle account type active status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<AccountTypeDto>> ToggleAccountTypeStatus(int id)
        {
            try
            {
                var accountType = await _unitOfWork.AccountTypes.GetByIdAsync(id);
                if (accountType == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Account type not found"));
                }

                accountType.IsActive = !accountType.IsActive;
                await _unitOfWork.AccountTypes.UpdateAsync(accountType);
                await _unitOfWork.SaveChangesAsync();

                var accountTypeDto = _mapper.Map<AccountTypeDto>(accountType);
                var statusMessage = accountType.IsActive ? "activated" : "deactivated";
                
                return Ok(ApiResponse<AccountTypeDto>.SuccessResult(accountTypeDto, $"Account type {statusMessage} successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for account type {AccountTypeId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to update account type status"));
            }
        }
    }
}

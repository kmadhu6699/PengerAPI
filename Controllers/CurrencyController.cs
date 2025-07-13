using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PengerAPI.Data.Repositories;
using PengerAPI.DTOs;
using PengerAPI.Models;

namespace PengerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CurrencyController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all currencies with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<CurrencySummaryDto>>> GetCurrencies([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var currencies = await _unitOfWork.Currencies.GetPagedAsync(
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                var currencyDtos = _mapper.Map<List<CurrencySummaryDto>>(currencies.Items);

                var response = new PagedResponse<CurrencySummaryDto>(
                    currencyDtos,
                    currencies.TotalCount,
                    paginationParams.PageNumber,
                    paginationParams.PageSize
                );

                return Ok(ApiResponse<PagedResponse<CurrencySummaryDto>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currencies");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve currencies"));
            }
        }

        /// <summary>
        /// Get active currencies only
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CurrencySummaryDto>>> GetActiveCurrencies()
        {
            try
            {
                var currencies = await _unitOfWork.Currencies.GetActiveAsync();
                var currencyDtos = _mapper.Map<List<CurrencySummaryDto>>(currencies);

                return Ok(ApiResponse<IEnumerable<CurrencySummaryDto>>.SuccessResult(currencyDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active currencies");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve active currencies"));
            }
        }

        /// <summary>
        /// Get currency by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CurrencyDto>> GetCurrency(int id)
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetByIdAsync(id);
                if (currency == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Currency not found"));
                }

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return Ok(ApiResponse<CurrencyDto>.SuccessResult(currencyDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency {CurrencyId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve currency"));
            }
        }

        /// <summary>
        /// Get currency by code
        /// </summary>
        [HttpGet("code/{code}")]
        public async Task<ActionResult<CurrencyDto>> GetCurrencyByCode(string code)
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetByCodeAsync(code.ToUpper());
                if (currency == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Currency not found"));
                }

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return Ok(ApiResponse<CurrencyDto>.SuccessResult(currencyDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency with code {CurrencyCode}", code);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve currency"));
            }
        }

        /// <summary>
        /// Create a new currency
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CurrencyDto>> CreateCurrency([FromBody] CreateCurrencyDto createCurrencyDto)
        {
            try
            {
                // Check if currency code already exists
                var existingCurrency = await _unitOfWork.Currencies.GetByCodeAsync(createCurrencyDto.Code.ToUpper());
                if (existingCurrency != null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Currency with this code already exists"));
                }

                var currency = _mapper.Map<Currency>(createCurrencyDto);
                currency.Code = currency.Code.ToUpper(); // Ensure uppercase

                await _unitOfWork.Currencies.AddAsync(currency);
                await _unitOfWork.SaveChangesAsync();

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return CreatedAtAction(nameof(GetCurrency), new { id = currency.Id },
                    ApiResponse<CurrencyDto>.SuccessResult(currencyDto, "Currency created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating currency");
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to create currency"));
            }
        }

        /// <summary>
        /// Update currency information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CurrencyDto>> UpdateCurrency(int id, [FromBody] UpdateCurrencyDto updateCurrencyDto)
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetByIdAsync(id);
                if (currency == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Currency not found"));
                }

                // Check if currency code is being changed and if it already exists
                if (updateCurrencyDto.Code.ToUpper() != currency.Code)
                {
                    var existingCurrency = await _unitOfWork.Currencies.GetByCodeAsync(updateCurrencyDto.Code.ToUpper());
                    if (existingCurrency != null)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResult("Currency code is already in use"));
                    }
                }

                _mapper.Map(updateCurrencyDto, currency);
                currency.Code = currency.Code.ToUpper(); // Ensure uppercase

                await _unitOfWork.Currencies.UpdateAsync(currency);
                await _unitOfWork.SaveChangesAsync();

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return Ok(ApiResponse<CurrencyDto>.SuccessResult(currencyDto, "Currency updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating currency {CurrencyId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to update currency"));
            }
        }

        /// <summary>
        /// Delete currency
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCurrency(int id)
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetByIdAsync(id);
                if (currency == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Currency not found"));
                }

                // Check if currency is being used by any accounts
                var hasAccounts = await _unitOfWork.Accounts.ExistsAsync(a => a.CurrencyId == id);
                if (hasAccounts)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Cannot delete currency that is being used by accounts"));
                }

                await _unitOfWork.Currencies.DeleteAsync(currency);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(null, "Currency deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting currency {CurrencyId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to delete currency"));
            }
        }

        /// <summary>
        /// Toggle currency active status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<CurrencyDto>> ToggleCurrencyStatus(int id)
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetByIdAsync(id);
                if (currency == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Currency not found"));
                }

                currency.IsActive = !currency.IsActive;
                await _unitOfWork.Currencies.UpdateAsync(currency);
                await _unitOfWork.SaveChangesAsync();

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                var statusMessage = currency.IsActive ? "activated" : "deactivated";

                return Ok(ApiResponse<CurrencyDto>.SuccessResult(currencyDto, $"Currency {statusMessage} successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for currency {CurrencyId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to update currency status"));
            }
        }
    }
}

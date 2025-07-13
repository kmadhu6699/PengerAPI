namespace PengerAPI.DTOs
{
    // Standard API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResult(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ApiResponse<T> ErrorResult(string message, string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }

        public static ApiResponse<T> ValidationError(IEnumerable<FluentValidation.Results.ValidationFailure> errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors.Select(e => e.ErrorMessage).ToList()
            };
        }
    }

    // Paginated response
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public PagedResponse(List<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            HasNextPage = pageNumber < TotalPages;
            HasPreviousPage = pageNumber > 1;
        }
    }

    // Pagination request parameters
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    // Search and filter parameters
    public class SearchParams : PaginationParams
    {
        public string SearchTerm { get; set; }
        public string Query => SearchTerm; // Alias for backward compatibility
        public string SortBy { get; set; }
        public string SortOrder { get; set; } = "asc"; // asc or desc
    }

    // Validation error response
    public class ValidationErrorResponse
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public object AttemptedValue { get; set; }
    }

    // Health check response
    public class HealthCheckResponse
    {
        public required string Status { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
        public string Environment { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class OTPCleanupResponse
    {
        public int DeletedCount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class DeleteResponse
    {
        public int Id { get; set; }
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class PasswordChangeResponse
    {
        public int UserId { get; set; }
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class TransferResponse
    {
        public string FromAccount { get; set; } = string.Empty;
        public string ToAccount { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}


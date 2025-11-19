namespace BuyMate.DTO.Common
{
    public class PaginatedResponse<TEntity>
    {
        public PaginatedResponse(TEntity? data, string? message, bool? status, int totalCount = 0, int pageSize = 0, int pageNumber = 0)
        {
            Data = data;
            Message = message;
            Status = status;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public PaginatedResponse() { }

        public TEntity? Data { get; set; }
        public string? Message { get; set; }
        public bool? Status { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}

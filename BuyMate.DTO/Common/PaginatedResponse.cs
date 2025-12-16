namespace BuyMate.DTO.Common
{
    public class PaginatedResponse<TEntity> : Response<TEntity>
    {
        public PaginatedResponse() { }

        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        // ovrride Success and Fail methods to include pagination info
        public static PaginatedResponse<TEntity> Success(TEntity data, int totalCount, int pageSize, int pageNumber, string? message = null) =>
            new PaginatedResponse<TEntity>
            {
                Status = true,
                Data = data,
                Message = message!,
                TotalCount = totalCount,
                PageSize = pageSize,
                PageNumber = pageNumber
            };
        public static PaginatedResponse<TEntity> Fail(string message) =>
            new PaginatedResponse<TEntity>
            {
                Status = false,
                Data = default!,
                Message = message
            };
    }
}

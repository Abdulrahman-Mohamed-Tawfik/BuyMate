namespace BuyMate.DTO.Common
{
    public class Response<TEntity>
    {
        public Response(TEntity? data, string? message, bool? status)
        {
            Data = data;
            Message = message;
            Status = status;
        }
        public Response()
        {

        }
        public TEntity? Data { get; set; }

        public string? Message { get; set; }

        public bool? Status { get; set; }

    }
}

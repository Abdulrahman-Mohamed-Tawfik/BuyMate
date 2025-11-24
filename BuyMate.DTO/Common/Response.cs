namespace BuyMate.DTO.Common
{
    public class Response<T>
    {
        public required bool Status { get; set; }
        public required string Message { get; set; }
        public T? Data { get; set; }

        public static Response<T> Success(T data, string? message = null) =>
            new Response<T> { Status = true, Data = data, Message = message };

        public static Response<T> Fail(string message) =>
            new Response<T> { Status = false, Message = message, Data = default };

    }
}

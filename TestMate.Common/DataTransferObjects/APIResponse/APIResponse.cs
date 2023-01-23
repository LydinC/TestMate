using TestMate.Common.Enums;

namespace TestMate.Common.DataTransferObjects.APIResponse
{

    public class APIResponse<T>
    {
        public Status Status { get; set; }

        public string Message { get; set; }

        public T? Data { get; set; }

        public bool Success { get { return this.Status == Status.Ok; } }


        public APIResponse(Status status, string message, T data)
        {
            Status = status;
            Message = message;
            Data = data;
        }

        public APIResponse(Status status, string message)
        {
            Status = status;
            Message = message;
        }
        public APIResponse()
        {
            Status = Status.Ok;
            Message = "Successful";
        }

        public APIResponse(T data) : this()
        {
            this.Data = data;
        }

    }
}

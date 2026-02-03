namespace RootLibrary.Models.Response
{
    public class ErrorResponse
    {
        public string Message { get; set; }          
        public int Code { get; set; }


        public ErrorResponse()
        {
            
        }

        public ErrorResponse(string message, int code = 500)
        {
            Message = message;
            Code = code;
        }
    }
}
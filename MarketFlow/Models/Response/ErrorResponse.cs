namespace MarketPlace.Models.Response
{
    public class ErrorResponse
    {
        public string Message { get; set; }          
        public int Code { get; set; }       
        public object Details { get; set; }          

        public ErrorResponse() { }

        public ErrorResponse(string message, int code = 500, object details = null)
        {
            Message = message;
            Code = code;
            Details = details;
        }
    }
}
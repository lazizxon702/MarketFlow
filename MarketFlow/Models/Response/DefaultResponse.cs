namespace MarketPlace.Models.Response
{
    public class DefaultResponse<T>
    {
        public bool Success { get; set; }           
        public string Message { get; set; }         
        public T Data { get; set; }                 
        public ErrorResponse Error { get; set; }    

        public DefaultResponse() { }

       
        public DefaultResponse(T data, string message = null)
        {
            Success = true;
            Data = data;
            Message = message;
        }

       
        public DefaultResponse(ErrorResponse error)
        {
            Success = false;
            Error = error;
            Message = error?.Message;
        }
    }
}
namespace Acceloka.Api.Common
{
    public class ApiExceptions : Exception
    {
        public int StatusCode { get; }

        public ApiExceptions(string message, int statusCode)
            :base(message)
        {
            StatusCode = statusCode; 
        }
    }
}

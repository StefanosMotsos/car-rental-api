namespace CarRentalApp.Exceptions
{
    public class InvalidArgumentException : AppException
    {

        private static readonly string DEFAULT_CODE = "InvalidArgument";

        public InvalidArgumentException(string entity, string message) : base(entity + DEFAULT_CODE, string.Format(message, entity))
        {
        }
    }
}

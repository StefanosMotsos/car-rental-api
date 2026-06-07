namespace CarRentalApp.Exceptions
{
    public class EntityAlreadyExistsException : AppException
    {

        private static readonly string DEFAULT_CODE = "AlreadyExists";

        public EntityAlreadyExistsException(string entity, string message)
            : base(entity + DEFAULT_CODE, string.Format(message, entity))
        {
        }
    }
}

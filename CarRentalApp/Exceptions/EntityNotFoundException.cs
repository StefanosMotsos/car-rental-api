namespace CarRentalApp.Exceptions
{
    public class EntityNotFoundException : AppException
    {

        private static readonly string DEFAULT_CODE = "NotFound";

        public EntityNotFoundException(string entity, string message) : base(entity + DEFAULT_CODE, string.Format(message, entity))
        {
        }
    }
}

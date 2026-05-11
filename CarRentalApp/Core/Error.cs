namespace CarRentalApp.Core
{
    public class Error
    {
        public Error()
        {
        }

        public Error(string? code, string? field, string? message)
        {
            Code = code;
            Field = field;
            Message = message;
        }

        public string? Code { get; set; }
        public string? Field { get; set; }
        public string? Message { get; set; }


    }
}

namespace Utilities.Classes.Common
{
    public class ApiError
    {
        public Guid Identifier { get; set; }
        public short Status { get; set; }
        public string Links { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }

        public ApiError(Guid identifier)
        {
            Identifier = identifier;
            Status = 500;
        }
    }
}

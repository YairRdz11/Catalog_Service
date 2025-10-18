namespace Utilities.Classes.Common
{
    public class ApiResponse
    {
        public object Result { get; set; }
        public int Status { get; set; }
        public List<RuleError> RuleErrors { get; set; }
        public ApiError ApiError { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using Utilities.Classes.Common;

namespace ApiUtilities.Classes.Common
{
    public class ApiControllerBase : ControllerBase
    {
        public ApiResponse ApiActionResponse{ get; set; }

        public ApiControllerBase()
        {
            ApiActionResponse = new ApiResponse();
            ApiActionResponse.Status = 200;
        }
    }
}

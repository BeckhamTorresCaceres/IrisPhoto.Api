using IrisPhoto_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IrisPhoto_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IUsersServices _usersService;

        public HomeController(IUsersServices usersService)
        {
            _usersService = usersService;
        }

        [HttpGet("ServiceInformation")]
        public string ServiceInformation()
        {
            return $"API_IrisPhoto -> v.1 - {DateTime.Now}";
        }
    }
}

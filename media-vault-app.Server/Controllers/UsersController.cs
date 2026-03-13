using media_vault_app.Application.Interfaces.Repos;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepo _userRepo;

        public UsersController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }



    }
}

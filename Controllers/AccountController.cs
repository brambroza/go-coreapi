using System.Net;
using goalongapi.Datatools.Account;
using goalongapi.Entities;
using goalongapi.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace goalongapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        public AccountController(IAccountService accountService) => this.accountService = accountService;

        [HttpPost("[action]")]
        public async Task<ActionResult> Register(RegisterRequest registerRequest)
        {
            var account = registerRequest.Adapt<Account>();
            await accountService.Register(account);
            return StatusCode((int)HttpStatusCode.Created);
        }
        [EnableCors("AllowSpecificOrigins")]
        [HttpPost("[action]")]
        public async Task<ActionResult> Login(LoginRequest loginRequest)
        {
            var account = await accountService.Login(loginRequest.Username, loginRequest.Password);
            if (account == null)
            {
                return Unauthorized();
            }

            return Ok(new { token = accountService.GenerateToken(account) });

        }

        [HttpGet("[action]")]
        public async Task<ActionResult> Info()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null){
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);
            return Ok(new {
                username = account.Username,
                role = account.Role.Name
            });
        }
        

    }
}
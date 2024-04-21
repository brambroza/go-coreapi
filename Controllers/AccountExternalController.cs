using System.Net;
using goalongapi.Datatools.Account;
using goalongapi.Entities;
using goalongapi.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Net.Mail;
using coreapi.Models;
using System.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop.Infrastructure;

 
namespace coreapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountExternalController : ControllerBase
    {

        private readonly SignInManager<IdentityUser> signInManager;
        
           [HttpPost("[action]")]
        public  IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "AccountExternal",
            new { ReturnUrl = returnUrl });

            var properties =
                 signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }


        
    }
}
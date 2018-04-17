using DotNetCoreAPIWithReactAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreAPIWithReactAuthentication.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtOptions _jwtOptions;
        //private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOptions<JwtOptions> jwtOptions,

    //IEmailSender emailSender,
    ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            _logger = logger;
        }

        [HttpPost("api/auth/login")]
        [AllowAnonymous]
        //[ValidateModel]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return BadRequest(new
                {
                    error = "",
                    error_description = "Username or password is invalid."
                });
            }

            if(!await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new
                {
                    error = "email_not_confirmed",
                    error_description = "You must have a confirmed email to log in."
                });
            }

            _logger.LogInformation($"User logged in (id: {user.Id}");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.issuer,
                audience: _jwtOptions.issuer,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token) 
                }
            );
        }
    }
}

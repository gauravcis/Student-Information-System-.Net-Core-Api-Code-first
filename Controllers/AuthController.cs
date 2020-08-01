using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SIMS.Models;
using SIMS.Services;

namespace SIMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController  : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly SIMSContext _SIMSContext;
        private readonly IEmailService _emailService;
        public AuthController(SIMSContext sIMSContext,IConfiguration config,IEmailService emailService)
        {
            _SIMSContext = sIMSContext;
            _config = config;
            _emailService = emailService;
        }

        // Do Student Login 
        [HttpPost("login")]
        public IActionResult DoLogin([FromBody]AuthModel authModel)
        {

            var user = _SIMSContext.Students.Where(e => e.Email == authModel.EmailOrId && e.Password == authModel.Password).FirstOrDefault();

            if (user != null)
            {
                //Genrate Access (JWT) token
                var tokenStr = GenrateJSONWebToken(user);

                //Genrate Refresh token
                RefreshToken refreshToken = GenrateRefreshToken();

                refreshToken.StudentId = user.StudentId;
                _SIMSContext.refreshTokens.Add(refreshToken);
                _SIMSContext.SaveChanges();

                var JwtTokenExpirey = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:JwtExpire"]));
                var RefreshTokenExpire = DateTime.UtcNow.AddYears(Convert.ToInt32(_config["Jwt:JwtRefreshExpire"]));


                return Ok(new
                {
                    Name = user.FullName,
                    Email = user.Email,
                    status = 200,
                    message = "Login Success",
                    CurrentTime = DateTime.Now,
                    JwtToken = tokenStr,
                    JwtTokenExpirey = JwtTokenExpirey,
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpire = RefreshTokenExpire,
                    Auth = true,

                    
                });
            }
            else
            {
                return Ok(new { status = 401, message = "Invalid Id or Password" });
            }

        }

        // Do Student Login 
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> SendEmailAsync([FromBody]EmailModel EmailModel)
        { 
            var User = _SIMSContext.Students.Where(id => id.Email == EmailModel.Email).FirstOrDefault();

            if(User == null)
            {
                return Ok(new { status = 200, message = "User Does not Exist" });
            }
            else
            {
                try
                {
                    Random generator = new Random();
                    int otp = generator.Next(10000, 99999);
                    var Subject = "Email Verification | SIMS";
                    var Message = "Your verification Code is :"+otp;

                    await _emailService.SendEmail(EmailModel.Email, Subject, Message);
                    return Ok(new { status = 200, message = "Email send Successfully " });
                }
                catch(Exception e)
                {
                    return Ok(new { status = 200, message = e.Message });
                }
               
            }
           
        }

        
        // Refresh Token Controller 
        [HttpPost("RefreshToken")]
        public IActionResult RefreshToken([FromBody]RefreshRequest refreshRequest)
        {
            //getting user from existing JWt Token
            Student user = GetUserFromAccessToken(refreshRequest.AccessToken);


            if (user != null && validateRefreshToken(user, refreshRequest.RefreshToken))
            {
                var userWithToken = GenrateJSONWebToken(user);

                return Ok(new { newjwtToken = userWithToken });
            }
            else {
                return BadRequest();
            }


            
        }

        //genrate JWT (AccessToken)
        private string GenrateJSONWebToken(Student Studentinfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {

                new Claim(JwtRegisteredClaimNames.Sub,Studentinfo.FullName),
                new Claim(JwtRegisteredClaimNames.Email,Studentinfo.Email),
                new Claim(ClaimTypes.Name, Convert.ToString(Studentinfo.StudentId)),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:key"],
                audience: _config["Jwt:issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:JwtExpire"])), //after how much time we need refresh token 
                signingCredentials: credentials
                );
            IdentityModelEventSource.ShowPII = true;
            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);

            return encodetoken;
        }

        //genrate Refresh Token (AccessToken)
        private RefreshToken GenrateRefreshToken()
        {
            RefreshToken refreshToken = new RefreshToken();

            var randomNumber = new byte[32];
            using(var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.Token =  Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddYears(Convert.ToInt32(_config["Jwt:JwtRefreshExpire"])); // refresh token can use upto
            return refreshToken;
        }

        private Student GetUserFromAccessToken(string accessToken)
        {
            try
            {
                var tokenHandeler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["Jwt:key"]);

                var tokenValidationParameter = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromDays(3) // inactivity time
                  //  ClockSkew = TimeSpan.FromSeconds(1)
                };
                SecurityToken securityToken;
                var principle = tokenHandeler.ValidateToken(accessToken, tokenValidationParameter, out securityToken);

                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    var userID =  principle.FindFirst(ClaimTypes.Name)?.Value;
                    var userEmail = principle.FindFirst(ClaimTypes.Email)?.Value;

                    return _SIMSContext.Students.Where(usr => usr.Email == userEmail && usr.StudentId.ToString() == userID).FirstOrDefault();
                }
            }
            catch(Exception e)
            {
                return new Student();
            }
            return new Student();


        }

        private bool validateRefreshToken(Student user, string refreshToken)
        {
            RefreshToken refreshTokenUser =  _SIMSContext.refreshTokens.Where(rt => rt.Token == refreshToken)
                .OrderByDescending(rt => rt.ExpiryDate).FirstOrDefault();

            if(refreshTokenUser != null && refreshTokenUser.StudentId == user.StudentId && refreshTokenUser.ExpiryDate > DateTime.UtcNow)
            {
                return true;
            }
            return false;
        }

        
    }
}
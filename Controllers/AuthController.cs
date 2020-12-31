﻿using System;
using System.Text;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NetCoreAuthJwtMySql.Utils;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;


namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration )
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public ActionResult<ResponseLogin> Login(RequestLogin requestLogin)
        {
            var responseLogin = new ResponseLogin();
            {
              
                var claimList = new List<Claim>();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expireDate = DateTime.UtcNow.AddDays(1);
                var timeStamp = DateUtil.ConvertToTimeStamp(expireDate);
                var token = new JwtSecurityToken(
                    claims: claimList,
                    notBefore: DateTime.UtcNow,
                    expires: expireDate,
                    signingCredentials: creds);
                responseLogin.Success = true;
                responseLogin.Token = new JwtSecurityTokenHandler().WriteToken(token);
                responseLogin.ExpireDate = timeStamp;
             
               
            }
            return responseLogin;
        }
        
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public ActionResult<ResponseRegister> Register(RequestRegister requestRegister)
        {
            var responseRegister = new ResponseRegister();
            using (var db = new MySqlContext())
            {
                if (!db.User.Any(x => x.Email == requestRegister.Email))
                {
                    var email = requestRegister.Email;
                    var salt = CryptoUtil.GenerateSalt();
                    var password = requestRegister.Password;
                    var hashedPassword = CryptoUtil.HashMultiple(password, salt);
                    var user = new User();
                    user.Email = email;
                    user.Salt = salt;
                    user.Password = hashedPassword;
                    user.Role = "USER";
                    db.User.Add(user);
                    db.SaveChanges();
                    responseRegister.Success = true;
                }
                else
                {
                    responseRegister.MessageList.Add("Email is already in use");
                }
            }
            return responseRegister;
        }
    }
}

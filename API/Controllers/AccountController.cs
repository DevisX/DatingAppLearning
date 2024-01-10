

using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService): base(context){
            _tokenService = tokenService;
        }

        [HttpPost("register")] //api/account/register?username=devis&password=pwt
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){

            if(await UserExist(registerDto.Username)) return BadRequest("User alredy exist");

            using var hmac= new HMACSHA512();

            var user = new AppUser{
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                PasswordSalt = hmac.Key,
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)      
            };
        }

        [HttpPost("login")]

        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user = await _context.Users.SingleOrDefaultAsync(x =>
            x.UserName == loginDto.UserName);

            if(user == null) return Unauthorized("Invalid username");

            using var hmac= new HMACSHA512(user.PasswordSalt); //converto il passwordSalt in modo tale che se mi ritrni un passwrod hash

            var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i=0; i< ComputedHash.Length; i++){

                if(ComputedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)      
            };

        }

        private async Task<bool> UserExist(String username){
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
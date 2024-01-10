using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    
    

    public UsersController(DataContext context): base(context)
    {
       
    }

    [AllowAnonymous]
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUser()
    {
        var users = await _context.Users.ToListAsync();
        return users;
    }

    

    [HttpGet("{id}")] //api/users/{id}
    public async Task<ActionResult<AppUser>> GetUSer(int id)
    {
        var user = await _context.Users.FindAsync(id);

        return user;
    }

}

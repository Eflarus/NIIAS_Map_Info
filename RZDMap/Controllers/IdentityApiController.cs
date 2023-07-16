﻿using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RZDMap.DTO.ViewModels;
using RZDMap.Services.Token;

namespace RZDMap.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/identity")]
public class IdentityApiController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IJwtTokenGenerator _jwtToken;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityApiController(UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager, 
        IJwtTokenGenerator jwtToken,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtToken = jwtToken;
        _roleManager = roleManager;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var userFromDb = await _userManager.FindByNameAsync(model.Username);

        if (userFromDb == null)
        {
            return BadRequest();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(userFromDb, model.Password, false);


        if (!result.Succeeded)
        {
            return BadRequest();
        }
        
        var roles = await _userManager.GetRolesAsync(userFromDb);
        
        IList<Claim> claims = await _userManager.GetClaimsAsync(userFromDb);
        return Ok(new
        {
            result = result,
            username = userFromDb.UserName,
            email = userFromDb.Email,
            token = _jwtToken.GenerateToken(userFromDb, roles, claims)
        });
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!(await _roleManager.RoleExistsAsync(model.Role)))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }
        
        var userToCreate = new IdentityUser
        {
            Email = model.Email,
            UserName = model.Username
        };
        
        var result = await _userManager.CreateAsync(userToCreate, model.Password);

        if (result.Succeeded)
        {
            var userFromDb = await _userManager.FindByNameAsync(userToCreate.UserName);
            await _userManager.AddToRoleAsync(userFromDb, model.Role);
            
            var claim = new Claim("JobTitle", model.JobTitle);

            await _userManager.AddClaimAsync(userFromDb, claim);
            
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost]
    [Route("confirmemail")]
    public IActionResult ConfirmEmail(ConfirmEmailViewModel model)
    {
        return Ok();
    }
}
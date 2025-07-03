
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{

    AuthTokenManager? _authManager;
    private readonly UserManager<ApplicationUser>? _userManager;

    public AuthController(AuthTokenManager? authManager, UserManager<ApplicationUser>? userManager)
    {
        _authManager = authManager;
        _userManager = userManager;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto user)
    {

        var DbUser = await _userManager!.FindByEmailAsync(user.Email);
        if (DbUser != null)
            return BadRequest();
        var NewUser = new ApplicationUser { Email = user.Email, UserName = user.UserName };
        var created = await _userManager.CreateAsync(NewUser, user.Password);
        if (created.Succeeded)
            return Ok("user created successfully");
        return BadRequest();
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto user)
    {

        var dbUser = await _userManager!.FindByEmailAsync(user.Email);
        if (dbUser == null || !await _userManager.CheckPasswordAsync(dbUser, user.Password))
            return Unauthorized();
        var JWT = _authManager!.GenerateJWT(dbUser);
        var refreshToken = _authManager.GenerateRefreshToken();
        dbUser.RefreshToken = refreshToken;
        dbUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(dbUser);
        return Ok(new TokneBearerDTO
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(JWT),
            RefreshToken = refreshToken
        });

    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh([FromBody] TokneBearerDTO tokenRequest)
    {


        var principal = _authManager!.GetPrincipalFromExpiredToken(tokenRequest.AccessToken);
        if (principal == null)
            return BadRequest("Invalid access token or refresh token");
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager!.FindByIdAsync(userId!);
        if (user == null || user.RefreshToken != tokenRequest.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Unauthorized("Invalid refresh token");
        var newAccessToken = _authManager.GenerateJWT(user);
        var newRefreshToken = _authManager.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);
        return Ok(new TokneBearerDTO
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            RefreshToken = newRefreshToken
        });
    }

}




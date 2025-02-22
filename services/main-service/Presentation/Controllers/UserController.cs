using MainService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MainService.Presentation.DTOs;
using MainService.Domain.UseCases;

namespace MainService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserUseCase _userUseCase;

    public UserController(UserUseCase userUseCase)
    {
        _userUseCase = userUseCase;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest userRequest)
    {
        if (string.IsNullOrWhiteSpace(userRequest.Username) ||
            string.IsNullOrWhiteSpace(userRequest.Email) ||
            string.IsNullOrWhiteSpace(userRequest.Password))
        {
            return BadRequest("All fields are required.");
        }

        var newUser = new UserDomain
        {
            Username = userRequest.Username,
            Email = userRequest.Email,
            Password = userRequest.Password,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userUseCase.CreateUser(newUser);
        return Ok(new { Message = "User created successfully", User = createdUser });
    }
}

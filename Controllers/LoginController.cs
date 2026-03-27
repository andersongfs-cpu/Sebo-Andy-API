using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sebo_Andy.Data;
using Sebo_Andy.DTOs;
using Sebo_Andy.Services;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly TokenService _tokenService;
	public LoginController(AppDbContext context, TokenService tokenService)
	{
		_context = context;
		_tokenService = tokenService;
	}

	[HttpPost]
	public async Task<ActionResult<dynamic>> Autenticar([FromBody] LoginDto loginDto)
	{
		// Procura o usuário
		var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

		// Retorna 401 se email for inválido.
		if (user == null)
		{
			return Unauthorized(new { mensagem = "E-Mail ou senha inválidos." });
		}

		bool senhaValida = BCrypt.Net.BCrypt.Verify(loginDto.Senha, user.Senha);

		if (!senhaValida)
		{
			return Unauthorized("E-Mail ou senha inválidos.");
		}

		var token = _tokenService.GerarToken(user);

		return Ok(new
		{
			usuario = user.Nome,
			token = token
		});
	}
}


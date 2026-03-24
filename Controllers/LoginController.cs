using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sebo_Andy.Data;
using Sebo_Andy.DTOs;
using Sebo_Andy.Models;
using Sebo_Andy.Services;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	private readonly AppDbContext _context;
	public LoginController(AppDbContext context)
	{
		_context = context;
	}

	[HttpPost]
	public async Task<ActionResult<dynamic>> Autenticar([FromBody] LoginDto model)
	{
		// Procura o usuário
		var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);
		// Erro se o email não existir OU a senha inserida for diferente da senha cadastrada
		if (user == null || user.Senha != model.Senha)
		{
			return Unauthorized(new { mensagem = "Login ou senha inválidos." });
		}

		// Gera token
		var token = TokenService.GerarToken(user);

		return Ok(new
		{
			usuario = user.Nome,
			token = token
		});
	}
}


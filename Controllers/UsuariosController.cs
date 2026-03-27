using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sebo_Andy.Data;
using Sebo_Andy.DTOs;
using Sebo_Andy.Models;
using Sebo_Andy.Services;

namespace Sebo_Andy.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsuariosController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly AuthServices _auth;

		public UsuariosController(AppDbContext context, AuthServices auth)
		{
			_context = context;
			_auth = auth;
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<UsuarioGetDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<List<UsuarioGetDto>>> GetTodos(
		[FromQuery] int? id,
		[FromQuery] string? nome,
		[FromQuery] string? email,
		[FromQuery] TipoCargo? cargo)
		{
			var query = _context.Usuarios.AsQueryable();

			if (id.HasValue)
			{
				query = query.Where(u => u.Id == id.Value);
			}

			if (!string.IsNullOrWhiteSpace(nome))
			{
				query = query.Where(u => u.Nome.Contains(nome));
			}

			if (!string.IsNullOrWhiteSpace(email))
			{
				query = query.Where(u => u.Email.Contains(email));
			}

			if (cargo.HasValue)
			{
				query = query.Where(u => u.Cargo == cargo.Value);
			}

			var usuariosDto = await query
				.Select(u => new UsuarioGetDto
				{
					Nome = u.Nome,
					Email = u.Email,
					Cargo = u.Cargo.ToString()
				})
				.ToListAsync();

			if (usuariosDto.Count == 0)
			{
				return NotFound("Usuário não encontrado!");
			}

			return Ok(usuariosDto);
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> AddUsuario(UsuarioPostDto novoUsuarioDto)
		{
			string senhaComHash = BCrypt.Net.BCrypt.HashPassword(novoUsuarioDto.Senha);

			var usuarioParaSalvar = new Usuario
			{
				Nome = novoUsuarioDto.Nome,
				Email = novoUsuarioDto.Email,
				Senha = senhaComHash, //Salvando a senha com hash
				Cargo = novoUsuarioDto.Cargo
			};

			_context.Usuarios.Add(usuarioParaSalvar);
			await _context.SaveChangesAsync();

			return Ok(usuarioParaSalvar);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DelUsuario(int id)
		{
			var removerUsuario = await _context.Usuarios.FindAsync(id);

			if (removerUsuario == null)
			{
				return NotFound("Usuário não encontrado.");
			}

			_context.Usuarios.Remove(removerUsuario);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpPut("{id}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> EditUsuario(int id, UsuarioUpdateDto editDto)
		{
			var atualUsuario = await _context.Usuarios.FindAsync(id);

			if (atualUsuario == null)
			{
				return NotFound($"Usuário de Id {id} não encontrado!");
			}

			if (!string.IsNullOrWhiteSpace(editDto.Nome) && editDto.Nome != "string")
			{
				atualUsuario.Nome = editDto.Nome;
			}

			if (!string.IsNullOrWhiteSpace(editDto.Email) && editDto.Email != "string")
			{
				atualUsuario.Email = editDto.Email;
			}

			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}

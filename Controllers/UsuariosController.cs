using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
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
		public async Task<IActionResult> GetTodos([FromHeader] int adminId)
		{
			// Verifica cargo do usuario
			if (await _auth.EhAdmin(adminId))
			{
				var todosUsuarios = await _context.Usuarios.ToListAsync();
				return Ok(todosUsuarios);
			}

			var usuariosDto = await _context.Usuarios				
				.Select(u => new UsuarioGetDto
				{ 
					Nome = u.Nome, 
					Email = u.Email, 
					Cargo = u.Cargo.ToString()
				})
				.ToListAsync();
			
			return Ok(usuariosDto);
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetPorId(int id, [FromHeader] int usuarioId)
		{
			var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

			if (usuario == null)
			{
				return NotFound("Usuário não cadastrado no sistema.");
			}

			if(await _auth.EhAdmin(usuarioId))
			{
				return Ok(usuario);
			}

			var usuarioDto = new UsuarioGetDto
			{
				Nome = usuario.Nome,
				Email = usuario.Email,
				Cargo = usuario.Cargo.ToString()
			};

			return Ok(usuarioDto);
		}

		[HttpPost]
		public async Task<IActionResult> AddUsuario(UsuarioPostDto novoUsuarioDto, [FromHeader] int usuarioId)
		{
			var usuarioParaSalvar = new Usuario
			{
				Nome = novoUsuarioDto.Nome,
				Email = novoUsuarioDto.Email ?? "Sem email",
				Cargo = TipoCargo.Cliente
			};

			if (await _auth.EhAdmin(usuarioId))
			{
				usuarioParaSalvar.Cargo = novoUsuarioDto.Cargo;
			}

			_context.Usuarios.Add(usuarioParaSalvar);
			await _context.SaveChangesAsync();

			return Ok(usuarioParaSalvar);
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DelUsuario(int id)
		{
			var removerUsuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
			if (removerUsuario == null)
			{
				return NotFound($"Usuário de Id {id} não encontrado!");
			}

			_context.Usuarios.Remove(removerUsuario);
			await _context.SaveChangesAsync();
			return Ok(new {mensagem = $"Usuário de Id {id} foi removido com sucesso!", removerUsuario});
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> EditUsuario(int id, Usuario editUsuario)
		{
			var atualUsuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
			if(atualUsuario == null)
			{
				return NotFound($"Usuário de Id {id} não encontrado!");
			}

			atualUsuario.Nome = editUsuario.Nome;
			atualUsuario.Email = editUsuario.Email;

			await _context.SaveChangesAsync();
			return Ok(new {mensagem = $"Usuário de Id {id} editado com sucesso!", atualUsuario });
		}
	}
}

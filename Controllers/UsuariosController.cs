using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Sebo_Andy.DTOs;
using Sebo_Andy.Models;
using Sebo_Andy.Data;
using Sebo_Andy.Services;
using Microsoft.Identity.Client;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations.Schema;

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
		public ActionResult GetTodos([FromHeader] int adminId)
		{
			// Verifica cargo do usuario
			if (_auth.EhAdmin(adminId))
			{
				return Ok(_context.Usuarios.ToList());
			};

			var usuariosDto = _context.Usuarios
				.ToList()
				.Select(u => new UsuarioGetDto{ Nome = u.Nome})
				.ToList();
			
			return Ok(usuariosDto);
		}

		[HttpGet("{id:int}")]
		public ActionResult<Usuario> GetPorId(int id)
		{
			var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
			if(usuario == null)
			{
				return NotFound("Usuário não encontrado!");
			}

			TipoCargo dados = usuario.Cargo;
			string nomeCargo = dados.ToString();

			return Ok(new { mensagem = "Usuário Encontrado", usuario, nomeCargo});
		}

		[HttpPost]
		public IActionResult AddUsuario(Usuario novoUsuario)
		{
			_context.Usuarios.Add(novoUsuario);
			_context.SaveChanges();

			return Ok(new{mensagem = "Usuário adicionado com sucesso!",	novoUsuario});
		}

		[HttpDelete("{id:int}")]
		public IActionResult DelUsuario(int id)
		{
			var removerUsuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
			if (removerUsuario == null)
			{
				return NotFound($"Usuário de Id {id} não encontrado!");
			}

			_context.Usuarios.Remove(removerUsuario);
			_context.SaveChanges();
			return Ok(new {mensagem = $"Usuário de Id {id} foi removido com sucesso!", removerUsuario});
		}

		[HttpPut("{id:int}")]
		public IActionResult EditUsuario(int id, Usuario editUsuario)
		{
			var atualUsuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
			if(atualUsuario == null)
			{
				return NotFound($"Usuário de Id {id} não encontrado!");
			}
			atualUsuario.Nome = editUsuario.Nome;
			atualUsuario.Email = editUsuario.Email;
			_context.SaveChanges();
			return Ok(new {mensagem = $"Usuário de Id {id} editado com sucesso!", atualUsuario });
		}
	}
}

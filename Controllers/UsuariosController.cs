using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sebo_Andy.Models;
using Sebo_Andy.Data;

namespace Sebo_Andy.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsuariosController : ControllerBase
	{
		private readonly AppDbContext _context;

		public UsuariosController(AppDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public ActionResult<List<Usuario>> GetTodos()
		{			
			return Ok(_context.Usuarios.ToList());
		}

		[HttpGet("{id:int}")]
		public ActionResult<Usuario> GetPorId(int id)
		{
			var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
			if(usuario == null)
			{
				return NotFound("Usuário não encontrado!");
			}
			return Ok(new {mensagem = "Usuário Encontrado", usuario });
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

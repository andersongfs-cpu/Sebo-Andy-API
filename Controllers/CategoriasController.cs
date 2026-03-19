using Microsoft.AspNetCore.Mvc;
using Sebo_Andy.Data;
using Sebo_Andy.Models;
using Sebo_Andy.Services;


namespace Sebo_Andy.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CategoriasController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly AuthServices _auth;

		public CategoriasController(AppDbContext context, AuthServices auth)
		{
			_context = context;
			_auth = auth;
		}

		// Método para listar as categorias de livros cadastradas.
		[HttpGet]
		public ActionResult<List<Categoria>> GetTodos()
		{
			return Ok(new
			{
				mensagem = "Lista de categorias.",
				categorias = _context.Categorias.ToList()
			});
		}

		// Método para adicionar uma nova categoria
		[HttpPost]
		public ActionResult PostNovaCategoria(Categoria novaCategoria, [FromHeader] int adminId)
		{
			// Verifica se o usuário é admin através do AuthService.cs // ! = diferente de
			if (!_auth.EhAdmin(adminId))
			{
				return StatusCode(403, "Acesso negado! Apenas administradores podem adicionar novas categorias!");
			}
			// Se usuário = admin código continua
			_context.Categorias.Add(novaCategoria);
			_context.SaveChanges();

			return Ok(new
			{
				mensagem = "Categoria adicionada com sucesso!",
				dados = novaCategoria
			});
		}

		[HttpDelete("{id:int}")]
		public ActionResult DeleteCategoria(int id, [FromHeader] int adminId)
		{
			// Verifica se o usuário é admin através do AuthService.cs // ! = diferente de
			if (!_auth.EhAdmin(adminId))
			{
				return StatusCode(403, "Acesso negado! Apenas administradores podem excluir categorias!");
			}
			// Se usuário = admin código continua			
			var delCategoria = _context.Categorias.FirstOrDefault(c => c.Id == id);
			if (delCategoria == null)
			{
				return NotFound("Categoria não encontrada!");
			}
			string nomeCategoria = delCategoria.Nome;
			_context.Categorias.Remove(delCategoria);
			_context.SaveChanges();
			return Ok(new { mensagem = $"Categoria {nomeCategoria} removida com sucesso!" });
		}

		[HttpPut("{id:int}")]
		public ActionResult EditarCategoria(int id, [FromHeader] int adminId, Categoria categoriaAtt)
		{
			// Verifica se o usuário é admin através do AuthService.cs // ! = diferente de
			if (!_auth.EhAdmin(adminId))
			{
				return StatusCode(403, "Acesso Negado! Apenas administradores podem editar categorias!");
			}

			// Se usuário = admin código continua
			var categoriaOriginal = _context.Categorias.FirstOrDefault(c => c.Id == id);
			if (categoriaOriginal == null)
			{
				return NotFound("Categoria não encontrada!");
			}

			// Guarda o nome antigo
			string nomeCategoria = categoriaOriginal.Nome;

			// Aplica alteração
			categoriaOriginal.Nome = categoriaAtt.Nome;

			// Salva alterações no banco de dados
			_context.SaveChanges();

			return Ok(new
			{
				mensagem = $"Categoria {nomeCategoria} foi alterada para {categoriaOriginal.Nome} com sucesso!",
				dados = categoriaOriginal
			});			
		}
	}
}

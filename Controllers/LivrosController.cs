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
	public class LivrosController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly AuthServices _auth;

		public LivrosController(AppDbContext context, AuthServices auth)
		{
			_context = context;
			_auth = auth;
		}

		// Lista todos os livros
		[HttpGet]
		[ProducesResponseType(typeof(List<LivroExibicaoDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<List<LivroExibicaoDto>>> GetTodos([FromQuery] string? titulo, [FromQuery] string? autor)
		{
			var query = _context.Livros.AsQueryable();

			if (!string.IsNullOrWhiteSpace(titulo))
			{
				query = query.Where(l => l.Titulo.Contains(titulo));
			}

			if (!string.IsNullOrWhiteSpace(autor))
			{
				query = query.Where(l => l.Autor.Contains(autor));
			}

			var livros = await query
					.Select(l => new LivroExibicaoDto
					{
						Titulo = l.Titulo,
						Autor = l.Autor,
						CategoriaNome = l.Categoria != null ? l.Categoria.Nome : "Sem Categoria",
						Estoque = l.Estoque,
						Preco = l.Preco
					})
					.ToListAsync();

			if (livros.Count == 0)
			{
				return NotFound("Nenhum livro encontrado!");
			}

			return Ok(livros);
		}

		// Adiciona novo livro
		[HttpPost]
		public async Task<ActionResult> PostNovoLivro(LivroCriacaoDto livroDto, [FromHeader] int adminId)
		{
			// Verifica se usuário é admin
			if (!await _auth.EhAdmin(adminId)) return StatusCode(403, "Acesso negado! Apenas administradores podem adicionar livros!");

			// Se passar pela segurança, método segue normalmente
			// Transforma o DTO numa entidade Livro
			var novoLivro = new Livro
			{
				Titulo = livroDto.Titulo,
				Autor = livroDto.Autor,
				Estoque = livroDto.Estoque,
				Preco = livroDto.Preco,
				CategoriaId = livroDto.CategoriaId
			};

			_context.Livros.Add(novoLivro);
			await _context.SaveChangesAsync();

			return Ok(new { mensagem = "Livro adicionado com sucesso via DTO!", dados = novoLivro });
		}

		// Endpoint para deletar um Livro
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DeleteLivro(int id, [FromHeader] int adminId)
		{
			//verifica se usuário é admin
			if (!await _auth.EhAdmin(adminId))
			{
				return StatusCode(403, "Acesso negado! Apenas administradores podem remover livros!");
			}

			var delLivro = await _context.Livros.FirstOrDefaultAsync(l => l.Id == id);
			if (delLivro == null)
			{
				return NotFound("Livro não encontrado!");
			}
			_context.Livros.Remove(delLivro);
			await _context.SaveChangesAsync();
			return Ok(new { mensagem = "Livro removido com sucesso!", delLivro });
		}

		// Edita um livro após a ID do mesmo ser inserida
		[HttpPut("{id:int}")]
		public async Task<IActionResult> EditarLivro(int id, LivroAtualizacaoDto livroDto, [FromHeader] int adminId)
		{
			//verifica se usuário é admin
			if (!await _auth.EhAdmin(adminId))
			{
				return StatusCode(403, "Acesso negado! Apenas administradores podem editar livros!");
			}

			// Pega o ID do livro que foi inserido no swagger
			var livroOriginal = await _context.Livros.FirstOrDefaultAsync(l => l.Id == id);

			// Verifica se o livro com o ID inserido existe
			if (livroOriginal == null)
			{
				return NotFound(new { mensagem = $"Livro de ID {id} não encontrado!" });
			}
			// Se o ID existe, irá fazer DTO(Data Transfer Object) das seguintes propriedades:

			// Só muda o titulo se for inserido algo diferente de Nulo, Vazio ou "string"
			if (!string.IsNullOrWhiteSpace(livroDto.Titulo) && livroDto.Titulo != "string")
			{
				livroOriginal.Titulo = livroDto.Titulo;
			}

			// Só muda o autor se for inserido algo diferente de Nulo, Vazio ou "string"
			if (!string.IsNullOrWhiteSpace(livroDto.Autor) && livroDto.Autor != "string")
			{
				livroOriginal.Autor = livroDto.Autor;
			}

			// Só muda o estoque se o usuário inseriu um valor
			if (livroDto.Estoque.HasValue)
			{
				livroOriginal.Estoque = livroDto.Estoque.Value;
			}

			// Só muda o preço se o usuário inseriu um valor
			if (livroDto.Preco.HasValue)
			{
				livroOriginal.Preco = livroDto.Preco.Value;
			}

			// Só muda a categoria se o usuario inserir um valor
			if (livroDto.CategoriaId.HasValue)
			{
				// Verifica se a Id inserida pertence a alguma categoria existente
				var existeId = await _context.Categorias.AnyAsync(c => c.Id == livroDto.CategoriaId);
				if (!existeId)
				{
					return NotFound($"Erro: Categoria de Id {livroDto.CategoriaId} não encontrada!");
				}

				livroOriginal.CategoriaId = livroDto.CategoriaId.Value;
			}

			// Salva as mudanças no Banco de Dados
			await _context.SaveChangesAsync();
			return Ok(new { mensagem = $"Livro de ID {id} foi editado com sucesso!", dados = livroOriginal });
		}
	}
}

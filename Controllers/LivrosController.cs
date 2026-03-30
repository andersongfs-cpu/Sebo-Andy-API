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
	public class LivrosController : ControllerBase
	{
		private readonly AppDbContext _context;

		public LivrosController(AppDbContext context)
		{
			_context = context;
		}

		// Lista livros usando Querys
		[HttpGet]
		[ProducesResponseType(typeof(List<LivroExibicaoDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<List<LivroExibicaoDto>>> GetTodos(
		[FromQuery] int? id,
		[FromQuery] string? titulo,
		[FromQuery] string? autor,
		[FromQuery] int? categoriaId,
		[FromQuery] string? categoria)
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

			if (categoriaId.HasValue)
			{
				query = query.Where(l => l.CategoriaId == categoriaId);
			}

			if (!string.IsNullOrWhiteSpace(categoria))
			{
				query = query.Where(l => l.Categoria != null && l.Categoria.Nome.Contains(categoria));
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
				return Ok(new List<LivroExibicaoDto>());
			}

			return Ok(livros);
		}

		// Adiciona novo livro
		[Authorize(Roles = "Admin")]
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]     // Novo usuário criado
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]// Não fez login
		[ProducesResponseType(StatusCodes.Status403Forbidden)]   // Fez login, mas não tem autoridade pra criar novo usuário
		[ProducesResponseType(StatusCodes.Status404NotFound)]	 // Retorna não encontrado em caso de dado inserido correto, mas não localizado no BD
		public async Task<IActionResult> PostNovoLivro(LivroCriacaoDto livroDto)
		{
			// Verifica se a ID existe em Categorias
			var existeCategoria = await _context.Categorias.FindAsync(livroDto.CategoriaId);
			// Se não existe, retorna 404
			if (existeCategoria == null)
			{
				return NotFound("Categoria não existe.");
			}
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

			var exibicaoDto = new LivroExibicaoDto
			{
				Titulo = novoLivro.Titulo,
				Autor = novoLivro.Autor,
				Estoque = novoLivro.Estoque,
				Preco = novoLivro.Preco,
				CategoriaNome = existeCategoria.Nome
			};

			return CreatedAtAction(
				nameof(GetTodos),
				new { titulo = exibicaoDto.Titulo },
				 exibicaoDto);
		}

		// Deleta livro através de uma ID
		[Authorize(Roles = "Admin")]
		[HttpDelete("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteLivro(int id)
		{
			var delLivro = await _context.Livros.FindAsync(id);
			if (delLivro == null)
			{
				return NotFound("Livro não encontrado!");
			}

			_context.Livros.Remove(delLivro);
			await _context.SaveChangesAsync();
			return NoContent();
		}

		// Edita livro através de uma ID
		[Authorize(Roles = "Admin,Funcionario")]
		[HttpPut("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> EditarLivro(int id, LivroAtualizacaoDto livroDto)
		{
			// Pega o ID do livro que foi inserido no swagger
			var livroOriginal = await _context.Livros.FindAsync(id);

			// Verifica se o livro com o ID inserido existe
			if (livroOriginal == null)
			{
				return NotFound("Id não encontrada.");
			}
			// Se o ID existe, irá fazer DTO(Data Transfer Object) das seguintes propriedades:

			// Só muda o titulo se for inserido algo diferente de Nulo ou Vazio
			if (!string.IsNullOrWhiteSpace(livroDto.Titulo))
			{
				livroOriginal.Titulo = livroDto.Titulo;
			}

			// Só muda o autor se for inserido algo diferente de Nulo ou Vazio
			if (!string.IsNullOrWhiteSpace(livroDto.Autor))
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

			// Só muda a categoria se o usuário inseriu um valor
			if (livroDto.CategoriaId.HasValue)
			{
				// Verifica se a Id inserida pertence a alguma categoria existente
				var existeId = await _context.Categorias.AnyAsync(c => c.Id == livroDto.CategoriaId);
				if (!existeId)
				{
					return NotFound("Categoria não encontrada.");
				}

				livroOriginal.CategoriaId = livroDto.CategoriaId.Value;
			}

			// Salva as mudanças no Banco de Dados
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}

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
		public async Task<ActionResult<List<LivroExibicaoDto>>> GetTodos()
		{
			var livros = await _context.Livros
					.Select(l => new LivroExibicaoDto
					{
						Titulo = l.Titulo,
						Autor = l.Autor,
						CategoriaNome = l.Categoria != null ? l.Categoria.Nome : "Sem Categoria",
						Estoque = l.Estoque,
						Preco = l.Preco
					})
					.ToListAsync();

			return Ok(livros);
		}

		//Procura livros por nome do título
		[HttpGet("pesquisar-livro/{titulo}")]		
		public async Task<ActionResult<List<LivroExibicaoDto>>> GetPorNome(string titulo)
		{
			var livro = await _context.Livros
				.Where(l => l.Titulo.ToLower().Contains(titulo.ToLower()))
				.Select(l => new LivroExibicaoDto
				{
					Titulo = l.Titulo,
					Autor = l.Autor,
					Estoque = l.Estoque,
					Preco = l.Preco,
					CategoriaNome = l.Categoria != null ? l.Categoria.Nome : "Sem Categoria"
				})
				.ToListAsync();

			if (livro.Count == 0)
			{
				return NotFound($"Livro com titulo {titulo} não encontrado!");
			}

			return Ok(livro);
		}

		// Endpoint para pegar apenas livros acima do valor inserido
		[HttpGet("preco-acima/{preco:decimal}")]
		public async Task<ActionResult<List<LivroExibicaoDto>>> GetPorMaiorValor(decimal preco)
		{
			var livros = await _context.Livros.Where(l => l.Preco >= preco).ToListAsync();
			if (livros.Count == 0)
			{
				return NotFound($"Não há livros de valor igual ou acima de {preco:C2}");
			}

			var livrosDto = livros
				.Select(l => new LivroExibicaoDto
				{
					Titulo = l.Titulo,
					Autor = l.Autor,
					Estoque = l.Estoque,
					Preco = l.Preco,
					CategoriaNome = l.Categoria != null ? l.Categoria.Nome : "Sem categoria."
				})
				.ToList();

			return Ok(new { mensagem = "Livro(s) Encontrado(s)!", livrosDto });
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
			if (!string.IsNullOrWhiteSpace(livroDto.Titulo) && livroDto.Titulo != "string" )
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

		// Mostra um DTO de livros que pertencem a categoria que for digitada
		[HttpGet("pesquisar-categoria/{categoria}")]
		public async Task<ActionResult> GetPorNomeCategoria(string categoria)
		{			
			var resultadoBusca = await _context.Livros
				.Where(c => c.Categoria!.Nome.ToLower().Contains(categoria.ToLower()))
				.Select(l => new LivroExibicaoDto{ 
					CategoriaNome = l.Categoria != null ? l.Categoria.Nome : "Sem categoria!",
					Titulo = l.Titulo,
					Autor = l.Autor,
					Estoque = l.Estoque,
					Preco = l.Preco})
				.ToListAsync();

			if (resultadoBusca.Count == 0)
			{
				return NotFound($"Nenhum livro encontrado na categoria '{categoria}'.");
			}
			return Ok(resultadoBusca);
		}
	}
}

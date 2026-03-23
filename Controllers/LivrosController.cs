using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sebo_Andy.Data;
using Sebo_Andy.DTOs;
using Sebo_Andy.Models;
using Sebo_Andy.Services;
using System.Linq;

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
		public ActionResult<List<LivroExibicaoDto>> GetTodos()
		{
			var livros = _context.Livros
					.Select(l => new LivroExibicaoDto
					{
						Titulo = l.Titulo,
						Autor = l.Autor,
						CategoriaNome = l.Categoria?.Nome,
						Estoque = l.Estoque,
						Preco = l.Preco
					})
					.ToList();

			return Ok(livros);
		}

		//Procura livros por nome do título
		[HttpGet("pesquisar-livro/{titulo}")]		
		public ActionResult<List<LivroExibicaoDto>> GetPorNome(string titulo)
		{
			var livro = _context.Livros
				.Where(l => l.Titulo.ToLower().Contains(titulo.ToLower()))
				.Select(l => new LivroExibicaoDto
				{
					Titulo = l.Titulo,
					Autor = l.Autor,
					Estoque = l.Estoque,
					Preco = l.Preco,
					CategoriaNome = l.Categoria.Nome
				})
				.ToList();

			if (livro.Count == 0)
			{
				return NotFound($"Livro com titulo {titulo} não encontrado!");
			}

			return Ok(livro);
		}

		[HttpGet("preco-acima/{preco:decimal}")]
		public ActionResult<List<Livro>> GetPorMaiorValor(decimal preco)
		{
			var livros = _context.Livros.Where(l => l.Preco >= preco).ToList();
			if (livros.Count == 0)
			{
				return NotFound($"Não há livros de valor igual ou acima de {preco:C2}");
			}
			return Ok(new { mensagem = "Livro(s) Encontrado(s)!", livros });
		}

		// Adiciona novo livro
		[HttpPost]		
		public ActionResult PostNovoLivro(LivroCriacaoDto livroDto, [FromHeader] int adminId)
		{
			// Verifica se usuário é admin
			if (!_auth.EhAdmin(adminId)) return StatusCode(403, "Acesso negado! Apenas administradores podem adicionar livros!");

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
			_context.SaveChanges();

			return Ok(new { mensagem = "Livro adicionado com sucesso via DTO!", dados = novoLivro });
		}

		[HttpDelete("{id:int}")]
		public IActionResult DeleteLivro(int id, [FromHeader] int adminId)
		{
			//verifica se usuário é admin
			if (!_auth.EhAdmin(adminId))
			{
				return StatusCode(403, "Acesso negado! Apenas administradores podem remover livros!");
			}

			var delLivro = _context.Livros.FirstOrDefault(l => l.Id == id);
			if (delLivro == null)
			{
				return NotFound("Livro não encontrado!");
			}
			_context.Livros.Remove(delLivro);
			_context.SaveChanges();
			return Ok(new { mensagem = "Livro removido com sucesso!", delLivro });
		}

		// Edita um livro após a ID do mesmo ser inserida
		[HttpPut("{id:int}")]
		public IActionResult EditarLivro(int id, LivroAtualizacaoDto livroDto, [FromHeader] int adminId)
		{
			//verifica se usuário é admin
			if (!_auth.EhAdmin(adminId))
			{
				return StatusCode(403, "Acesso negado! Apenas administradores podem editar livros!");
			}

			// Pega o ID do livro que foi inserido no swagger
			var livroOriginal = _context.Livros.FirstOrDefault(l => l.Id == id);

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
				var existeId = _context.Categorias.Any(c => c.Id == livroDto.CategoriaId);
				if (!existeId)
				{
					return NotFound($"Erro: Categoria de Id {livroDto.CategoriaId} não encontrada!");
				}

				livroOriginal.CategoriaId = livroDto.CategoriaId.Value;
			}

			// Salva as mudanças no Banco de Dados
			_context.SaveChanges();
			return Ok(new { mensagem = $"Livro de ID {id} foi editado com sucesso!", dados = livroOriginal });
		}

		// Faz uma busca por livros da categoria inserida
		[HttpGet("buscar-categoria/{id:int}")]
		public ActionResult<List<Livro>> GetPorCategoria(int id)
		{
			// Verifica se a categoria existe
			var existeCategoria = _context.Categorias.FirstOrDefault(c => c.Id == id);
			if(existeCategoria == null)
			{
				return NotFound($"Categoria de Id {id} não existe!");
			}
			// Verifica se existem livros daquela categoria no Banco de Dados
			var livros = _context.Livros
						.Include(l => l.Categoria)
						.Where(l => l.CategoriaId == id)
						.ToList();
			
			// Se categoria existe, verifica se existem livros que pertencem a ela
			if(livros.Count == 0)
			{				
				return Ok(new { mensagem = "Erro[200] Não existem livros pertencentes a categoria selecionada.", dados = existeCategoria });
			}

			return Ok(new{
			mensagem = "Livro(s) Encontrado(s): ",
			dados = livros
			});
		}

		// Mostra um DTO de livros que pertencem a categoria que for digitada
		[HttpGet("pesquisar-categoria/{categoria}")]
		public ActionResult GetPorNomeCategoria(string categoria)
		{			
			var resultadoBusca = _context.Livros
				.Where(c => c.Categoria.Nome.ToLower().Contains(categoria.ToLower()))
				.Select(l => new LivroExibicaoDto{ 
					CategoriaNome = l.Categoria.Nome,
					Titulo = l.Titulo,
					Autor = l.Autor,
					Estoque = l.Estoque,
					Preco = l.Preco})
				.ToList();

			if (resultadoBusca.Count == 0)
			{
				return NotFound($"Nenhum livro encontrado na categoria '{categoria}'.");
			}
			return Ok(resultadoBusca);
		}
	}
}

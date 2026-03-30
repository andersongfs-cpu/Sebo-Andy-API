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
	public class CategoriasController : ControllerBase
	{
		private readonly AppDbContext _context;
		public CategoriasController(AppDbContext context)
		{
			_context = context;
		}

		// Método para listar as categorias de livros cadastradas.
		[HttpGet]
		[ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]		
		public async Task<ActionResult<List<CategoriaDto>>> GetTodos([FromQuery] int? id, [FromQuery] string? nome)
		{
			var query = _context.Categorias.AsQueryable();

			if (id.HasValue)
			{
				query = query.Where(c => c.Id == id);
			}
			if (!string.IsNullOrWhiteSpace(nome))
			{
				query = query.Where(c => c.Nome.Contains(nome));
			}

			var categorias = await query
			.Select(c => new CategoriaDto
			{
				Nome = c.Nome
			})
			.ToListAsync();

			if (categorias.Count == 0)
			{
				return Ok(new List<CategoriaDto>());
			}

			return Ok(categorias);
		}

		// Método para adicionar uma nova categoria
		[Authorize(Roles = "Admin")]
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> PostNovaCategoria(CategoriaDto categoriaDto)
		{
			var novaCategoria = new Categoria
			{
				Nome = categoriaDto.Nome
			};

			_context.Categorias.Add(novaCategoria);
			await _context.SaveChangesAsync();

			return Created(string.Empty, novaCategoria);
		}

		// Endpoint para remover categoria via ID
		[Authorize(Roles = "Admin")]
		[HttpDelete("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteCategoria(int id)
		{
			var delCategoria = await _context.Categorias.FindAsync(id);
			if (delCategoria == null)
			{
				return NotFound("Categoria não encontrada!");
			}
			
			_context.Categorias.Remove(delCategoria);
			await _context.SaveChangesAsync();
			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("{id:int}")]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> EditarCategoria(int id, CategoriaDto categoriaAtt)
		{
			var categoriaOriginal = await _context.Categorias.FindAsync(id);
			if (categoriaOriginal == null)
			{
				return NotFound("Categoria não encontrada!");
			}

			if (!string.IsNullOrWhiteSpace(categoriaAtt.Nome))
			{
				categoriaOriginal.Nome = categoriaAtt.Nome;
			}

			await _context.SaveChangesAsync();
			return NoContent();			
		}
	}
}

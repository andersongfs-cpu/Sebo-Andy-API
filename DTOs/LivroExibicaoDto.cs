using Sebo_Andy.Models;

namespace Sebo_Andy.DTOs
{
	public class LivroExibicaoDto
	{
		public string? Titulo { get; set; } = string.Empty;
		public string? Autor { get; set; } = string.Empty;
		public int? Estoque { get; set; }
		public decimal? Preco { get; set; }
		public string? CategoriaNome { get; set; } = string.Empty;
	}
}

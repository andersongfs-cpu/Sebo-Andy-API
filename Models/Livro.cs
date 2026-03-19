namespace Sebo_Andy.Models
{
	public class Livro
	{
		public int Id { get; set; }
		public string? Titulo { get; set; } = string.Empty;
		public string? Autor { get; set; } = string.Empty;
		public int? Estoque { get; set; }
		public decimal? Preco { get; set; }
		public int? CategoriaId { get; set; }
		public Categoria? Categoria { get; set; }
	}
}

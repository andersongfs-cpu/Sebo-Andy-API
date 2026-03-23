namespace Sebo_Andy.DTOs
{
	public class LivroCriacaoDto
	{
		public string Titulo { get; set; } = string.Empty;
		public string Autor { get; set; } = string.Empty;
		public int? Estoque { get; set; }
		public decimal? Preco { get; set; }
		public int? CategoriaId { get; set; }
	}
}

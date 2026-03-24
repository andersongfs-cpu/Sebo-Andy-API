using Sebo_Andy.Models;

namespace Sebo_Andy.DTOs
{
	public class UsuarioPostDto
	{
		
		public string Nome { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Senha { get; set; } = string.Empty;
		public TipoCargo Cargo { get; set; } = TipoCargo.Cliente;
	}
}

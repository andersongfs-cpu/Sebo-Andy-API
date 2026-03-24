using System.ComponentModel.DataAnnotations;

namespace Sebo_Andy.Models
{
	public enum TipoCargo
	{
		Cliente,     // enum = 0 //Padrão para cargo com menos poder sob o sistema(Padrão Segurança)
		Funcionario, // enum = 1
		Admin        // enum = 2
	}


	public class Usuario
	{
		public int Id { get; set; }
		public string Nome { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Senha { get; set; } = string.Empty;
		public DateTime DataCadastro { get; set; } = DateTime.Now;		
		public TipoCargo Cargo { get; set; } = TipoCargo.Cliente;
	}
}

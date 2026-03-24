using Sebo_Andy.Migrations;

namespace Sebo_Andy.DTOs
{
	public class LoginDto
	{
		public string Email { get; set; } = string.Empty;
		public string Senha { get; set; } = string.Empty;
	}
}

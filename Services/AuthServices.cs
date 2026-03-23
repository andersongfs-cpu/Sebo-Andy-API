using Sebo_Andy.Data;
using Sebo_Andy.Models;

namespace Sebo_Andy.Services
	
{
	public class AuthServices
	{
		private readonly AppDbContext _context;

		public AuthServices(AppDbContext context)
		{
			_context = context;
		}

		//Método que verifica se o usuário é admin
		public async Task<bool> EhAdmin(int usuarioId)
		{
			var usuario = await _context.Usuarios.FindAsync(usuarioId);
			return usuario != null && usuario.Cargo == TipoCargo.Admin;
		}
	}
}

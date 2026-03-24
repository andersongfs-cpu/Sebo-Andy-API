using Microsoft.IdentityModel.Tokens;
using Sebo_Andy.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sebo_Andy.Services
{
	public static class TokenService
	{
		public static string GerarToken(Usuario usuario)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			
			var key = Encoding.ASCII.GetBytes("shVp1ufAgFLJlVvxLuUqvJ9Z7yTUo6doODtcj58YDXTbbp34ir9eVUhtCvpICM1U");

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[]
				{
					new Claim(ClaimTypes.Name, usuario.Nome),
					new Claim(ClaimTypes.Email, usuario.Email),
					new Claim(ClaimTypes.Role, usuario.Cargo.ToString())
				}),
				Expires = DateTime.UtcNow.AddHours(2), // Token se torna inválido após 2 horas
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
	}
}

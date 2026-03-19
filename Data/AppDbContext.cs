using Microsoft.EntityFrameworkCore;
using Sebo_Andy.Models;

namespace Sebo_Andy.Data
{
	// O DbContext é o tradutor entre suas classes C# e as tabelas do SQL
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}
		//tabelas criadas com base nas classes(Livro e Usuario)
		public DbSet<Livro> Livros { get; set; }

		public DbSet<Usuario> Usuarios { get; set; }

		public DbSet<Categoria> Categorias { get; set; }

		//Diz quantos digitos antes e após a vírgula o SQL irá aceitar na variavel Preco(preço)
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Livro>()
				.Property(l => l.Preco)
				.HasColumnType("decimal(18,2)");

			base.OnModelCreating(modelBuilder);
		}
	}
}

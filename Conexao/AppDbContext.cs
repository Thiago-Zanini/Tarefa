using Microsoft.EntityFrameworkCore;
using Tarefa.Tabela;

namespace Tarefa.Conexao
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options){ }

        public DbSet<TarefaModel> tarefas { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tarefa.Tabela
{
    [Table("Tarefa")]
    public class TarefaModel
    {
        [Key]
        public int TarefaId { get; set; }

        [Required(ErrorMessage = "Nome da Tarefa é obrigatório")]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "Nome do Titulo é obrigatório")]
        public required string Titulo { get; set; }
        public DateTime Data { get; private set; } = DateTime.UtcNow;

        public bool Concluida { get; set; } = false;

        public void ConcluirTarefa()
        {
            if(!Concluida) Concluida = true;
        }

        public void TarefaPendente()
        {
            if(Concluida) Concluida = false;
        }
    }
}

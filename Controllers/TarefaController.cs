using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using Tarefa.Conexao;
using Tarefa.Tabela;

namespace Tarefa.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Tarefa")]
    public class TarefaController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly ILogger<TarefaController> logger;
        private readonly IMemoryCache memoryCache;

        private const string FazerCacheKey = nameof(FazerCacheKey);
        private const string ConcluidaCacheKey = nameof(ConcluidaCacheKey);
        private const string TodasCacheKey = nameof(TodasCacheKey);

        public TarefaController(AppDbContext context, ILogger<TarefaController> logger, IMemoryCache memoryCache) 
        {
            this.context = context;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        private void RemoveCache()
        {
            memoryCache.Remove(FazerCacheKey);
            memoryCache.Remove(ConcluidaCacheKey);
            memoryCache.Remove(TodasCacheKey);
        }

        [HttpPost("Adicionar")]
        public async Task<IActionResult> Add([FromBody] TarefaModel tarefas)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var existe = await context.tarefas.AnyAsync(p => p.Nome == tarefas.Nome);

                if (existe) return Conflict(new { message = "Já existe essa tarefa" });

                await context.tarefas.AddAsync(tarefas);
                await context.SaveChangesAsync();

                RemoveCache();

                return Ok(new { message = "Tarefa adicionada com sucesso" });
            }
            catch (Exception ex)
            { 
                logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Concluida")]
        public async Task<IActionResult> TarefaConcluida([FromBody] Concluir Nome)
        {
            try
            {
                var tarefa = await context.tarefas.FirstOrDefaultAsync(p => p.Nome == Nome.Nome);

                if (tarefa == null) return NotFound();
                if(tarefa.Concluida) return Ok(new {message = "essa tarefa já está concluida"});

                tarefa.ConcluirTarefa();
                await context.SaveChangesAsync();

                RemoveCache();

                return Ok(new { message = "Tarefa concluida com sucesso" });
            }catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Pendente")]
        public async Task<IActionResult> TarefaConcluidaFalse([FromBody] Concluir Nome)
        {
            try
            {
                var tarefa = await context.tarefas.FirstOrDefaultAsync(p => p.Nome == Nome.Nome);

                if(tarefa == null) return NotFound();
                if(!tarefa.Concluida) return Ok(new {message = "já está como pendente"});

                tarefa.TarefaPendente();
                await context.SaveChangesAsync();

                RemoveCache();

                return Ok(new {message = "Tarefa colocada como pendente"});
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Fazer")]
        public async Task<IActionResult> Tarefas()
        {
            try
            { 

                if (!memoryCache.TryGetValue(FazerCacheKey, out List<TarefaDtos>? cachedData))
                {
                  var fazer = await context.tarefas.AsNoTracking().Where(p => p.Concluida == false)
                  .Select(p => new TarefaDtos
                  {
                    Nome = p.Nome,
                    Titulo = p.Titulo,
                    Concluida = p.Concluida,
                  })
                  .ToListAsync();

                    memoryCache.Set(FazerCacheKey, fazer, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });

                    cachedData = fazer;

                }

                return Ok(cachedData);
            }catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Concluida")]
        public async Task<IActionResult> Concluida()
        {
            try
            {
                if (!memoryCache.TryGetValue(ConcluidaCacheKey, out List<TarefaDtos>? tarefa))
                {
                    var concluidas = await context.tarefas.AsNoTracking().Where(p => p.Concluida == true)
                        .Select(p => new TarefaDtos
                        {
                            Nome = p.Nome,
                            Titulo = p.Titulo,
                            Concluida = p.Concluida
                        }).ToListAsync();

                    memoryCache.Set(ConcluidaCacheKey, concluidas, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                    
                    tarefa = concluidas;
                }

                return Ok(tarefa);
            }catch(Exception ex)
            {
                logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Todas")]
        public async Task<IActionResult> Todas()
        {
            try
            {
                if (!memoryCache.TryGetValue(TodasCacheKey, out List<TarefaDtos>? tarefas))
                {
                    var todas = await context.tarefas.AsNoTracking().Select(p => new TarefaDtos
                    {
                        Nome = p.Nome,
                        Titulo = p.Titulo,
                        Concluida = p.Concluida
                    }).ToListAsync();

                    memoryCache.Set(TodasCacheKey, todas, new MemoryCacheEntryOptions 
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)});
                    
                    tarefas = todas;
                }

                return Ok(tarefas);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}

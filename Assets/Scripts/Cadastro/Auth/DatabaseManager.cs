using UnityEngine;
using SQLite;
using System.IO;

/// <summary>
/// Gerencia a conexao com o banco de dados SQLite do Perielio.
/// Singleton — use DatabaseManager.Instancia para acessar de qualquer script.
/// Cria automaticamente o arquivo .db e as tabelas ao iniciar o jogo.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────
    public static DatabaseManager Instancia { get; private set; }

    // ─── CONEXAO ─────────────────────────────────────────────────────────────
    private SQLiteConnection db;
    private const string NOME_BANCO = "perielio.db";

    // ─── PROPRIEDADE PUBLICA ─────────────────────────────────────────────────
    public SQLiteConnection Conexao => db;

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
        InicializarBanco();
    }

    // ════════════════════════════════════════════════════════════════════════
    // INICIALIZACAO
    // ════════════════════════════════════════════════════════════════════════

    private void InicializarBanco()
    {
        string caminho = Path.Combine(Application.persistentDataPath, NOME_BANCO);
        Debug.Log($"[Database] Banco de dados em: {caminho}");

        db = new SQLiteConnection(caminho);
        db.CreateTable<TabelaUsuario>();
        db.CreateTable<TabelaProgresso>();

        Debug.Log("[Database] Tabelas criadas/verificadas com sucesso!");
    }

    private void OnApplicationQuit()
    {
        if (db != null)
        {
            db.Close();
            Debug.Log("[Database] Conexao encerrada.");
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // METODOS DE PROGRESSO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Salva ou atualiza o progresso de uma fase.
    /// Avaliacao binaria: concluida = 1, nao concluida = 0.
    /// Nunca regride — se ja estava concluida, mantem (RN05).
    /// </summary>
    public void SalvarProgresso(int usuarioId, int mundo, int fase, int tentativas)
    {
        var existente = db.Table<TabelaProgresso>()
                          .Where(p => p.UsuarioId == usuarioId
                                   && p.Mundo == mundo
                                   && p.Fase == fase)
                          .FirstOrDefault();

        if (existente == null)
        {
            db.Insert(new TabelaProgresso
            {
                UsuarioId     = usuarioId,
                Mundo         = mundo,
                Fase          = fase,
                Concluida     = 1,
                Tentativas    = tentativas,
                DataConclusao = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            });

            Debug.Log($"[Database] Progresso salvo: Mundo {mundo} Fase {fase} em {tentativas} tentativas.");
        }
        else if (existente.Concluida == 0)
        {
            existente.Concluida     = 1;
            existente.Tentativas    = tentativas;
            existente.DataConclusao = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            db.Update(existente);

            Debug.Log($"[Database] Progresso atualizado: Mundo {mundo} Fase {fase}.");
        }
    }
}

// ════════════════════════════════════════════════════════════════════════
// MODELOS DAS TABELAS
// ════════════════════════════════════════════════════════════════════════

[Table("Usuario")]
public class TabelaUsuario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } 

    [NotNull, Unique]
    public string Nome { get; set; }

    [NotNull]
    public string Senha { get; set; }

    [NotNull]
    public string Perfil { get; set; }

    public string DataCadastro { get; set; }
}

[Table("Progresso")]
public class TabelaProgresso
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public int UsuarioId { get; set; }

    [NotNull]
    public int Mundo { get; set; }

    [NotNull]
    public int Fase { get; set; }

    public int Concluida { get; set; }

    public int Tentativas { get; set; }

    public string DataConclusao { get; set; }
}

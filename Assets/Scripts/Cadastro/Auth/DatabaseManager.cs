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
        // Singleton persistente entre cenas
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
    // INICIALIZAÇÃO
    // ════════════════════════════════════════════════════════════════════════

    private void InicializarBanco()
    {
        string caminho = Path.Combine(Application.persistentDataPath, NOME_BANCO);
        Debug.Log($"[Database] Banco de dados em: {caminho}");

        db = new SQLiteConnection(caminho);

        // Cria as tabelas se ainda nao existirem
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
}

// ════════════════════════════════════════════════════════════════════════
// MODELOS DAS TABELAS
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// Tabela de usuarios — alunos e professores cadastrados.
/// RF01 — Cadastro de Perfil.
/// </summary>
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
    public string Perfil { get; set; }  // "aluno" ou "professor"

    public string DataCadastro { get; set; }
}

/// <summary>
/// Tabela de progresso por fase — vinculada ao usuario.
/// RF10 — Avaliacao por Estrelas.
/// RF08 — Desbloqueio Automatico de Fase.
/// </summary>
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

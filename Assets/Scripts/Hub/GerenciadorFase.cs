using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Gerencia o estado de cada fase do jogo Perielio.
/// Controla tentativas, conclusao, feedback pedagogico e navegacao.
/// Avaliacao binaria: a fase e concluida ao alcancar o destino (RN04).
/// Implementa RN04, RN05, RN07, RN09, RN10, RN12, RN13.
/// </summary>
public class GerenciadorFase : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────
    public static GerenciadorFase Instancia { get; private set; }

    // ─── DADOS DA FASE ───────────────────────────────────────────────────────
    [Header("Dados da Fase")]
    [SerializeField] private string nomeFase = "Fase 1.1";
    [SerializeField] private int indiceMundo = 1;
    [SerializeField] private int indiceFase = 1;

    [Header("Dica Pedagogica (RN09)")]
    [SerializeField][TextArea(2, 5)]
    private string dicaConceito = "Quanto maior a massa do foguete, maior a forca gravitacional que os planetas exercem sobre ele!";
    [SerializeField] private string conceitoFisico = "Lei da Gravitacao Universal";

    [Header("Navegacao")]
    [SerializeField] private string cenaProximaFase = "Fase1.2";
    [SerializeField] private string cenaMenu = "HubFasesMundo01";

    // ─── UI ──────────────────────────────────────────────────────────────────
    [Header("UI de Gameplay")]
    [SerializeField] private TextMeshProUGUI txtNomeFase;
    [SerializeField] private TextMeshProUGUI txtTentativas;

    [Header("Painel de Resultado")]
    [SerializeField] private GameObject painelResultado;
    [SerializeField] private TextMeshProUGUI txtResultadoTitulo;
    [SerializeField] private TextMeshProUGUI txtDicaPedagogica;
    [SerializeField] private TextMeshProUGUI txtConceitoFisico;
    [SerializeField] private Button btnProximaFase;
    [SerializeField] private Button btnRepetir;
    [SerializeField] private Button btnMenu;

    [Header("Painel de Colisao")]
    [SerializeField] private GameObject painelColisao;
    [SerializeField] private TextMeshProUGUI txtMensagemColisao;
    [SerializeField] private float tempoExibicaoColisao = 2.5f;

    // ─── ESTADO ──────────────────────────────────────────────────────────────
    private int tentativasUsadas = 0;
    private bool faseConcluida = false;

    // ─── REFERENCIAS ─────────────────────────────────────────────────────────
    private FogueteController foguete;

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    private void Start()
    {
        foguete = FindFirstObjectByType<FogueteController>();

        if (painelResultado != null) painelResultado.SetActive(false);
        if (painelColisao != null) painelColisao.SetActive(false);

        AtualizarUI();
    }

    // ════════════════════════════════════════════════════════════════════════
    // EVENTOS DE JOGO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Chamado pelo FogueteController ao colidir com planeta ou buraco negro (RN07).
    /// </summary>
    public void RegistrarColisao()
    {
        if (faseConcluida) return;

        tentativasUsadas++;
        AtualizarUI();

        string mensagem = tentativasUsadas == 1
            ? "A forca gravitacional desviou seu foguete! Tente ajustar a massa."
            : $"Tentativa {tentativasUsadas}. Observe a massa e ângulo tentados anteriormente e ajuste!";

        StartCoroutine(ExibirFeedbackColisao(mensagem));
    }

    /// <summary>
    /// Chamado pelo FogueteController ao atingir o destino (RF04, RN04).
    /// </summary>
    public void RegistrarChegada()
    {
        if (faseConcluida) return;

        faseConcluida = true;
        tentativasUsadas++;

        SalvarProgresso();

        StartCoroutine(ExibirResultado());
    }

    // ════════════════════════════════════════════════════════════════════════
    // PERSISTENCIA (RN04, RN05, RF08)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Grava a conclusao da fase. Avaliacao binaria: chegou = concluida.
    /// Escreve as MESMAS chaves que os hubs leem para desbloquear fase e mundo.
    /// Nunca regride um progresso ja conquistado (RN05).
    /// </summary>
    private void SalvarProgresso()
    {
        // ── Progresso por aluno: desbloqueia fase, mundo e glossario ──
        ProgressoAluno.SalvarConclusao(indiceMundo, indiceFase, tentativasUsadas);

        // ── Espelha no SQLite, quando disponivel ──
        try
        {
            if (DatabaseManager.Instancia != null &&
                DatabaseManager.Instancia.Conexao != null &&
                SessaoManager.Instancia != null &&
                SessaoManager.Instancia.UsuarioAtual != null)
            {
                DatabaseManager.Instancia.SalvarProgresso(
                    SessaoManager.Instancia.UsuarioAtual.Id,
                    indiceMundo,
                    indiceFase,
                    tentativasUsadas);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Fase] SQLite indisponivel ({e.Message}). Progresso salvo em PlayerPrefs.");
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // UI E FEEDBACK
    // ════════════════════════════════════════════════════════════════════════

    private void AtualizarUI()
    {
        if (txtNomeFase)   txtNomeFase.text   = nomeFase;
        if (txtTentativas) txtTentativas.text = $"Tentativas: {tentativasUsadas}";
    }

    private IEnumerator ExibirFeedbackColisao(string mensagem)
    {
        if (painelColisao != null) painelColisao.SetActive(true);
        if (txtMensagemColisao != null) txtMensagemColisao.text = mensagem;

        yield return new WaitForSeconds(tempoExibicaoColisao);

        if (painelColisao != null) painelColisao.SetActive(false);

        // Reinicia o foguete (RN12 - tentativas ilimitadas)
        if (foguete != null) foguete.Reiniciar();
    }

    private IEnumerator ExibirResultado()
    {
        yield return new WaitForSeconds(0.5f);

        if (painelResultado != null) painelResultado.SetActive(true);

        if (txtResultadoTitulo != null)
            txtResultadoTitulo.text = tentativasUsadas == 1
                ? "Perfeito! Rota certeira!"
                : "Fase concluida!";

        if (txtDicaPedagogica != null) txtDicaPedagogica.text = dicaConceito;
        if (txtConceitoFisico != null) txtConceitoFisico.text = $"Conceito aprendido: {conceitoFisico}";
    }

    // ════════════════════════════════════════════════════════════════════════
    // BOTOES DO PAINEL DE RESULTADO
    // ════════════════════════════════════════════════════════════════════════

    public void BtnProximaFase()
    {
        if (string.IsNullOrEmpty(cenaProximaFase))
        {
            Debug.LogWarning("[Fase] 'Cena Proxima Fase' vazia no Inspector. Voltando ao menu.");
            BtnMenu();
            return;
        }
        SceneManager.LoadScene(cenaProximaFase);
    }

    public void BtnRepetir()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BtnMenu()
    {
        SceneManager.LoadScene(cenaMenu);
    }

    // ════════════════════════════════════════════════════════════════════════
    // UTILITARIO DE TESTE
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Apaga todo o progresso salvo. Util para testar o fluxo do zero.
    /// Clique com o botao direito no componente -> "Limpar Progresso Salvo".
    /// </summary>
    [ContextMenu("Limpar Progresso de TODOS os Alunos")]
    private void LimparProgressoSalvo()
    {
        ProgressoAluno.LimparTudo();
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia o Hub de Fases de um mundo especifico.
/// Exibe 5 botoes de fase com desbloqueio sequencial.
/// Uma fase so desbloqueia se a anterior foi concluida (RF07, RF08).
/// Avaliacao binaria: concluida (chegou ao destino) ou nao concluida.
/// </summary>
public class HubFasesController : MonoBehaviour
{
    // ─── CONFIGURACAO DO MUNDO ───────────────────────────────────────────────
    [Header("Configuracao do Mundo")]
    [SerializeField] private int numeroMundo = 1;
    [SerializeField] private string nomeMundo = "Sistema Solar";

    // ─── BOTOES DE FASE ──────────────────────────────────────────────────────
    [System.Serializable]
    public class BotaoFase
    {
        public Button            btn;
        public TextMeshProUGUI   txtNumero;     // "Fase 1"
        public Image             imgStatus;     // verde=concluida, cinza=nao concluida
        public GameObject        iconeCadeado;  // aparece se bloqueada
        public Color             corConcluida   = new Color(0.2f, 0.7f, 0.3f);
        public Color             corDisponivel  = new Color(0.2f, 0.5f, 0.9f);
        public Color             corBloqueada   = new Color(0.3f, 0.3f, 0.3f);
    }

    [Header("Botoes das 5 Fases")]
    [SerializeField] private BotaoFase[] fases = new BotaoFase[5];

    [Header("Nomes das Cenas de Cada Fase")]
    [SerializeField] private string[] cenasFases = new string[5];

    [Header("UI Geral")]
    [SerializeField] private TextMeshProUGUI txtNomeMundo;
    [SerializeField] private TextMeshProUGUI txtProgresso;
    [SerializeField] private Button          btnVoltar;

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        if (txtNomeMundo != null)
            txtNomeMundo.text = $"Mundo {numeroMundo} — {nomeMundo}";

        if (btnVoltar != null)
            btnVoltar.onClick.AddListener(Voltar);

        AtualizarFases();
    }

    // ════════════════════════════════════════════════════════════════════════
    // LOGICA DE DESBLOQUEIO (RF07, RF08)
    // ════════════════════════════════════════════════════════════════════════

    private void AtualizarFases()
    {
        int totalConcluidas = 0;

        for (int i = 0; i < fases.Length; i++)
        {
            if (fases[i] == null || fases[i].btn == null) continue;

            int numeroFase = i + 1;
            bool concluida    = FaseConcluida(numeroMundo, numeroFase);
            bool desbloqueada = numeroFase == 1 || FaseConcluida(numeroMundo, numeroFase - 1);

            if (concluida) totalConcluidas++;

            ConfigurarBotaoFase(fases[i], numeroFase, concluida, desbloqueada);
        }

        // Atualiza progresso
        if (txtProgresso != null)
            txtProgresso.text = $"{totalConcluidas} / {fases.Length} concluidas";
    }

    private bool FaseConcluida(int mundo, int fase)
    {
        // Tenta SQLite
        if (DatabaseManager.Instancia != null && SessaoManager.Instancia?.UsuarioAtual != null)
        {
            int usuarioId = SessaoManager.Instancia.UsuarioAtual.Id;
            var progresso = DatabaseManager.Instancia.Conexao
                .Table<TabelaProgresso>()
                .Where(p => p.UsuarioId == usuarioId
                         && p.Mundo == mundo
                         && p.Fase == fase
                         && p.Concluida == 1)
                .FirstOrDefault();

            return progresso != null;
        }

        // Fallback PlayerPrefs
        return PlayerPrefs.GetInt($"fase_{mundo}_{fase}_concluida", 0) == 1;
    }

    private void ConfigurarBotaoFase(BotaoFase botao, int numeroFase, bool concluida, bool desbloqueada)
    {
        // Texto
        if (botao.txtNumero != null)
            botao.txtNumero.text = $"Fase {numeroFase}";

        // Cadeado
        if (botao.iconeCadeado != null)
            botao.iconeCadeado.SetActive(!desbloqueada);

        // Cor do botao
        if (botao.imgStatus != null)
        {
            botao.imgStatus.color = concluida    ? botao.corConcluida  :
                                    desbloqueada ? botao.corDisponivel :
                                                   botao.corBloqueada;
        }

        // Interatividade
        botao.btn.interactable = desbloqueada;
        botao.btn.onClick.RemoveAllListeners();

        if (desbloqueada)
        {
            string cena = cenasFases[numeroFase - 1];
            botao.btn.onClick.AddListener(() => AbrirFase(cena, numeroFase));
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // NAVEGACAO
    // ════════════════════════════════════════════════════════════════════════

    private void AbrirFase(string cena, int numeroFase)
    {
        Debug.Log($"[HubFases] Abrindo Mundo {numeroMundo} Fase {numeroFase}: {cena}");
        SceneManager.LoadScene(cena);
    }

    public void Voltar()
    {
        SceneManager.LoadScene("HubMundos");
    }
}

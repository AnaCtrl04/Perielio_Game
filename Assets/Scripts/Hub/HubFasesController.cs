using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Gerencia o Hub de Fases de um mundo especifico.
/// Desbloqueio sequencial: uma fase so abre se a anterior foi concluida (RF07, RF08).
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

    [Header("Botoes das Fases")]
    [SerializeField] private BotaoFase[] fases = new BotaoFase[3];

    [Header("Nomes das Cenas de Cada Fase")]
    [SerializeField] private string[] cenasFases = new string[3];

    [Header("UI Geral")]
    [SerializeField] private TextMeshProUGUI txtNomeMundo;
    [SerializeField] private TextMeshProUGUI txtProgresso;
    [SerializeField] private Button          btnVoltar;

    [Header("Navegacao")]
    [SerializeField] private string cenaHubMundos = "HubMundos";

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
            if (fases[i] == null || fases[i].btn == null)
            {
                Debug.LogWarning($"[HubFases] Botao da fase {i + 1} nao esta atribuido no Inspector!");
                continue;
            }

            int numeroFase = i + 1;

            bool concluida    = FaseConcluida(numeroMundo, numeroFase);
            bool desbloqueada = (numeroFase == 1) || FaseConcluida(numeroMundo, numeroFase - 1);

            if (concluida) totalConcluidas++;

            Debug.Log($"[HubFases] Mundo {numeroMundo} Fase {numeroFase} -> " +
                      $"concluida={concluida} | desbloqueada={desbloqueada}");

            ConfigurarBotaoFase(fases[i], numeroFase, concluida, desbloqueada);
        }

        if (txtProgresso != null)
            txtProgresso.text = $"{totalConcluidas} / {fases.Length} concluidas";
    }

    /// <summary>
    /// Verifica se a fase foi concluida PELO ALUNO LOGADO.
    /// A consulta vai toda pela classe ProgressoAluno, que usa chaves
    /// por aluno — assim um perfil novo nunca herda o avanco de outro.
    /// </summary>
    private bool FaseConcluida(int mundo, int fase)
    {
        return ProgressoAluno.FaseConcluida(mundo, fase);
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

        if (!desbloqueada) return;

        // Valida a cena antes de ligar o clique
        int indice = numeroFase - 1;
        if (cenasFases == null || indice >= cenasFases.Length || string.IsNullOrEmpty(cenasFases[indice]))
        {
            Debug.LogError($"[HubFases] 'Cenas Fases[{indice}]' esta vazio no Inspector! " +
                           $"Preencha com o nome da cena da Fase {numeroFase}.");
            botao.btn.interactable = false;
            return;
        }

        string cena = cenasFases[indice];
        botao.btn.onClick.AddListener(() => AbrirFase(cena, numeroFase));
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
        SceneManager.LoadScene(cenaHubMundos);
    }
}
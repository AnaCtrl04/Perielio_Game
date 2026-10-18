using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia o Hub de Mundos do jogo Perielio.
/// Exibe 3 cards de mundo com desbloqueio progressivo.
/// O Mundo 2 so desbloqueia quando todas as 3 fases do Mundo 1 forem concluidas.
/// O Mundo 3 so desbloqueia quando todas as 3 fases do Mundo 2 forem concluidas.
/// Avaliacao: passou (chegou ao destino) ou nao passou — sem estrelas (RF08, RF09).
/// Todo o progresso e consultado por aluno atraves da classe ProgressoAluno.
/// </summary>
public class HubMundoController : MonoBehaviour
{
    // ─── TOTAL DE FASES POR MUNDO ────────────────────────────────────────────
    private const int FASES_POR_MUNDO = 3;

    // ─── CARDS DOS MUNDOS ────────────────────────────────────────────────────
    [System.Serializable]
    public class CardMundo
    {
        public GameObject card;                    // objeto do card inteiro
        public Button     btnJogar;                // botao "Jogar" do card
        public TextMeshProUGUI txtNome;            // ex: "Mundo 1"
        public TextMeshProUGUI txtSubtitulo;       // ex: "Sistema Solar"
        public TextMeshProUGUI txtProgresso;       // ex: "3 / 3 fases"
        public GameObject painelBloqueado;         // overlay de cadeado
        public GameObject painelDesbloqueado;      // conteudo visivel
        public Image      imgIcone;                // icone do mundo
        public string     cenaFases;               // ex: "HubFasesMundo01"
    }

    [Header("Cards dos Mundos")]
    [SerializeField] private CardMundo mundo1;
    [SerializeField] private CardMundo mundo2;
    [SerializeField] private CardMundo mundo3;

    [Header("UI Geral")]
    [SerializeField] private TextMeshProUGUI txtNomeAluno;
    [SerializeField] private Button          btnSair;

    [Header("Glossario")]
    [SerializeField] private string cenaGlossario = "Glossario";

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        // Nome do aluno logado — usa a mesma fonte do resto do jogo
        if (txtNomeAluno != null)
        {
            string nome = ProgressoAluno.NomeAtual();
            txtNomeAluno.text = string.IsNullOrEmpty(nome) ? "Ola!" : $"Ola, {nome}!";
        }

        if (btnSair != null)
            btnSair.onClick.AddListener(Sair);

        AtualizarMundos();
    }

    // ════════════════════════════════════════════════════════════════════════
    // LOGICA DE DESBLOQUEIO (RF09)
    // ════════════════════════════════════════════════════════════════════════

    private void AtualizarMundos()
    {
        int concluidas1 = ProgressoAluno.FasesConcluidasNoMundo(1, FASES_POR_MUNDO);
        int concluidas2 = ProgressoAluno.FasesConcluidasNoMundo(2, FASES_POR_MUNDO);
        int concluidas3 = ProgressoAluno.FasesConcluidasNoMundo(3, FASES_POR_MUNDO);

        bool mundo1Completo = concluidas1 >= FASES_POR_MUNDO;
        bool mundo2Completo = concluidas2 >= FASES_POR_MUNDO;

        // Mundo 1 — sempre desbloqueado
        ConfigurarCard(mundo1, true, concluidas1, 1);

        // Mundo 2 — exige o Mundo 1 completo
        ConfigurarCard(mundo2, mundo1Completo, concluidas2, 2);

        // Mundo 3 — exige o Mundo 2 completo
        ConfigurarCard(mundo3, mundo2Completo, concluidas3, 3);

        Debug.Log($"[Hub] Progresso de '{ProgressoAluno.NomeAtual()}': " +
                  $"M1={concluidas1}/3  M2={concluidas2}/3  M3={concluidas3}/3");
    }

    /// <summary>
    /// Configura visualmente um card de mundo.
    /// </summary>
    private void ConfigurarCard(CardMundo card, bool desbloqueado, int fasesConcluidas, int numeroMundo)
    {
        if (card == null) return;

        // Alterna painel bloqueado / desbloqueado
        if (card.painelBloqueado    != null) card.painelBloqueado.SetActive(!desbloqueado);
        if (card.painelDesbloqueado != null) card.painelDesbloqueado.SetActive(desbloqueado);

        // Progresso
        if (card.txtProgresso != null)
            card.txtProgresso.text = desbloqueado
                ? $"{fasesConcluidas} / {FASES_POR_MUNDO} fases"
                : "Bloqueado";

        // Botao jogar
        if (card.btnJogar != null)
        {
            card.btnJogar.interactable = desbloqueado;
            card.btnJogar.onClick.RemoveAllListeners();

            if (desbloqueado)
            {
                if (string.IsNullOrEmpty(card.cenaFases))
                {
                    Debug.LogError($"[Hub] 'Cena Fases' do Mundo {numeroMundo} esta vazio no Inspector!");
                    card.btnJogar.interactable = false;
                    return;
                }

                string cena = card.cenaFases;
                card.btnJogar.onClick.AddListener(() => AbrirMundo(cena, numeroMundo));
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // NAVEGACAO
    // ════════════════════════════════════════════════════════════════════════

    private void AbrirMundo(string cena, int numeroMundo)
    {
        Debug.Log($"[Hub] Abrindo Mundo {numeroMundo}: {cena}");
        SceneManager.LoadScene(cena);
    }

    public void AbrirGlossario()
    {
        SceneManager.LoadScene(cenaGlossario);
    }

    public void Sair()
    {
        if (SessaoManager.Instancia != null)
            SessaoManager.Instancia.Sair();
        else
            SceneManager.LoadScene("Cadastro");
    }
}
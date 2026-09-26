using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia o Hub de Mundos do jogo Perielio.
/// Exibe 3 cards de mundo com desbloqueio progressivo.
/// O Mundo 2 so desbloqueia quando todas as 5 fases do Mundo 1 forem concluidas.
/// O Mundo 3 so desbloqueia quando todas as 5 fases do Mundo 2 forem concluidas.
/// Avaliacao: passou (chegou ao destino) ou nao passou — sem estrelas (RF08, RF09).
/// </summary>
public class HubMundoController : MonoBehaviour
{
    // ─── TOTAL DE FASES POR MUNDO ────────────────────────────────────────────
    private const int FASES_POR_MUNDO = 5;

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
        public string     cenaFases;               // ex: "HubFases_Mundo1"
    }

    [Header("Cards dos Mundos")]
    [SerializeField] private CardMundo mundo1;
    [SerializeField] private CardMundo mundo2;
    [SerializeField] private CardMundo mundo3;

    [Header("UI Geral")]
    [SerializeField] private TextMeshProUGUI txtNomeAluno;
    [SerializeField] private Button          btnSair;

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        // Exibe nome do aluno logado
        if (txtNomeAluno != null && SessaoManager.Instancia != null)
            txtNomeAluno.text = $"Ola, {SessaoManager.Instancia.UsuarioAtual?.Nome}!";

        // Conecta botao sair
        if (btnSair != null)
            btnSair.onClick.AddListener(Sair);

        // Atualiza estado de cada mundo
        AtualizarMundos();
    }

    // ════════════════════════════════════════════════════════════════════════
    // LOGICA DE DESBLOQUEIO
    // ════════════════════════════════════════════════════════════════════════

    private void AtualizarMundos()
    {
        int fasesConcluidas1 = ContarFasesConcluidas(1);
        int fasesConcluidas2 = ContarFasesConcluidas(2);
        int fasesConcluidas3 = ContarFasesConcluidas(3);

        bool mundo1Completo = fasesConcluidas1 >= FASES_POR_MUNDO;
        bool mundo2Completo = fasesConcluidas2 >= FASES_POR_MUNDO;

        // Mundo 1 — sempre desbloqueado
        ConfigurarCard(mundo1, true, fasesConcluidas1, 1);

        // Mundo 2 — desbloqueia quando Mundo 1 completo (RF09)
        ConfigurarCard(mundo2, mundo1Completo, fasesConcluidas2, 2);

        // Mundo 3 — desbloqueia quando Mundo 2 completo (RF09)
        ConfigurarCard(mundo3, mundo2Completo, fasesConcluidas3, 3);
    }

    /// <summary>
    /// Conta quantas fases do mundo foram concluidas pelo aluno logado.
    /// Usa SQLite se disponivel, senão usa PlayerPrefs como fallback.
    /// </summary>
    private int ContarFasesConcluidas(int mundo)
    {
        int count = 0;

        for (int fase = 1; fase <= FASES_POR_MUNDO; fase++)
        {
            // Tenta SQLite primeiro
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

                if (progresso != null) count++;
            }
            else
            {
                // Fallback PlayerPrefs
                if (PlayerPrefs.GetInt($"fase_{mundo}_{fase}_concluida", 0) == 1)
                    count++;
            }
        }

        return count;
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
                ? $"{fasesConcluidas} / {FASES_POR_MUNDO} fases concluidas"
                : "Bloqueado";

        // Botao jogar
        if (card.btnJogar != null)
        {
            card.btnJogar.interactable = desbloqueado;
            card.btnJogar.onClick.RemoveAllListeners();

            if (desbloqueado)
            {
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

    public void Sair()
    {
        if (SessaoManager.Instancia != null)
            SessaoManager.Instancia.Sair();
        else
            SceneManager.LoadScene("Cadastro");
    }
}
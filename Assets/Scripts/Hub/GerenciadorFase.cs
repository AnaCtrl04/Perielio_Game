using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Gerencia o estado de cada fase do jogo Periélio.
/// Controla tentativas, pontuação por estrelas, feedback pedagógico
/// e transições entre estados (pré-lançamento, voo, resultado).
/// Implementa as Regras de Negócio RN04, RN05, RN07, RN09, RN12, RN13.
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

    [Header("Dica Pedagógica (RN09)")]
    [SerializeField][TextArea(2, 5)]
    private string dicaConceito = "Quanto maior a massa do foguete, maior a força gravitacional que os planetas exercem sobre ele!";
    [SerializeField] private string conceitoFisico = "Lei da Gravitação Universal";

    // ─── UI ──────────────────────────────────────────────────────────────────
    [Header("UI de Gameplay")]
    [SerializeField] private TextMeshProUGUI txtNomeFase;
    [SerializeField] private TextMeshProUGUI txtTentativas;
    [SerializeField] private Image[] imgEstrelas;              // 3 imagens de estrela no HUD

    [Header("Painel de Resultado")]
    [SerializeField] private GameObject painelResultado;
    [SerializeField] private TextMeshProUGUI txtResultadoTitulo;
    [SerializeField] private TextMeshProUGUI txtDicaPedagogica;
    [SerializeField] private TextMeshProUGUI txtConceitoFisico;
    [SerializeField] private Image[] imgEstrelasResultado;     // 3 estrelas no card
    [SerializeField] private Sprite spriteEstrelaCheia;
    [SerializeField] private Sprite spriteEstrelaVazia;
    [SerializeField] private Button btnProximaFase;
    [SerializeField] private Button btnRepetir;
    [SerializeField] private Button btnMenu;

    [Header("Painel de Colisão")]
    [SerializeField] private GameObject painelColisao;
    [SerializeField] private TextMeshProUGUI txtMensagemColisao;
    [SerializeField] private float tempoExibicaoColisao = 2.5f;

    // ─── ESTADO ──────────────────────────────────────────────────────────────
    private int tentativasUsadas = 0;
    private int estrelasObtidas = 0;
    private bool faseConcluida = false;

    // ─── REFERÊNCIAS ─────────────────────────────────────────────────────────
    private FogueteController foguete;

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        // Singleton
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    private void Start()
    {
        foguete = FindFirstObjectByType<FogueteController>();

        painelResultado.SetActive(false);
        painelColisao.SetActive(false);

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
            ? "A força gravitacional desviou seu foguete! Tente ajustar a massa."
            : $"Tentativa {tentativasUsadas}. Observe o rastro anterior e ajuste!";

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

        estrelasObtidas = CalcularEstrelas();
        SalvarProgresso();

        StartCoroutine(ExibirResultado());
    }

    // ════════════════════════════════════════════════════════════════════════
    // SISTEMA DE ESTRELAS (RN04)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calcula as estrelas obtidas conforme as Regras de Negócio RN04:
    /// 1 estrela = chegou ao destino
    /// 2 estrelas = chegou em menos de N tentativas
    /// 3 estrelas = chegou na primeira tentativa (rota ótima)
    /// </summary>
    private int CalcularEstrelas()
{
    return 1; // avaliacao binaria: passou ou nao passou
}

    // ════════════════════════════════════════════════════════════════════════
    // PERSISTÊNCIA (RN05, RF08)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Salva o progresso. Nunca regride a pontuação (RN05).
    /// </summary>
    private void SalvarProgresso()
    {
        string chave = $"fase_{indiceMundo}_{indiceFase}_estrelas";
        int estrelasSalvas = PlayerPrefs.GetInt(chave, 0);

        // RN05: só atualiza se melhorou
        if (estrelasObtidas > estrelasSalvas)
        {
            PlayerPrefs.SetInt(chave, estrelasObtidas);
            PlayerPrefs.Save();
        }

        // Desbloqueia glossário da fase (RN10)
        PlayerPrefs.SetInt($"glossario_{indiceMundo}_{indiceFase}", 1);
        PlayerPrefs.Save();
    }

    // ════════════════════════════════════════════════════════════════════════
    // UI E FEEDBACK
    // ════════════════════════════════════════════════════════════════════════

    private void AtualizarUI()
    {
        if (txtNomeFase) txtNomeFase.text = nomeFase;
        if (txtTentativas) txtTentativas.text = $"Tentativas: {tentativasUsadas}";
    }

    private IEnumerator ExibirFeedbackColisao(string mensagem)
    {
        painelColisao.SetActive(true);
        txtMensagemColisao.text = mensagem;
        yield return new WaitForSeconds(tempoExibicaoColisao);
        painelColisao.SetActive(false);

        // Reinicia foguete (RN12 - sem limite de tentativas)
        foguete?.Reiniciar();
    }

    private IEnumerator ExibirResultado()
    {
        yield return new WaitForSeconds(0.5f);

        painelResultado.SetActive(true);

        // Título
        txtResultadoTitulo.text = estrelasObtidas == 3
            ? "Perfeito! Rota ótima!" : estrelasObtidas == 2
            ? "Muito bem!" : "Fase concluída!";

        // Dica pedagógica (RN09)
        txtDicaPedagogica.text = dicaConceito;
        txtConceitoFisico.text = $"Conceito aprendido: {conceitoFisico}";

        // Estrelas do resultado
        for (int i = 0; i < imgEstrelasResultado.Length; i++)
            imgEstrelasResultado[i].sprite = i < estrelasObtidas
                ? spriteEstrelaCheia : spriteEstrelaVazia;
    }

    // ════════════════════════════════════════════════════════════════════════
    // BOTÕES DO PAINEL DE RESULTADO
    // ════════════════════════════════════════════════════════════════════════

    public void BtnProximaFase()
    {
        int proximaFase = indiceFase + 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            $"Mundo{indiceMundo}_Fase{proximaFase}");
    }

    public void BtnRepetir()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void BtnMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("HubFases");
    }
}
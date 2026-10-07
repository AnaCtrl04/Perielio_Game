using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GerenciadorFase : MonoBehaviour
{
    public static GerenciadorFase Instancia { get; private set; }

    [Header("Dados da Fase")]
    [SerializeField] private string nomeFase = "Fase 1.1";
    [SerializeField] private int indiceMundo = 1;
    [SerializeField] private int indiceFase  = 1;

    [Header("Dica Pedagogica (RN09)")]
    [SerializeField][TextArea(2, 5)]
    private string dicaConceito = "Quanto maior a massa do foguete, maior a forca gravitacional que os planetas exercem sobre ele!";
    [SerializeField] private string conceitoFisico = "Lei da Gravitacao Universal";

    [Header("Navegacao")]
    [SerializeField] private string cenaProximaFase = "Fase1.2";
    [SerializeField] private string cenaMenu        = "HubFasesMundo01";

    [Header("UI de Gameplay")]
    [SerializeField] private TextMeshProUGUI txtNomeFase;
    [SerializeField] private TextMeshProUGUI txtTentativas;
    [SerializeField] private Image[]         imgEstrelas;

    [Header("Painel de Resultado")]
    [SerializeField] private GameObject      painelResultado;
    [SerializeField] private TextMeshProUGUI txtResultadoTitulo;
    [SerializeField] private TextMeshProUGUI txtDicaPedagogica;
    [SerializeField] private TextMeshProUGUI txtConceitoFisico;
    [SerializeField] private Image[]         imgEstrelasResultado;
    [SerializeField] private Sprite          spriteEstrelaCheia;
    [SerializeField] private Sprite          spriteEstrelaVazia;
    [SerializeField] private Button          btnProximaFase;
    [SerializeField] private Button          btnRepetir;
    [SerializeField] private Button          btnMenu;

    [Header("Painel de Colisao")]
    [SerializeField] private GameObject      painelColisao;
    [SerializeField] private TextMeshProUGUI txtMensagemColisao;
    [SerializeField] private float           tempoExibicaoColisao = 2.5f;

    private int  tentativasUsadas = 0;
    private bool faseConcluida    = false;
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
        if (painelColisao   != null) painelColisao.SetActive(false);

        // Conecta botoes por codigo como seguranca extra
        if (btnProximaFase != null) btnProximaFase.onClick.AddListener(BtnProximaFase);
        if (btnRepetir     != null) btnRepetir.onClick.AddListener(BtnRepetir);
        if (btnMenu        != null) btnMenu.onClick.AddListener(BtnMenu);

        AtualizarUI();
    }

    // ════════════════════════════════════════════════════════════════════════
    // EVENTOS DE JOGO
    // ════════════════════════════════════════════════════════════════════════

    public void RegistrarColisao()
    {
        if (faseConcluida) return;

        tentativasUsadas++;
        AtualizarUI();

        string mensagem = tentativasUsadas == 1
            ? "A forca gravitacional desviou seu foguete! Tente ajustar a massa."
            : $"Tentativa {tentativasUsadas}. Observe o rastro anterior e ajuste!";

        StartCoroutine(ExibirFeedbackColisao(mensagem));
    }

    public void RegistrarChegada()
    {
        if (faseConcluida) return;

        faseConcluida    = true;
        tentativasUsadas++;

        SalvarProgresso();
        StartCoroutine(ExibirResultado());
    }

    // ════════════════════════════════════════════════════════════════════════
    // PERSISTENCIA
    // ════════════════════════════════════════════════════════════════════════

    private void SalvarProgresso()
    {
        // Salva como concluida no PlayerPrefs
        string chave = $"fase_{indiceMundo}_{indiceFase}_concluida";
        PlayerPrefs.SetInt(chave, 1);

        // Salva tentativas
        string chaveTent = $"fase_{indiceMundo}_{indiceFase}_tentativas";
        PlayerPrefs.SetInt(chaveTent, tentativasUsadas);
        PlayerPrefs.Save();

        // Salva no SQLite se disponivel
        if (DatabaseManager.Instancia != null && SessaoManager.Instancia?.UsuarioAtual != null)
        {
            DatabaseManager.Instancia.SalvarProgresso(
                SessaoManager.Instancia.UsuarioAtual.Id,
                indiceMundo,
                indiceFase,
                tentativasUsadas
            );
        }

        // Registra nome do aluno na lista para o modo professor
        string nomeAluno = PlayerPrefs.GetString("sessao_usuario", "");
        if (!string.IsNullOrEmpty(nomeAluno))
        {
            string listaAtual = PlayerPrefs.GetString("lista_alunos", "");
            if (!listaAtual.Contains(nomeAluno))
            {
                listaAtual = string.IsNullOrEmpty(listaAtual) ? nomeAluno : listaAtual + "," + nomeAluno;
                PlayerPrefs.SetString("lista_alunos", listaAtual);
                PlayerPrefs.Save();
            }
        }

        Debug.Log($"[GerenciadorFase] Mundo {indiceMundo} Fase {indiceFase} concluida em {tentativasUsadas} tentativas.");
    }

    // ════════════════════════════════════════════════════════════════════════
    // UI E FEEDBACK
    // ════════════════════════════════════════════════════════════════════════

    private void AtualizarUI()
    {
        if (txtNomeFase   != null) txtNomeFase.text   = nomeFase;
        if (txtTentativas != null) txtTentativas.text = $"Tentativas: {tentativasUsadas}";
    }

    private IEnumerator ExibirFeedbackColisao(string mensagem)
    {
        if (painelColisao != null)
        {
            painelColisao.SetActive(true);
            if (txtMensagemColisao != null) txtMensagemColisao.text = mensagem;
        }

        yield return new WaitForSeconds(tempoExibicaoColisao);

        if (painelColisao != null) painelColisao.SetActive(false);
        if (foguete != null) foguete.Reiniciar();
    }

    private IEnumerator ExibirResultado()
    {
        yield return new WaitForSeconds(0.5f);

        if (painelResultado != null) painelResultado.SetActive(true);

        if (txtResultadoTitulo != null)
            txtResultadoTitulo.text = "Fase concluida!";

        if (txtDicaPedagogica != null)
            txtDicaPedagogica.text = dicaConceito;

        if (txtConceitoFisico != null)
            txtConceitoFisico.text = $"Conceito aprendido: {conceitoFisico}";
    }

    // ════════════════════════════════════════════════════════════════════════
    // BOTOES DE NAVEGACAO
    // ════════════════════════════════════════════════════════════════════════

    public void BtnProximaFase()
    {
        Debug.Log($"[GerenciadorFase] Carregando proxima fase: {cenaProximaFase}");
        SceneManager.LoadScene(cenaProximaFase);
    }

    public void BtnRepetir()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BtnMenu()
    {
        Debug.Log($"[GerenciadorFase] Voltando para menu: {cenaMenu}");
        SceneManager.LoadScene(cenaMenu);
    }

    private int CalcularEstrelas()
    {
        return 1;
    }
}
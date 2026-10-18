using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Glossario educativo do jogo Perielio.
/// Exibe os verbetes (conceitos de fisica/astrofisica) que o aluno desbloqueou
/// ao concluir cada fase. Verbetes ainda nao desbloqueados aparecem bloqueados.
/// Implementa RF10 / RN10.
/// </summary>
public class GlossarioController : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════
    // CLASSE DO VERBETE
    // ═══════════════════════════════════════════════════════════════════

    [System.Serializable]
    public class Verbete
    {
        [Header("Origem (fase que desbloqueia)")]
        public int mundo = 1;
        public int fase = 1;

        [Header("Conteudo")]
        public string titulo = "Conceito";
        [TextArea(3, 6)] public string definicao = "";
        [TextArea(2, 4)] public string curiosidade = "";
    }

    // ═══════════════════════════════════════════════════════════════════
    // CONFIGURACAO NO INSPECTOR
    // ═══════════════════════════════════════════════════════════════════

    [Header("Verbetes do Glossario")]
    [SerializeField] private List<Verbete> verbetes = new List<Verbete>();

    [Header("Lista de Cards")]
    [SerializeField] private Transform containerLista;      // ScrollView/Viewport/Content
    [SerializeField] private GameObject prefabCardVerbete;  // CardVerbete (prefab)

    [Header("Painel de Detalhe")]
    [SerializeField] private GameObject painelDetalhe;
    [SerializeField] private TextMeshProUGUI txtDetalheTitulo;
    [SerializeField] private TextMeshProUGUI txtDetalheDefinicao;
    [SerializeField] private TextMeshProUGUI txtDetalheCuriosidade;
    [SerializeField] private Button btnFecharDetalhe;

    [Header("Textos de Topo")]
    [SerializeField] private TextMeshProUGUI txtProgresso;  // "3 de 9 verbetes desbloqueados"
    [SerializeField] private TextMeshProUGUI txtVazio;      // mensagem quando nada desbloqueado

    [Header("Navegacao")]
    [SerializeField] private Button btnVoltar;
    [SerializeField] private string cenaVoltar = "HubMundos";

    [Header("Cores")]
    [SerializeField] private Color corDesbloqueado = new Color(0.20f, 0.28f, 0.52f, 1f);
    [SerializeField] private Color corBloqueado = new Color(0.16f, 0.17f, 0.22f, 1f);
    [SerializeField] private Color corTextoDesbloqueado = Color.white;
    [SerializeField] private Color corTextoBloqueado = new Color(0.5f, 0.5f, 0.55f, 1f);

    // ═══════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════

    private void Start()
    {
        if (painelDetalhe != null) painelDetalhe.SetActive(false);

        if (btnFecharDetalhe != null)
            btnFecharDetalhe.onClick.AddListener(FecharDetalhe);

        if (btnVoltar != null)
            btnVoltar.onClick.AddListener(Voltar);

        MontarLista();
    }

    // ═══════════════════════════════════════════════════════════════════
    // MONTAGEM DA LISTA
    // ═══════════════════════════════════════════════════════════════════

    private void MontarLista()
    {
        if (containerLista == null || prefabCardVerbete == null)
        {
            Debug.LogError("[Glossario] Container ou Prefab nao atribuido no Inspector!");
            return;
        }

        // Limpa cards antigos
        foreach (Transform filho in containerLista)
            Destroy(filho.gameObject);

        int desbloqueados = 0;

        foreach (Verbete v in verbetes)
        {
            bool liberado = VerbeteDesbloqueado(v.mundo, v.fase);
            if (liberado) desbloqueados++;

            CriarCard(v, liberado);
        }

        // Texto de progresso
        if (txtProgresso != null)
            txtProgresso.text = $"{desbloqueados} de {verbetes.Count} verbetes desbloqueados";

        // Mensagem quando nada foi desbloqueado
        if (txtVazio != null)
            txtVazio.gameObject.SetActive(desbloqueados == 0);

        Debug.Log($"[Glossario] {desbloqueados}/{verbetes.Count} verbetes desbloqueados.");
    }

    /// <summary>
    /// Verifica se a fase de origem do verbete ja foi concluida.
    /// Consulta o SQLite pela conexao publica do DatabaseManager;
    /// se o banco nao estiver disponivel, usa PlayerPrefs.
    /// </summary>
    private bool VerbeteDesbloqueado(int mundo, int fase)
    {
        return ProgressoAluno.VerbeteDesbloqueado(mundo, fase);
    }

    /// <summary>
    /// Instancia um card na lista.
    /// O prefab deve ter, como filhos: TxtTitulo, TxtOrigem, IconeCadeado.
    /// </summary>
    private void CriarCard(Verbete v, bool liberado)
    {
        GameObject card = Instantiate(prefabCardVerbete, containerLista);
        card.SetActive(true);

        TextMeshProUGUI txtTitulo = card.transform.Find("TxtTitulo")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI txtOrigem = card.transform.Find("TxtOrigem")?.GetComponent<TextMeshProUGUI>();
        GameObject iconeCadeado = card.transform.Find("IconeCadeado")?.gameObject;
        Image fundo = card.GetComponent<Image>();
        Button btn = card.GetComponent<Button>();

        if (txtTitulo != null)
        {
            txtTitulo.text = liberado ? v.titulo : "? ? ? ? ?";
            txtTitulo.color = liberado ? corTextoDesbloqueado : corTextoBloqueado;
        }

        if (txtOrigem != null)
        {
            txtOrigem.text = liberado
                ? $"Mundo {v.mundo} - Fase {v.mundo}.{v.fase}"
                : $"Conclua a Fase {v.mundo}.{v.fase} para desbloquear";
            txtOrigem.color = corTextoBloqueado;
        }

        if (iconeCadeado != null)
            iconeCadeado.SetActive(!liberado);

        if (fundo != null)
            fundo.color = liberado ? corDesbloqueado : corBloqueado;

        if (btn != null)
        {
            btn.interactable = liberado;
            if (liberado)
            {
                Verbete copia = v; // captura local para o closure
                btn.onClick.AddListener(() => AbrirDetalhe(copia));
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // PAINEL DE DETALHE
    // ═══════════════════════════════════════════════════════════════════

    public void AbrirDetalhe(Verbete v)
    {
        if (painelDetalhe == null) return;

        painelDetalhe.SetActive(true);

        if (txtDetalheTitulo != null) txtDetalheTitulo.text = v.titulo;
        if (txtDetalheDefinicao != null) txtDetalheDefinicao.text = v.definicao;
        if (txtDetalheCuriosidade != null)
            txtDetalheCuriosidade.text = string.IsNullOrEmpty(v.curiosidade)
                ? "" : $"Voce sabia? {v.curiosidade}";
    }

    public void FecharDetalhe()
    {
        if (painelDetalhe != null) painelDetalhe.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════════════
    // NAVEGACAO
    // ═══════════════════════════════════════════════════════════════════

    public void Voltar()
    {
        SceneManager.LoadScene(cenaVoltar);
    }

    // ═══════════════════════════════════════════════════════════════════
    // PREENCHIMENTO AUTOMATICO (clique com botao direito no componente)
    // ═══════════════════════════════════════════════════════════════════

    [ContextMenu("Preencher Verbetes Padrao")]
    private void PreencherVerbetesPadrao()
    {
        verbetes = new List<Verbete>
        {
            // ─── MUNDO 1: SISTEMA SOLAR ─────────────────────────────────
            new Verbete {
                mundo = 1, fase = 1,
                titulo = "Lei da Gravitacao Universal",
                definicao = "Todo corpo com massa atrai outro corpo com massa. A forca dessa atracao " +
                            "aumenta quando as massas sao maiores e diminui rapidamente quando a " +
                            "distancia entre eles cresce. Formula: F = G x (M1 x M2) / d2.",
                curiosidade = "Isaac Newton formulou essa lei em 1687. Ela explica tanto a queda de " +
                              "uma maca quanto a orbita da Lua ao redor da Terra."
            },
            new Verbete {
                mundo = 1, fase = 2,
                titulo = "Massa e Forca Gravitacional",
                definicao = "Massa e a quantidade de materia de um corpo. Quanto maior a massa de um " +
                            "planeta, mais forte ele puxa objetos proximos. Por isso Jupiter desvia " +
                            "muito mais um foguete do que a Lua.",
                curiosidade = "Jupiter tem cerca de 318 vezes a massa da Terra e funciona como um " +
                              "'escudo', atraindo cometas que poderiam atingir nosso planeta."
            },
            new Verbete {
                mundo = 1, fase = 3,
                titulo = "Assistencia Gravitacional",
                definicao = "Manobra em que uma nave passa perto de um planeta e usa a gravidade dele " +
                            "para ganhar velocidade e mudar de direcao, economizando combustivel.",
                curiosidade = "As sondas Voyager 1 e 2 usaram assistencia gravitacional em Jupiter e " +
                              "Saturno e hoje ja sairam do Sistema Solar."
            },

            // ─── MUNDO 2: ESTRELAS E EVOLUCAO ESTELAR ───────────────────
            new Verbete {
                mundo = 2, fase = 1,
                titulo = "Zona Habitavel",
                definicao = "Faixa de distancia ao redor de uma estrela onde a temperatura permite que " +
                            "exista agua em estado liquido na superficie de um planeta - condicao " +
                            "considerada essencial para a vida como conhecemos.",
                curiosidade = "Tambem chamada de 'zona de Cachinhos Dourados': nem quente demais, nem " +
                              "fria demais. A Terra esta bem no meio dela."
            },
            new Verbete {
                mundo = 2, fase = 2,
                titulo = "Gigante Vermelha",
                definicao = "Fase final da vida de estrelas de massa media. Quando o hidrogenio do nucleo " +
                            "acaba, a estrela se expande muito e sua superficie esfria, ficando avermelhada.",
                curiosidade = "Daqui a cerca de 5 bilhoes de anos o Sol vira uma gigante vermelha e pode " +
                              "engolir a orbita de Mercurio e Venus."
            },
            new Verbete {
                mundo = 2, fase = 3,
                titulo = "Supernova",
                definicao = "Explosao violenta que marca a morte de estrelas muito massivas. Em poucos " +
                            "segundos libera mais energia do que o Sol produzira em toda a sua vida e " +
                            "espalha elementos quimicos pelo espaco.",
                curiosidade = "O calcio dos seus ossos e o ferro do seu sangue foram formados dentro de " +
                              "estrelas e espalhados por supernovas. Somos literalmente poeira de estrelas."
            },

            // ─── MUNDO 3: BURACOS NEGROS E GRAVIDADE EXTREMA ────────────
            new Verbete {
                mundo = 3, fase = 1,
                titulo = "Sistema Binario",
                definicao = "Duas estrelas que orbitam um centro de massa comum. A trajetoria de qualquer " +
                            "objeto que passe perto e influenciada pelas duas ao mesmo tempo.",
                curiosidade = "Mais da metade das estrelas da Via Lactea fazem parte de sistemas com duas " +
                              "ou mais estrelas. Sois solitarios como o nosso sao minoria."
            },
            new Verbete {
                mundo = 3, fase = 2,
                titulo = "Lente Gravitacional",
                definicao = "Quando a luz passa perto de um corpo de massa enorme, sua trajetoria e " +
                            "curvada. O corpo funciona como uma lente e distorce ou amplia a imagem do " +
                            "que esta atras dele.",
                curiosidade = "Previsto por Einstein em 1915 e comprovado em 1919 durante um eclipse " +
                              "observado em Sobral, no Ceara, e na ilha de Principe."
            },
            new Verbete {
                mundo = 3, fase = 3,
                titulo = "Buraco Negro Supermassivo",
                definicao = "Regiao do espaco com gravidade tao intensa que nem a luz consegue escapar. " +
                            "O limite a partir do qual nao ha volta se chama horizonte de eventos.",
                curiosidade = "No centro da Via Lactea existe o Sagitario A*, com massa equivalente a " +
                              "cerca de 4 milhoes de sois. Sua primeira imagem foi divulgada em 2022."
            }
        };

        Debug.Log($"[Glossario] {verbetes.Count} verbetes padrao preenchidos!");
    }
}
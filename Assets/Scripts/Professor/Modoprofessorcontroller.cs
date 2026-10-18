using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controla a tela do Modo Professor do jogo Perielio.
/// Exibe o ranking de alunos com filtro por mundo e por fase, feito com
/// botoes (mais confiaveis e mais faceis de tocar que dropdowns no mobile).
/// RF14 — Autenticacao do Professor.
/// RF15 — Visualizacao de progresso dos alunos.
/// </summary>
public class ModoProfessorController : MonoBehaviour
{
    // ─── FILTROS POR BOTAO ───────────────────────────────────────────────────
    [Header("Filtro de Mundo (indice 0 = Todos)")]
    [Tooltip("Ordem: Todos, Mundo 1, Mundo 2, Mundo 3")]
    [SerializeField] private Button[] btnsMundo = new Button[4];

    [Header("Filtro de Fase (indice 0 = Todas)")]
    [Tooltip("Ordem: Todas, Fase 1, Fase 2, Fase 3")]
    [SerializeField] private Button[] btnsFase = new Button[4];

    [Header("Botao Limpar (opcional)")]
    [SerializeField] private Button btnLimparFiltro;

    [Header("Cores dos Botoes de Filtro")]
    [SerializeField] private Color corFiltroAtivo   = new Color(0.95f, 0.55f, 0.15f, 1f);
    [SerializeField] private Color corFiltroInativo = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color corTextoAtivo    = Color.white;
    [SerializeField] private Color corTextoInativo  = new Color(0.85f, 0.85f, 0.9f, 1f);

    // ─── RANKING ─────────────────────────────────────────────────────────────
    [Header("Ranking")]
    [SerializeField] private Transform       containerRanking;   // ScrollView/Viewport/Content
    [SerializeField] private GameObject      prefabCardAluno;    // prefab de cada linha
    [SerializeField] private TextMeshProUGUI txtTituloRanking;
    [SerializeField] private TextMeshProUGUI txtSemDados;

    // ─── UI GERAL ────────────────────────────────────────────────────────────
    [Header("UI Geral")]
    [SerializeField] private TextMeshProUGUI txtNomeProfessor;
    [SerializeField] private Button          btnVoltar;

    [Header("Navegacao")]
    [SerializeField] private string cenaVoltar = "Cadastro";

    // ─── CORES DO PODIO ──────────────────────────────────────────────────────
    [Header("Cores do Ranking")]
    [SerializeField] private Color corPrimeiro = new Color(1f, 0.84f, 0f, 0.30f);
    [SerializeField] private Color corSegundo  = new Color(0.75f, 0.75f, 0.75f, 0.30f);
    [SerializeField] private Color corTerceiro = new Color(0.80f, 0.50f, 0.20f, 0.30f);
    [SerializeField] private Color corDemais   = new Color(1f, 1f, 1f, 0.10f);
    [SerializeField] private bool  destacarPodio = true;

    // ─── ESTADO ──────────────────────────────────────────────────────────────
    private int mundoFiltro = 0; // 0 = todos
    private int faseFiltro  = 0; // 0 = todas

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        if (txtNomeProfessor != null)
        {
            string nome = PlayerPrefs.GetString(ProgressoAluno.CHAVE_SESSAO, "Professor");
            txtNomeProfessor.text = "Bem-vindo, " + nome + "!";
        }

        if (btnVoltar != null)
            btnVoltar.onClick.AddListener(Voltar);

        if (btnLimparFiltro != null)
            btnLimparFiltro.onClick.AddListener(LimparFiltro);

        ConfigurarBotoesDeFiltro();
        CarregarRanking(0, 0);
    }

    // ════════════════════════════════════════════════════════════════════════
    // BOTOES DE FILTRO
    // ════════════════════════════════════════════════════════════════════════

    private void ConfigurarBotoesDeFiltro()
    {
        for (int i = 0; i < btnsMundo.Length; i++)
        {
            if (btnsMundo[i] == null)
            {
                Debug.LogWarning($"[Professor] Botao de mundo indice {i} nao atribuido no Inspector.");
                continue;
            }

            int indice = i; // captura local para o closure
            btnsMundo[i].onClick.RemoveAllListeners();
            btnsMundo[i].onClick.AddListener(() => SelecionarMundo(indice));
        }

        for (int i = 0; i < btnsFase.Length; i++)
        {
            if (btnsFase[i] == null)
            {
                Debug.LogWarning($"[Professor] Botao de fase indice {i} nao atribuido no Inspector.");
                continue;
            }

            int indice = i;
            btnsFase[i].onClick.RemoveAllListeners();
            btnsFase[i].onClick.AddListener(() => SelecionarFase(indice));
        }

        AtualizarVisualDosFiltros();
    }

    public void SelecionarMundo(int indice)
    {
        mundoFiltro = indice;
        AtualizarVisualDosFiltros();
        CarregarRanking(mundoFiltro, faseFiltro);
    }

    public void SelecionarFase(int indice)
    {
        faseFiltro = indice;
        AtualizarVisualDosFiltros();
        CarregarRanking(mundoFiltro, faseFiltro);
    }

    public void LimparFiltro()
    {
        mundoFiltro = 0;
        faseFiltro  = 0;
        AtualizarVisualDosFiltros();
        CarregarRanking(0, 0);
    }

    /// <summary>Destaca o botao selecionado de cada linha de filtro.</summary>
    private void AtualizarVisualDosFiltros()
    {
        for (int i = 0; i < btnsMundo.Length; i++)
            PintarBotao(btnsMundo[i], i == mundoFiltro);

        for (int i = 0; i < btnsFase.Length; i++)
            PintarBotao(btnsFase[i], i == faseFiltro);
    }

    private void PintarBotao(Button btn, bool ativo)
    {
        if (btn == null) return;

        var img = btn.GetComponent<Image>();
        if (img != null) img.color = ativo ? corFiltroAtivo : corFiltroInativo;

        var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.color = ativo ? corTextoAtivo : corTextoInativo;
    }

    // ════════════════════════════════════════════════════════════════════════
    // RANKING (RF15)
    // ════════════════════════════════════════════════════════════════════════

    private void CarregarRanking(int mundo, int fase)
    {
        LimparCards();

        string titulo = "Ranking Geral";
        if (mundo > 0 && fase > 0) titulo = $"Ranking — Mundo {mundo} / Fase {fase}";
        else if (mundo > 0)        titulo = $"Ranking — Mundo {mundo}";
        else if (fase  > 0)        titulo = $"Ranking — Fase {fase} (todos os mundos)";

        if (txtTituloRanking != null) txtTituloRanking.text = titulo;

        List<DadosAluno> alunos = BuscarDadosAlunos(mundo, fase);

        if (alunos.Count == 0)
        {
            if (txtSemDados != null)
            {
                txtSemDados.text = "Nenhum aluno encontrado.";
                txtSemDados.gameObject.SetActive(true);
            }
            return;
        }

        if (txtSemDados != null) txtSemDados.gameObject.SetActive(false);

        alunos = alunos
            .OrderByDescending(a => a.fasesConcluidas)
            .ThenBy(a => a.totalTentativas)
            .ToList();

        for (int i = 0; i < alunos.Count; i++)
            CriarCardAluno(alunos[i], i + 1);
    }

    /// <summary>
    /// Monta o ranking a partir das MESMAS chaves que o restante do jogo usa
    /// (classe ProgressoAluno), garantindo que o que o professor ve seja
    /// exatamente o progresso que o aluno tem nos hubs e no glossario.
    /// </summary>
    private List<DadosAluno> BuscarDadosAlunos(int mundo, int fase)
    {
        var resultado = new List<DadosAluno>();

        string[] nomes = ProgressoAluno.ListaDeAlunos();
        if (nomes.Length == 0) return resultado;

        // 0 nos filtros significa "todos"
        int mundoMin = mundo > 0 ? mundo : 1;
        int mundoMax = mundo > 0 ? mundo : 3;
        int faseMin  = fase  > 0 ? fase  : 1;
        int faseMax  = fase  > 0 ? fase  : 3;

        foreach (string bruto in nomes)
        {
            string nome = bruto.Trim();
            if (string.IsNullOrEmpty(nome)) continue;

            var dados = new DadosAluno { nome = nome };

            for (int m = mundoMin; m <= mundoMax; m++)
            {
                for (int f = faseMin; f <= faseMax; f++)
                {
                    if (!ProgressoAluno.FaseConcluidaDe(nome, m, f)) continue;

                    dados.fasesConcluidas++;
                    dados.totalTentativas += ProgressoAluno.TentativasDe(nome, m, f);
                }
            }

            resultado.Add(dados);
        }

        Debug.Log($"[Professor] Filtro mundo={mundo} fase={fase} -> " +
                  $"{resultado.Count} aluno(s). " +
                  string.Join(" | ", resultado.Select(a => $"{a.nome}:{a.fasesConcluidas}")));

        return resultado;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CARDS DO RANKING
    // ════════════════════════════════════════════════════════════════════════

    private void CriarCardAluno(DadosAluno dados, int posicao)
    {
        if (prefabCardAluno == null || containerRanking == null)
        {
            Debug.LogError("[Professor] 'Prefab Card Aluno' ou 'Container Ranking' nao atribuido!");
            return;
        }

        GameObject card = Instantiate(prefabCardAluno, containerRanking);
        card.SetActive(true);

        var txtPos = card.transform.Find("TxtPosicao")?.GetComponent<TextMeshProUGUI>();
        if (txtPos != null) txtPos.text = posicao + "º";

        var txtNome = card.transform.Find("TxtNome")?.GetComponent<TextMeshProUGUI>();
        if (txtNome != null) txtNome.text = dados.nome;

        var txtFases = card.transform.Find("TxtFases")?.GetComponent<TextMeshProUGUI>();
        if (txtFases != null)
            txtFases.text = dados.fasesConcluidas == 1 ? "1 fase" : dados.fasesConcluidas + " fases";

        var txtTent = card.transform.Find("TxtTentativas")?.GetComponent<TextMeshProUGUI>();
        if (txtTent != null) txtTent.text = dados.totalTentativas + " tent.";

        if (!destacarPodio) return;

        var imgFundo = card.GetComponent<Image>();
        if (imgFundo != null)
        {
            imgFundo.color = posicao == 1 ? corPrimeiro
                           : posicao == 2 ? corSegundo
                           : posicao == 3 ? corTerceiro
                           : corDemais;
        }
    }

    private void LimparCards()
    {
        if (containerRanking == null) return;
        foreach (Transform filho in containerRanking)
            Destroy(filho.gameObject);
    }

    // ════════════════════════════════════════════════════════════════════════
    // NAVEGACAO
    // ════════════════════════════════════════════════════════════════════════

    public void Voltar()
    {
        SceneManager.LoadScene(cenaVoltar);
    }
}

// ════════════════════════════════════════════════════════════════════════
// MODELO DE DADOS
// ════════════════════════════════════════════════════════════════════════

public class DadosAluno
{
    public string nome;
    public int    fasesConcluidas;
    public int    totalTentativas;
}
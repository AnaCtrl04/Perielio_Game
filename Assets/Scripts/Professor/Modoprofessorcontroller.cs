using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controla a tela do Modo Professor do jogo Periélio.
/// Exibe ranking de alunos com filtro por mundo e fase.
/// RF14 — Autenticação do Professor.
/// RF15 — Visualização de progresso dos alunos.
/// </summary>
public class ModoProfessorController : MonoBehaviour
{
    // ─── FILTROS ─────────────────────────────────────────────────────────────
    [Header("Filtros")]
    [SerializeField] private TMP_Dropdown dropdownMundo;
    [SerializeField] private TMP_Dropdown dropdownFase;
    [SerializeField] private Button       btnFiltrar;
    [SerializeField] private Button       btnLimparFiltro;

    // ─── RANKING ─────────────────────────────────────────────────────────────
    [Header("Ranking")]
    [SerializeField] private Transform    containerRanking;   // pai dos cards
    [SerializeField] private GameObject   prefabCardAluno;    // prefab de cada linha
    [SerializeField] private TextMeshProUGUI txtTituloRanking;
    [SerializeField] private TextMeshProUGUI txtSemDados;

    // ─── UI GERAL ────────────────────────────────────────────────────────────
    [Header("UI Geral")]
    [SerializeField] private TextMeshProUGUI txtNomeProfessor;
    [SerializeField] private Button          btnVoltar;

    // ─── ESTADO ──────────────────────────────────────────────────────────────
    private int mundoFiltro = 0; // 0 = todos
    private int faseFiltro  = 0; // 0 = todas

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        // Nome do professor logado
        if (txtNomeProfessor != null)
        {
            string nome = PlayerPrefs.GetString("sessao_usuario", "Professor");
            txtNomeProfessor.text = "Bem-vindo, " + nome + "!";
        }

        // Botoes
        if (btnVoltar   != null) btnVoltar.onClick.AddListener(Voltar);
        if (btnFiltrar  != null) btnFiltrar.onClick.AddListener(AplicarFiltro);
        if (btnLimparFiltro != null) btnLimparFiltro.onClick.AddListener(LimparFiltro);

        // Configura dropdowns
        ConfigurarDropdowns();

        // Carrega ranking geral
        CarregarRanking(0, 0);
    }

    // ════════════════════════════════════════════════════════════════════════
    // DROPDOWNS
    // ════════════════════════════════════════════════════════════════════════

    private void ConfigurarDropdowns()
    {
        // Dropdown Mundo
        if (dropdownMundo != null)
        {
            dropdownMundo.ClearOptions();
            dropdownMundo.AddOptions(new List<string>
            {
                "Mundos",
                "Mundo 1 — Sistema Solar",
                "Mundo 2 — Evolução Estelar",
                "Mundo 3 — Cosmo Profundo"
            });
            dropdownMundo.onValueChanged.AddListener(OnMundoChanged);
        }

        // Dropdown Fase
        if (dropdownFase != null)
        {
            dropdownFase.ClearOptions();
            dropdownFase.AddOptions(new List<string>
            {
                "Fases",
                "Fase 1",
                "Fase 2",
                "Fase 3"
            });
        }
    }

    private void OnMundoChanged(int index)
    {
        mundoFiltro = index;
    }

    // ════════════════════════════════════════════════════════════════════════
    // FILTRO
    // ════════════════════════════════════════════════════════════════════════

    public void AplicarFiltro()
    {
        mundoFiltro = dropdownMundo != null ? dropdownMundo.value : 0;
        faseFiltro  = dropdownFase  != null ? dropdownFase.value  : 0;
        CarregarRanking(mundoFiltro, faseFiltro);
    }

    public void LimparFiltro()
    {
        if (dropdownMundo != null) dropdownMundo.value = 0;
        if (dropdownFase  != null) dropdownFase.value  = 0;
        mundoFiltro = 0;
        faseFiltro  = 0;
        CarregarRanking(0, 0);
    }

    // ════════════════════════════════════════════════════════════════════════
    // RANKING (RF15)
    // ════════════════════════════════════════════════════════════════════════

    private void CarregarRanking(int mundo, int fase)
    {
        // Limpa cards anteriores
        LimparCards();

        // Atualiza título
        string titulo = "Ranking Geral";
        if (mundo > 0 && fase > 0)
            titulo = $"Ranking — Mundo {mundo} / Fase {fase}";
        else if (mundo > 0)
            titulo = $"Ranking — Mundo {mundo}";

        if (txtTituloRanking != null)
            txtTituloRanking.text = titulo;

        // Busca todos os alunos no PlayerPrefs
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

        // Ordena: mais fases concluídas primeiro, depois menos tentativas
        alunos = alunos
            .OrderByDescending(a => a.fasesConcluidas)
            .ThenBy(a => a.totalTentativas)
            .ToList();

        // Cria cards
        for (int i = 0; i < alunos.Count; i++)
        {
            CriarCardAluno(alunos[i], i + 1);
        }
    }

    private List<DadosAluno> BuscarDadosAlunos(int mundo, int fase)
    {
        var resultado = new List<DadosAluno>();

        // Busca via SQLite se disponível
        if (DatabaseManager.Instancia != null)
        {
            var usuarios = DatabaseManager.Instancia.Conexao
                .Table<TabelaUsuario>()
                .Where(u => u.Perfil == "aluno")
                .ToList();

            foreach (var usuario in usuarios)
            {
                var dados = new DadosAluno { nome = usuario.Nome };

                // Busca progresso filtrado
                var query = DatabaseManager.Instancia.Conexao
                    .Table<TabelaProgresso>()
                    .Where(p => p.UsuarioId == usuario.Id && p.Concluida == 1);

                if (mundo > 0) query = query.Where(p => p.Mundo == mundo);
                if (fase  > 0) query = query.Where(p => p.Fase  == fase);

                var progressos = query.ToList();
                dados.fasesConcluidas  = progressos.Count;
                dados.totalTentativas  = progressos.Sum(p => p.Tentativas);

                resultado.Add(dados);
            }
        }
        else
        {
            // Fallback PlayerPrefs
            resultado = BuscarDadosPlayerPrefs(mundo, fase);
        }

        return resultado;
    }

    private List<DadosAluno> BuscarDadosPlayerPrefs(int mundo, int fase)
    {
        var resultado = new List<DadosAluno>();
        string listaAlunos = PlayerPrefs.GetString("lista_alunos", "");

        if (string.IsNullOrEmpty(listaAlunos)) return resultado;

        string[] nomes = listaAlunos.Split(',');
        int mundoMax = mundo > 0 ? mundo : 3;
        int faseMax  = fase  > 0 ? fase  : 3;
        int mundoMin = mundo > 0 ? mundo : 1;
        int faseMin  = fase  > 0 ? fase  : 1;

        foreach (string nome in nomes)
        {
            if (string.IsNullOrEmpty(nome)) continue;

            var dados = new DadosAluno { nome = nome };
            int concluidas = 0;
            int tentativas = 0;

            for (int m = mundoMin; m <= mundoMax; m++)
            {
                for (int f = faseMin; f <= faseMax; f++)
                {
                    string prefixo = $"aluno_{nome}_m{m}_f{f}";
                    if (PlayerPrefs.GetInt(prefixo + "_concluida", 0) == 1)
                    {
                        concluidas++;
                        tentativas += PlayerPrefs.GetInt(prefixo + "_tentativas", 0);
                    }
                }
            }

            dados.fasesConcluidas = concluidas;
            dados.totalTentativas = tentativas;
            resultado.Add(dados);
        }

        return resultado;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CARDS DO RANKING
    // ════════════════════════════════════════════════════════════════════════

    private void CriarCardAluno(DadosAluno dados, int posicao)
    {
        if (prefabCardAluno == null || containerRanking == null) return;

        GameObject card = Instantiate(prefabCardAluno, containerRanking);

        // Posição no ranking
        var txtPos = card.transform.Find("TxtPosicao")?.GetComponent<TextMeshProUGUI>();
        if (txtPos != null) txtPos.text = posicao + "º";

        // Nome do aluno
        var txtNome = card.transform.Find("TxtNome")?.GetComponent<TextMeshProUGUI>();
        if (txtNome != null) txtNome.text = dados.nome;

        // Fases concluídas
        var txtFases = card.transform.Find("TxtFases")?.GetComponent<TextMeshProUGUI>();
        if (txtFases != null) txtFases.text = dados.fasesConcluidas + " fases";

        // Total de tentativas
        var txtTent = card.transform.Find("TxtTentativas")?.GetComponent<TextMeshProUGUI>();
        if (txtTent != null) txtTent.text = dados.totalTentativas + " tent.";

        // Destaque para top 3
        var imgFundo = card.GetComponent<Image>();
        if (imgFundo != null)
        {
            imgFundo.color = posicao == 1 ? new Color(1f, 0.84f, 0f, 0.3f)    // ouro
                           : posicao == 2 ? new Color(0.75f, 0.75f, 0.75f, 0.3f) // prata
                           : posicao == 3 ? new Color(0.8f, 0.5f, 0.2f, 0.3f)  // bronze
                           : new Color(1f, 1f, 1f, 0.1f);                        // normal
        }
    }

    private void LimparCards()
    {
        if (containerRanking == null) return;
        foreach (Transform filho in containerRanking)
            Destroy(filho.gameObject);
    }

    // ════════════════════════════════════════════════════════════════════════
    // NAVEGAÇÃO
    // ════════════════════════════════════════════════════════════════════════

    public void Voltar()
    {
        SceneManager.LoadScene("Cadastro");
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
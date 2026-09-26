using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla a tela de cadastro e login do jogo Periélio.
/// Gerencia perfis de Aluno e Professor com usuário e senha (RF01, RF14).
/// Utiliza PlayerPrefs para persistência local sem necessidade de servidor (RF09).
/// </summary>
public class CadastroController : MonoBehaviour
{
    // ─── TIPO DE PERFIL ──────────────────────────────────────────────────────
    private enum TipoPerfil { Nenhum, Aluno, Professor }
    private TipoPerfil perfilSelecionado = TipoPerfil.Nenhum;

    // ─── PAINEL PRINCIPAL ────────────────────────────────────────────────────
    [Header("Botões de Perfil")]
    [SerializeField] private Button btnAluno;
    [SerializeField] private Button btnProfessor;
    [SerializeField] private Color corBotaoSelecionado = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color corBotaoNormal      = new Color(0.3f, 0.3f, 0.3f);

    // ─── CAMPOS DO FORMULÁRIO ────────────────────────────────────────────────
    [Header("Campos de Cadastro")]
    [SerializeField] private TMP_InputField inputNome;
    [SerializeField] private TMP_InputField inputSenha;

    // ─── ALTERNÂNCIA CADASTRO / LOGIN ────────────────────────────────────────
    [Header("Alternância Cadastro / Login")]
    [SerializeField] private GameObject painelCadastro;
    [SerializeField] private GameObject painelLogin;

    // ─── CAMPOS DE LOGIN ─────────────────────────────────────────────────────
    [Header("Campos de Login")]
    [SerializeField] private TMP_InputField inputLoginNome;
    [SerializeField] private TMP_InputField inputLoginSenha;
    [SerializeField] private Button         btnLoginAluno;
    [SerializeField] private Button         btnLoginProfessor;
    private TipoPerfil perfilLogin = TipoPerfil.Nenhum;

    // ─── FEEDBACK ────────────────────────────────────────────────────────────
    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI txtErro;
    [SerializeField] private TextMeshProUGUI txtSucesso;

    // ─── CENAS ───────────────────────────────────────────────────────────────
    [Header("Cenas")]
    [SerializeField] private string cenaHubMundos = "HubMundos";
    [SerializeField] private string cenaModoProfessor = "ModoProfessor";

    // ─── PREFIXOS PLAYERPREFS ────────────────────────────────────────────────
    private const string PREFIXO_ALUNO     = "aluno_";
    private const string PREFIXO_PROFESSOR = "prof_";

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        // Inicia mostrando cadastro
        MostrarCadastro();
        LimparFeedback();

    }

    // ════════════════════════════════════════════════════════════════════════
    // ALTERNÂNCIA CADASTRO / LOGIN
    // ════════════════════════════════════════════════════════════════════════

    public void MostrarCadastro()
    {
        if (painelCadastro != null) painelCadastro.SetActive(true);
        if (painelLogin    != null) painelLogin.SetActive(false);
        LimparFeedback();
    }

    public void MostrarLogin()
    {
        if (painelCadastro != null) painelCadastro.SetActive(false);
        if (painelLogin    != null) painelLogin.SetActive(true);
        LimparFeedback();
    }

    // ════════════════════════════════════════════════════════════════════════
    // SELEÇÃO DE PERFIL — CADASTRO
    // ════════════════════════════════════════════════════════════════════════

    public void SelecionarAluno()
    {
        perfilSelecionado = TipoPerfil.Aluno;
        AtualizarCorBotoes(btnAluno, btnProfessor);

        LimparFeedback();
    }

    public void SelecionarProfessor()
    {
        perfilSelecionado = TipoPerfil.Professor;
        AtualizarCorBotoes(btnProfessor, btnAluno);

        LimparFeedback();
    }

    private void AtualizarCorBotoes(Button btnAtivo, Button btnInativo)
    {
        if (btnAtivo   != null) btnAtivo.GetComponent<Image>().color   = corBotaoSelecionado;
        if (btnInativo != null) btnInativo.GetComponent<Image>().color = corBotaoNormal;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CADASTRO (RF01)
    // ════════════════════════════════════════════════════════════════════════

    public void Cadastrar()
    {
        LimparFeedback();

        // Validações
        if (perfilSelecionado == TipoPerfil.Nenhum)
        {
            MostrarErro("Selecione um perfil: Aluno ou Professor.");
            return;
        }

        string nome  = inputNome  != null ? inputNome.text.Trim()  : "";
        string senha = inputSenha != null ? inputSenha.text.Trim() : "";

        if (string.IsNullOrEmpty(nome))
        {
            MostrarErro("Informe um nome de usuário.");
            return;
        }

        if (string.IsNullOrEmpty(senha) || senha.Length < 6)
        {
            MostrarErro("A senha deve ter pelo menos 6 caracteres.");
            return;
        }

        // Verifica se já existe
        string prefixo = perfilSelecionado == TipoPerfil.Aluno
            ? PREFIXO_ALUNO
            : PREFIXO_PROFESSOR;

        if (PlayerPrefs.HasKey(prefixo + nome + "_senha"))
        {
            MostrarErro("Usuário já cadastrado. Faça login.");
            return;
        }

        // Salva no PlayerPrefs
        PlayerPrefs.SetString(prefixo + nome + "_senha", senha);
        PlayerPrefs.SetString(prefixo + nome + "_perfil",
            perfilSelecionado == TipoPerfil.Aluno ? "aluno" : "professor");
       

        PlayerPrefs.Save();

        Debug.Log($"[Cadastro] {perfilSelecionado} '{nome}' cadastrado com sucesso!");
        MostrarSucesso("Cadastro realizado! Faça login para continuar.");

        // Limpa campos
        if (inputNome  != null) inputNome.text  = "";
        if (inputSenha != null) inputSenha.text = "";
    }

    // ════════════════════════════════════════════════════════════════════════
    // SELEÇÃO DE PERFIL — LOGIN
    // ════════════════════════════════════════════════════════════════════════

    public void LoginSelecionarAluno()
    {
        perfilLogin = TipoPerfil.Aluno;
        AtualizarCorBotoes(btnLoginAluno, btnLoginProfessor);
        LimparFeedback();
    }

    public void LoginSelecionarProfessor()
    {
        perfilLogin = TipoPerfil.Professor;
        AtualizarCorBotoes(btnLoginProfessor, btnLoginAluno);
        LimparFeedback();
    }

    // ════════════════════════════════════════════════════════════════════════
    // LOGIN (RF01.1, RF01.2, RF14)
    // ════════════════════════════════════════════════════════════════════════

    public void Entrar()
    {
        LimparFeedback();

        if (perfilLogin == TipoPerfil.Nenhum)
        {
            MostrarErro("Selecione um perfil: Aluno ou Professor.");
            return;
        }

        string nome  = inputLoginNome  != null ? inputLoginNome.text.Trim()  : "";
        string senha = inputLoginSenha != null ? inputLoginSenha.text.Trim() : "";

        if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(senha))
        {
            MostrarErro("Preencha usuário e senha.");
            return;
        }

        string prefixo = perfilLogin == TipoPerfil.Aluno
            ? PREFIXO_ALUNO
            : PREFIXO_PROFESSOR;

        string chave = prefixo + nome + "_senha";

        // Verifica se usuário existe
        if (!PlayerPrefs.HasKey(chave))
        {
            MostrarErro("Usuário não encontrado. Cadastre-se primeiro.");
            return;
        }

        // Verifica senha
        string senhaSalva = PlayerPrefs.GetString(chave);
        if (senhaSalva != senha)
        {
            MostrarErro("Senha incorreta. Tente novamente.");
            return;
        }

        // Login bem-sucedido — salva sessão atual
        PlayerPrefs.SetString("sessao_usuario", nome);
        PlayerPrefs.SetString("sessao_perfil",
            perfilLogin == TipoPerfil.Aluno ? "aluno" : "professor");
        PlayerPrefs.Save();

        Debug.Log($"[Login] {perfilLogin} '{nome}' logado com sucesso!");

        // Redireciona conforme perfil
        if (perfilLogin == TipoPerfil.Professor)
        {
            SceneManager.LoadScene(cenaModoProfessor);
        }
        else
        {
            // No método Entrar() — aluno vai para HubMundos
            SceneManager.LoadScene(cenaHubMundos);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // FEEDBACK
    // ════════════════════════════════════════════════════════════════════════

    private void MostrarErro(string mensagem)
    {
        if (txtErro    != null) { txtErro.text    = mensagem; txtErro.gameObject.SetActive(true); }
        if (txtSucesso != null) txtSucesso.gameObject.SetActive(false);
    }

    private void MostrarSucesso(string mensagem)
    {
        if (txtSucesso != null) { txtSucesso.text = mensagem; txtSucesso.gameObject.SetActive(true); }
        if (txtErro    != null) txtErro.gameObject.SetActive(false);
    }

    private void LimparFeedback()
    {
        if (txtErro    != null) txtErro.gameObject.SetActive(false);
        if (txtSucesso != null) txtSucesso.gameObject.SetActive(false);
    }

    // ════════════════════════════════════════════════════════════════════════
    // UTILITÁRIO ESTÁTICO — outros scripts podem verificar a sessão
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Retorna o nome do usuário logado atualmente.
    /// </summary>
    public static string UsuarioAtual()
    {
        return PlayerPrefs.GetString("sessao_usuario", "");
    }

    /// <summary>
    /// Retorna true se o usuário logado é professor.
    /// </summary>
    public static bool EhProfessor()
    {
        return PlayerPrefs.GetString("sessao_perfil", "") == "professor";
    }

    /// <summary>
    /// Encerra a sessão e volta para a tela de login.
    /// </summary>
    public void Sair()
    {
        PlayerPrefs.DeleteKey("sessao_usuario");
        PlayerPrefs.DeleteKey("sessao_perfil");
        PlayerPrefs.Save();
        SceneManager.LoadScene("Cadastro");
    }
}
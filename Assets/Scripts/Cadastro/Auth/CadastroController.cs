using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla a tela de cadastro e login do jogo Perielio.
/// Gerencia perfis de Aluno e Professor com usuario e senha (RF01, RF14).
/// Utiliza PlayerPrefs para persistencia local sem necessidade de servidor (RF09).
/// As senhas sao gravadas como hash SHA-256 com salt individual (RNF08).
/// Contas criadas antes da adocao do hash sao migradas automaticamente no login.
/// </summary>
public class CadastroController : MonoBehaviour
{
    // ─── TIPO DE PERFIL ──────────────────────────────────────────────────────
    private enum TipoPerfil { Nenhum, Aluno, Professor }
    private TipoPerfil perfilSelecionado = TipoPerfil.Nenhum;

    // ─── PAINEL PRINCIPAL ────────────────────────────────────────────────────
    [Header("Botoes de Perfil")]
    [SerializeField] private Button btnAluno;
    [SerializeField] private Button btnProfessor;
    [SerializeField] private Color corBotaoSelecionado = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color corBotaoNormal      = new Color(0.3f, 0.3f, 0.3f);

    // ─── CAMPOS DO FORMULARIO ────────────────────────────────────────────────
    [Header("Campos de Cadastro")]
    [SerializeField] private TMP_InputField inputNome;
    [SerializeField] private TMP_InputField inputSenha;

    // ─── ALTERNANCIA CADASTRO / LOGIN ────────────────────────────────────────
    [Header("Alternancia Cadastro / Login")]
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
    [SerializeField] private string cenaHubMundos     = "HubMundos";
    [SerializeField] private string cenaModoProfessor = "ModoProfessor";

    // ─── PREFIXOS PLAYERPREFS ────────────────────────────────────────────────
    private const string PREFIXO_ALUNO     = "aluno_";
    private const string PREFIXO_PROFESSOR = "prof_";

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        MostrarCadastro();
        LimparFeedback();
    }

    // ════════════════════════════════════════════════════════════════════════
    // ALTERNANCIA CADASTRO / LOGIN
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
    // SELECAO DE PERFIL — CADASTRO
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

        if (perfilSelecionado == TipoPerfil.Nenhum)
        {
            MostrarErro("Selecione um perfil: Aluno ou Professor.");
            return;
        }

        string nome  = inputNome  != null ? inputNome.text.Trim()  : "";
        string senha = inputSenha != null ? inputSenha.text.Trim() : "";

        if (string.IsNullOrEmpty(nome))
        {
            MostrarErro("Informe um nome de usuario.");
            return;
        }

        if (string.IsNullOrEmpty(senha) || senha.Length < 6)
        {
            MostrarErro("A senha deve ter pelo menos 6 caracteres.");
            return;
        }

        string prefixo = perfilSelecionado == TipoPerfil.Aluno
            ? PREFIXO_ALUNO
            : PREFIXO_PROFESSOR;

        if (PlayerPrefs.HasKey(prefixo + nome + "_senha"))
        {
            MostrarErro("Usuario ja cadastrado. Faca login.");
            return;
        }

        // ─── RNF08: nunca grava a senha em texto puro ───────────────────────
        string salt = SegurancaSenha.GerarSalt();

        PlayerPrefs.SetString(prefixo + nome + "_salt",  salt);
        PlayerPrefs.SetString(prefixo + nome + "_senha", SegurancaSenha.GerarHash(senha, salt));
        PlayerPrefs.SetString(prefixo + nome + "_perfil",
            perfilSelecionado == TipoPerfil.Aluno ? "aluno" : "professor");

        PlayerPrefs.Save();

        Debug.Log($"[Cadastro] {perfilSelecionado} '{nome}' cadastrado com sucesso!");
        MostrarSucesso("Cadastro realizado! Faca login para continuar.");

        if (inputNome  != null) inputNome.text  = "";
        if (inputSenha != null) inputSenha.text = "";
    }

    // ════════════════════════════════════════════════════════════════════════
    // SELECAO DE PERFIL — LOGIN
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
            MostrarErro("Preencha usuario e senha.");
            return;
        }

        string prefixo = perfilLogin == TipoPerfil.Aluno
            ? PREFIXO_ALUNO
            : PREFIXO_PROFESSOR;

        string chaveSenha = prefixo + nome + "_senha";
        string chaveSalt  = prefixo + nome + "_salt";

        if (!PlayerPrefs.HasKey(chaveSenha))
        {
            MostrarErro("Usuario nao encontrado. Cadastre-se primeiro.");
            return;
        }

        string salt      = PlayerPrefs.GetString(chaveSalt, "");
        string hashSalvo = PlayerPrefs.GetString(chaveSenha);
        bool   senhaOk;

        if (string.IsNullOrEmpty(salt))
        {
            // ─── CONTA ANTIGA ────────────────────────────────────────────────
            // Criada antes da adocao do hash: a senha esta gravada em texto
            // puro e nao existe salt. Compara direto e, se conferir, converte
            // a conta para o formato novo sem o usuario perceber.
            senhaOk = (hashSalvo == senha);

            if (senhaOk)
            {
                string novoSalt = SegurancaSenha.GerarSalt();
                PlayerPrefs.SetString(chaveSalt,  novoSalt);
                PlayerPrefs.SetString(chaveSenha, SegurancaSenha.GerarHash(senha, novoSalt));
                PlayerPrefs.Save();

                Debug.Log($"[Login] Conta '{nome}' migrada do formato antigo " +
                          $"para senha com hash SHA-256.");
            }
        }
        else
        {
            // ─── CONTA NOVA ──────────────────────────────────────────────────
            senhaOk = SegurancaSenha.Conferir(senha, salt, hashSalvo);
        }

        if (!senhaOk)
        {
            MostrarErro("Senha incorreta. Tente novamente.");
            return;
        }

        // Login bem-sucedido — salva sessao atual
        PlayerPrefs.SetString("sessao_usuario", nome);
        PlayerPrefs.SetString("sessao_perfil",
            perfilLogin == TipoPerfil.Aluno ? "aluno" : "professor");
        PlayerPrefs.Save();

        Debug.Log($"[Login] {perfilLogin} '{nome}' logado com sucesso!");

        if (perfilLogin == TipoPerfil.Professor)
            SceneManager.LoadScene(cenaModoProfessor);
        else
            SceneManager.LoadScene(cenaHubMundos);
    }

    // ════════════════════════════════════════════════════════════════════════
    // FEEDBACK
    // ════════════════════════════════════════════════════════════════════════

    private void MostrarErro(string mensagem)
    {
        if (txtErro    != null) { txtErro.text = mensagem; txtErro.gameObject.SetActive(true); }
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
    // UTILITARIO ESTATICO — outros scripts podem verificar a sessao
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Retorna o nome do usuario logado atualmente.</summary>
    public static string UsuarioAtual()
    {
        return PlayerPrefs.GetString("sessao_usuario", "");
    }

    /// <summary>Retorna true se o usuario logado e professor.</summary>
    public static bool EhProfessor()
    {
        return PlayerPrefs.GetString("sessao_perfil", "") == "professor";
    }

    /// <summary>Encerra a sessao e volta para a tela de login.</summary>
    public void Sair()
    {
        PlayerPrefs.DeleteKey("sessao_usuario");
        PlayerPrefs.DeleteKey("sessao_perfil");
        PlayerPrefs.Save();
        SceneManager.LoadScene("Cadastro");
    }

    // ════════════════════════════════════════════════════════════════════════
    // UTILITARIOS DE TESTE
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Apaga TODAS as contas (senha, salt e perfil) de alunos e professores.
    /// O utilitario de ProgressoAluno apaga apenas o progresso; este apaga as
    /// credenciais. Clique com o botao direito no componente para executar.
    /// </summary>
    [ContextMenu("Apagar TODAS as contas cadastradas")]
    private void ApagarTodasAsContas()
    {
        int removidas = 0;

        // Alunos registrados na lista do ranking
        foreach (string nome in ProgressoAluno.ListaDeAlunos())
        {
            string n = nome.Trim();
            if (string.IsNullOrEmpty(n)) continue;

            foreach (string prefixo in new[] { PREFIXO_ALUNO, PREFIXO_PROFESSOR })
            {
                if (!PlayerPrefs.HasKey(prefixo + n + "_senha")) continue;
                PlayerPrefs.DeleteKey(prefixo + n + "_senha");
                PlayerPrefs.DeleteKey(prefixo + n + "_salt");
                PlayerPrefs.DeleteKey(prefixo + n + "_perfil");
                removidas++;
            }
        }

        PlayerPrefs.DeleteKey("sessao_usuario");
        PlayerPrefs.DeleteKey("sessao_perfil");
        PlayerPrefs.Save();

        Debug.Log($"[Cadastro] {removidas} conta(s) apagada(s) da lista de alunos. " +
                  $"Contas que nunca concluiram uma fase nao constam nessa lista: " +
                  $"para zerar tudo, use Edit > Clear All PlayerPrefs no Editor.");
    }
}
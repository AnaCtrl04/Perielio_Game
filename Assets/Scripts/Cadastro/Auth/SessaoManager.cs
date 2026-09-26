using UnityEngine;

/// <summary>
/// Gerencia a sessao do usuario logado.
/// Singleton persistente — acessivel de qualquer cena do jogo.
/// Substitui o uso de PlayerPrefs para dados de sessao.
/// </summary>
public class SessaoManager : MonoBehaviour
{
    // ─── SINGLETON ───────────────────────────────────────────────────────────
    public static SessaoManager Instancia { get; private set; }

    // ─── DADOS DA SESSAO ─────────────────────────────────────────────────────
    public TabelaUsuario UsuarioAtual { get; private set; }
    public bool EstaLogado => UsuarioAtual != null;
    public bool EhProfessor => UsuarioAtual?.Perfil == "professor";
    public bool EhAluno => UsuarioAtual?.Perfil == "aluno";

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    // ════════════════════════════════════════════════════════════════════════
    // SESSAO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Inicia a sessao com o usuario autenticado.
    /// </summary>
    public void IniciarSessao(TabelaUsuario usuario)
    {
        UsuarioAtual = usuario;
        Debug.Log($"[Sessao] Sessao iniciada: {usuario.Perfil} '{usuario.Nome}' (ID: {usuario.Id})");
    }

    /// <summary>
    /// Encerra a sessao e volta para a tela de cadastro.
    /// </summary>
    public void Sair()
    {
        Debug.Log($"[Sessao] Sessao encerrada: '{UsuarioAtual?.Nome}'");
        UsuarioAtual = null;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Cadastro");
    }
}

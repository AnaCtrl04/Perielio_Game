using UnityEngine;

/// <summary>
/// Ponto unico de leitura e gravacao do progresso do jogo Perielio.
/// Todas as chaves sao por aluno, no formato:
///     aluno_{nome}_m{mundo}_f{fase}_concluida
///     aluno_{nome}_m{mundo}_f{fase}_tentativas
/// Isso garante que cada perfil veja apenas o proprio avanco (RN01, RN05).
/// Classe estatica: nao precisa estar em nenhum GameObject.
/// </summary>
public static class ProgressoAluno
{
    public const string CHAVE_SESSAO = "sessao_usuario";
    public const string CHAVE_LISTA  = "lista_alunos";

    // ════════════════════════════════════════════════════════════════════
    // USUARIO ATUAL
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Nome do usuario logado. Usa o SessaoManager e, se ele nao existir
    /// (Play direto numa cena de fase), cai na chave gravada pelo Cadastro.
    /// </summary>
    public static string NomeAtual()
    {
        if (SessaoManager.Instancia != null &&
            SessaoManager.Instancia.UsuarioAtual != null &&
            !string.IsNullOrEmpty(SessaoManager.Instancia.UsuarioAtual.Nome))
            return SessaoManager.Instancia.UsuarioAtual.Nome;

        return PlayerPrefs.GetString(CHAVE_SESSAO, "");
    }

    private static string Prefixo(string nome, int mundo, int fase)
    {
        return $"aluno_{nome}_m{mundo}_f{fase}";
    }

    // ════════════════════════════════════════════════════════════════════
    // LEITURA
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A fase foi concluida pelo aluno logado? Sem aluno logado, retorna false
    /// — assim um perfil novo nunca herda o progresso de outro.
    /// </summary>
    public static bool FaseConcluida(int mundo, int fase)
    {
        string nome = NomeAtual();
        if (string.IsNullOrEmpty(nome)) return false;

        return PlayerPrefs.GetInt(Prefixo(nome, mundo, fase) + "_concluida", 0) == 1;
    }

    public static int TentativasDaFase(int mundo, int fase)
    {
        string nome = NomeAtual();
        if (string.IsNullOrEmpty(nome)) return 0;

        return PlayerPrefs.GetInt(Prefixo(nome, mundo, fase) + "_tentativas", 0);
    }

    // ── Consultas por nome (usadas pelo ranking do Modo Professor) ──

    /// <summary>A fase foi concluida por ESTE aluno (informado pelo nome)?</summary>
    public static bool FaseConcluidaDe(string nome, int mundo, int fase)
    {
        if (string.IsNullOrEmpty(nome)) return false;
        return PlayerPrefs.GetInt(Prefixo(nome, mundo, fase) + "_concluida", 0) == 1;
    }

    /// <summary>Tentativas gastas por ESTE aluno na fase.</summary>
    public static int TentativasDe(string nome, int mundo, int fase)
    {
        if (string.IsNullOrEmpty(nome)) return 0;
        return PlayerPrefs.GetInt(Prefixo(nome, mundo, fase) + "_tentativas", 0);
    }

    /// <summary>Nomes de todos os alunos que ja concluiram alguma fase.</summary>
    public static string[] ListaDeAlunos()
    {
        string lista = PlayerPrefs.GetString(CHAVE_LISTA, "");
        if (string.IsNullOrEmpty(lista)) return new string[0];

        return lista.Split(new char[] { ';', ',' },
                           System.StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>Quantas fases o aluno concluiu num mundo (RF07).</summary>
    public static int FasesConcluidasNoMundo(int mundo, int totalFases = 3)
    {
        int total = 0;
        for (int f = 1; f <= totalFases; f++)
            if (FaseConcluida(mundo, f)) total++;
        return total;
    }

    /// <summary>O mundo inteiro foi concluido? Usado para liberar o proximo (RN08).</summary>
    public static bool MundoConcluido(int mundo, int totalFases = 3)
    {
        return FasesConcluidasNoMundo(mundo, totalFases) >= totalFases;
    }

    /// <summary>
    /// O verbete do glossario ligado a essa fase ja foi liberado (RN10).
    /// Hoje equivale a ter concluido a fase.
    /// </summary>
    public static bool VerbeteDesbloqueado(int mundo, int fase)
    {
        return FaseConcluida(mundo, fase);
    }

    // ════════════════════════════════════════════════════════════════════
    // GRAVACAO
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Marca a fase como concluida para o aluno logado.
    /// Nunca regride: o numero de tentativas so e atualizado se melhorou (RN05).
    /// </summary>
    public static bool SalvarConclusao(int mundo, int fase, int tentativas)
    {
        string nome = NomeAtual();
        if (string.IsNullOrEmpty(nome))
        {
            Debug.LogWarning("[Progresso] Nenhum usuario logado — nada foi salvo. " +
                             "Inicie o jogo pela cena 'Cadastro'.");
            return false;
        }

        string prefixo = Prefixo(nome, mundo, fase);

        PlayerPrefs.SetInt(prefixo + "_concluida", 1);

        int anterior = PlayerPrefs.GetInt(prefixo + "_tentativas", int.MaxValue);
        if (tentativas < anterior)
            PlayerPrefs.SetInt(prefixo + "_tentativas", tentativas);

        RegistrarNaLista(nome);
        PlayerPrefs.Save();

        Debug.Log($"[Progresso] '{nome}' concluiu Mundo {mundo} / Fase {fase} " +
                  $"em {tentativas} tentativa(s).");
        return true;
    }

    /// <summary>Mantem a lista de alunos usada pelo ranking do Modo Professor.</summary>
    private static void RegistrarNaLista(string nome)
    {
        string lista = PlayerPrefs.GetString(CHAVE_LISTA, "");

        foreach (string n in lista.Split(';'))
            if (n == nome) return;

        lista = string.IsNullOrEmpty(nome) ? lista
              : string.IsNullOrEmpty(lista) ? nome
              : lista + ";" + nome;

        PlayerPrefs.SetString(CHAVE_LISTA, lista);
    }

    // ════════════════════════════════════════════════════════════════════
    // LIMPEZA (testes)
    // ════════════════════════════════════════════════════════════════════

    /// <summary>Apaga o progresso de um aluno especifico.</summary>
    public static void LimparAluno(string nome, int mundos = 3, int fases = 3)
    {
        if (string.IsNullOrEmpty(nome)) return;

        for (int m = 1; m <= mundos; m++)
            for (int f = 1; f <= fases; f++)
            {
                PlayerPrefs.DeleteKey(Prefixo(nome, m, f) + "_concluida");
                PlayerPrefs.DeleteKey(Prefixo(nome, m, f) + "_tentativas");
            }

        PlayerPrefs.Save();
        Debug.Log($"[Progresso] Progresso de '{nome}' apagado.");
    }

    /// <summary>Apaga o progresso de TODOS os alunos e as chaves globais antigas.</summary>
    public static void LimparTudo(int mundos = 3, int fases = 3)
    {
        string lista = PlayerPrefs.GetString(CHAVE_LISTA, "");

        foreach (string nome in lista.Split(';'))
            if (!string.IsNullOrEmpty(nome))
                LimparAluno(nome, mundos, fases);

        // chaves globais da versao antiga
        for (int m = 1; m <= mundos; m++)
            for (int f = 1; f <= fases; f++)
            {
                PlayerPrefs.DeleteKey($"fase_{m}_{f}_concluida");
                PlayerPrefs.DeleteKey($"fase_{m}_{f}_tentativas");
                PlayerPrefs.DeleteKey($"fase_{m}_{f}_estrelas");
                PlayerPrefs.DeleteKey($"glossario_{m}_{f}");
            }

        PlayerPrefs.DeleteKey(CHAVE_LISTA);
        PlayerPrefs.Save();
        Debug.Log("[Progresso] Todo o progresso foi apagado.");
    }
}
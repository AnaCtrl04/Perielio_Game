using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Repositorio de usuarios — operacoes de cadastro, login e consulta.
/// Usado pelo CadastroController para integrar a tela de cadastro com o SQLite.
/// RF01 — Cadastro de Perfil.
/// RF14 — Autenticacao do Professor.
/// </summary>
public class UsuarioRepository
{
    // ─── SINGLETON SIMPLES ───────────────────────────────────────────────────
    private static UsuarioRepository _instancia;
    public static UsuarioRepository Instancia
    {
        get
        {
            if (_instancia == null)
                _instancia = new UsuarioRepository();
            return _instancia;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // CADASTRO (RF01)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cadastra um novo usuario no banco.
    /// Retorna true se cadastrou com sucesso, false se o nome ja existe.
    /// </summary>
    public bool Cadastrar(string nome, string senha, string perfil = "")
    {
        try
        {
            var db = DatabaseManager.Instancia.Conexao;

            // Verifica se ja existe
            var existente = db.Table<TabelaUsuario>()
                              .Where(u => u.Nome == nome && u.Perfil == perfil)
                              .FirstOrDefault();

            if (existente != null)
            {
                Debug.LogWarning($"[Usuario] '{nome}' ja cadastrado como {perfil}.");
                return false;
            }

            // Insere o novo usuario
            var usuario = new TabelaUsuario
            {
                Nome          = nome,
                Senha         = senha,
                Perfil        = perfil,
                DataCadastro  = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            db.Insert(usuario);
            Debug.Log($"[Usuario] {perfil} '{nome}' cadastrado. ID: {usuario.Id}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Usuario] Erro ao cadastrar: {e.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // LOGIN (RF01.1, RF01.2, RF14)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Valida login do usuario.
    /// Retorna o objeto TabelaUsuario se credenciais corretas, null se invalido.
    /// </summary>
    public TabelaUsuario Login(string nome, string senha, string perfil)
    {
        try
        {
            var db = DatabaseManager.Instancia.Conexao;

            var usuario = db.Table<TabelaUsuario>()
                            .Where(u => u.Nome == nome
                                     && u.Senha == senha
                                     && u.Perfil == perfil)
                            .FirstOrDefault();

            if (usuario != null)
                Debug.Log($"[Usuario] Login bem-sucedido: {perfil} '{nome}' (ID: {usuario.Id})");
            else
                Debug.LogWarning($"[Usuario] Login invalido para '{nome}' ({perfil}).");

            return usuario;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Usuario] Erro ao fazer login: {e.Message}");
            return null;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // CONSULTAS (RF15)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Retorna todos os alunos cadastrados.
    /// </summary>
    public List<TabelaUsuario> ListarAlunos()
    {
        try
        {
            var db = DatabaseManager.Instancia.Conexao;
            return db.Table<TabelaUsuario>()
                     .Where(u => u.Perfil == "aluno")
                     .ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Usuario] Erro ao listar alunos: {e.Message}");
            return new List<TabelaUsuario>();
        }
    }

    /// <summary>
    /// Busca um usuario pelo ID.
    /// </summary>
    public TabelaUsuario BuscarPorId(int id)
    {
        try
        {
            return DatabaseManager.Instancia.Conexao
                                  .Table<TabelaUsuario>()
                                  .Where(u => u.Id == id)
                                  .FirstOrDefault();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Usuario] Erro ao buscar por ID: {e.Message}");
            return null;
        }
    }
}

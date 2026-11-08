using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Aplica hash SHA-256 nas senhas antes de grava-las (RNF08).
/// Cada usuario recebe um salt aleatorio proprio, de modo que duas senhas
/// iguais nunca produzam o mesmo hash — isso inviabiliza a comparacao com
/// tabelas de hashes pre-calculados.
/// Classe estatica: nao precisa estar em nenhum GameObject.
/// </summary>
public static class SegurancaSenha
{
    /// <summary>Gera um salt aleatorio de 16 bytes, codificado em Base64.</summary>
    public static string GerarSalt()
    {
        byte[] bytes = new byte[16];

        using (var gerador = RandomNumberGenerator.Create())
            gerador.GetBytes(bytes);

        return Convert.ToBase64String(bytes);
    }

    /// <summary>Calcula o hash SHA-256 da combinacao salt + senha.</summary>
    public static string GerarHash(string senha, string salt)
    {
        using (SHA256 algoritmo = SHA256.Create())
        {
            byte[] entrada = Encoding.UTF8.GetBytes(salt + senha);
            byte[] hash    = algoritmo.ComputeHash(entrada);
            return Convert.ToBase64String(hash);
        }
    }

    /// <summary>
    /// Confere a senha digitada contra o hash armazenado.
    /// A comparacao percorre a string inteira mesmo depois de encontrar uma
    /// diferenca, evitando que o tempo de resposta revele quantos caracteres
    /// ja conferem.
    /// </summary>
    public static bool Conferir(string senhaDigitada, string salt, string hashArmazenado)
    {
        if (string.IsNullOrEmpty(hashArmazenado)) return false;

        string hashDigitado = GerarHash(senhaDigitada, salt);

        if (hashDigitado.Length != hashArmazenado.Length) return false;

        int diferenca = 0;
        for (int i = 0; i < hashDigitado.Length; i++)
            diferenca |= hashDigitado[i] ^ hashArmazenado[i];

        return diferenca == 0;
    }
}

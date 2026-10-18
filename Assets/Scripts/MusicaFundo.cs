using UnityEngine;

/// <summary>
/// Toca a musica de fundo do Perielio em loop continuo.
/// Sobrevive a troca de cenas (DontDestroyOnLoad), entao a musica
/// nao reinicia quando o jogador navega entre as telas.
/// Basta existir UM objeto com este script, na primeira cena do jogo.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicaFundo : MonoBehaviour
{
    private static MusicaFundo instancia;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.35f;

    private void Awake()
    {
        // Se ja existe uma musica tocando, destroi esta copia
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        AudioSource fonte = GetComponent<AudioSource>();
        fonte.loop        = true;
        fonte.playOnAwake = true;
        fonte.volume      = volume;

        if (!fonte.isPlaying) fonte.Play();
    }
}

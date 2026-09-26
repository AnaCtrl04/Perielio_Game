using UnityEngine;

/// <summary>
/// Componente base para todos os corpos celestes do jogo Periélio:
/// planetas, estrelas, buracos negros, asteroides.
/// Define a massa e os indicadores visuais de campo gravitacional (RF06).
/// Anexe este script a qualquer objeto que deve exercer gravidade.
/// </summary>
public class CorpoCeleste : MonoBehaviour
{
    // ─── DADOS DO CORPO CELESTE ──────────────────────────────────────────────
    [Header("Propriedades Físicas")]
    [SerializeField] private string nomeCorpo = "Planeta";
    [SerializeField] private float massa = 10f;
    [SerializeField] private TipoCorpo tipo = TipoCorpo.Planeta;

    [Header("Indicadores Visuais de Campo Gravitacional (RF06)")]
    [SerializeField] private bool exibirCampoGravitacional = true;
    [SerializeField] private int quantidadeAneis = 3;
    [SerializeField] private float raioBaseAnel = 1.5f;
    [SerializeField] private float espacamentoAneis = 0.8f;
    [SerializeField] private Color corCampo = new Color(0.4f, 0.7f, 1f, 0.3f);

    [Header("Dica Pedagógica da Fase (RN09)")]
    [SerializeField][TextArea(2, 4)]
    private string dicaPedagogica = "Quanto maior a massa, maior a força gravitacional!";

    // ─── PROPRIEDADES PÚBLICAS ───────────────────────────────────────────────
    public float Massa => massa;
    public string NomeCorpo => nomeCorpo;
    public TipoCorpo Tipo => tipo;
    public string DicaPedagogica => dicaPedagogica;

    // ─── COMPONENTES ─────────────────────────────────────────────────────────
    private GameObject[] aneisGravitacionais;

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        if (exibirCampoGravitacional)
            CriarIndicadoresCampo();
    }

    private void OnValidate()
    {
        // Garante que a massa nunca seja zero ou negativa
        massa = Mathf.Max(0.1f, massa);
    }

    // ════════════════════════════════════════════════════════════════════════
    // INDICADORES VISUAIS DE CAMPO GRAVITACIONAL (RF06)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cria anéis ao redor do planeta para indicar o campo gravitacional.
    /// Anéis maiores = campo mais fraco. Intensidade de cor proporcional à massa.
    /// </summary>
    private void CriarIndicadoresCampo()
    {
        aneisGravitacionais = new GameObject[quantidadeAneis];

        for (int i = 0; i < quantidadeAneis; i++)
        {
            GameObject anel = new GameObject($"CampoGrav_{i}");
            anel.transform.SetParent(transform);
            anel.transform.localPosition = Vector3.zero;

            // LineRenderer para desenhar o anel circular
            LineRenderer lr = anel.AddComponent<LineRenderer>();
            float raio = raioBaseAnel + (i * espacamentoAneis);
            float opacidade = corCampo.a * (1f - (i / (float)quantidadeAneis));
            float largura = 0.04f * (1f - (i * 0.2f));

            lr.startWidth = largura;
            lr.endWidth = largura;
            lr.startColor = new Color(corCampo.r, corCampo.g, corCampo.b, opacidade);
            lr.endColor = new Color(corCampo.r, corCampo.g, corCampo.b, opacidade);
            lr.useWorldSpace = false;
            lr.loop = true;

            // Desenha círculo
            int segmentos = 64;
            lr.positionCount = segmentos;
            for (int j = 0; j < segmentos; j++)
            {
                float angulo = (j / (float)segmentos) * 2f * Mathf.PI;
                lr.SetPosition(j, new Vector3(
                    Mathf.Cos(angulo) * raio,
                    Mathf.Sin(angulo) * raio,
                    0f
                ));
            }

            aneisGravitacionais[i] = anel;
        }
    }

    /// <summary>
    /// Atualiza a opacidade dos anéis com base na distância do foguete.
    /// Quanto mais perto o foguete, mais intenso o campo visual.
    /// </summary>
    public void AtualizarIntensidadeCampo(float distanciaFoguete)
    {
        if (aneisGravitacionais == null) return;

        float intensidade = Mathf.Clamp01(1f - (distanciaFoguete / 10f));

        for (int i = 0; i < aneisGravitacionais.Length; i++)
        {
            LineRenderer lr = aneisGravitacionais[i].GetComponent<LineRenderer>();
            if (lr == null) continue;

            float opacidade = corCampo.a * intensidade * (1f - (i / (float)quantidadeAneis));
            lr.startColor = new Color(corCampo.r, corCampo.g, corCampo.b, opacidade);
            lr.endColor = new Color(corCampo.r, corCampo.g, corCampo.b, opacidade);
        }
    }
}

// ════════════════════════════════════════════════════════════════════════
// ENUM DE TIPOS
// ════════════════════════════════════════════════════════════════════════

public enum TipoCorpo
{
    Planeta,
    Estrela,
    BuracoNegro,
    Asteroide,
    NebulosaProtoplanetaria
}
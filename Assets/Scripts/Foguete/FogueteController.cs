using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class FogueteController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider sliderMassa;
    [SerializeField] private Slider sliderAngulo;
    [SerializeField] private TextMeshProUGUI txtMassa;
    [SerializeField] private TextMeshProUGUI txtAngulo;
    [SerializeField] private Button btnLancar;
    [SerializeField] private Button btnReiniciar;
    [SerializeField] private GameObject painelControles;
    [SerializeField] private Transform setaDirecao;

    [Header("Faixa de Massa (kg)")]
    [SerializeField] private float massaMinima = 1f;
    [SerializeField] private float massaMaxima = 100f;

    [Header("Velocidade de Lancamento (u/s)")]
    [Tooltip("Velocidade com a MASSA MINIMA — o foguete mais leve, que sai mais rapido.")]
    [SerializeField] private float velocidadeMaxima = 8f;
    [Tooltip("Velocidade com a MASSA MAXIMA — o foguete mais pesado, que sai mais devagar.\n" +
             "Mantenha acima de zero: se o foguete sair quase parado, o angulo escolhido " +
             "deixa de influenciar a trajetoria e a gravidade decide tudo sozinha.")]
    [SerializeField] private float velocidadeMinima = 3f;

    [Header("Faixa do Angulo (graus)")]
    [Tooltip("Limitar a faixa aumenta a precisao do slider no celular. " +
             "Ex.: foguete no canto inferior esquerdo mirando a direita -> 0 a 90.")]
    [SerializeField] private float anguloMinimo = 0f;
    [SerializeField] private float anguloMaximo = 360f;

    [Header("Orientacao do Sprite")]
    [Tooltip("Correcao entre o desenho do sprite e o eixo +X do mundo.\n" +
             "Sprite com a ponta desenhada para CIMA  -> -90\n" +
             "Sprite com a ponta desenhada para a DIREITA -> 0")]
    [SerializeField] private float offsetSpriteGraus = -90f;

    [Header("Rastro de Trajetoria")]
    [SerializeField] private int maxPontosRastro = 500;
    [SerializeField] private Color corRastro = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private float larguraRastro = 0.05f;

    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private bool foiLancado = false;
    private Vector3 posicaoInicial;
    private int contadorPontos = 0;

    public bool FoiLancado => foiLancado;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        // Salva posicao ANTES de qualquer movimento
        posicaoInicial = transform.position;

        Debug.Log($"[Foguete] Posicao inicial salva: {posicaoInicial}");

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        ConfigurarRastro();
        ConfigurarUI();

        if (sliderMassa  != null) sliderMassa.onValueChanged.AddListener(AtualizarMassa);
        if (sliderAngulo != null) sliderAngulo.onValueChanged.AddListener(AtualizarAngulo);

        if (btnReiniciar != null)
            btnReiniciar.gameObject.SetActive(false);

        if (sliderMassa  != null) AtualizarMassa(sliderMassa.value);
        if (sliderAngulo != null) AtualizarAngulo(sliderAngulo.value);
    }

    private void Update()
    {
        if (!foiLancado) return;

        // Durante o voo a ponta acompanha a direcao real do movimento.
        // O aluno ve o foguete "virar" ao passar perto de um planeta,
        // o que torna visivel o efeito da atracao gravitacional.
        if (rb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            float anguloMovimento = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            AplicarRotacaoVisual(anguloMovimento);
        }

        DesenharRastro();
    }

    // ════════════════════════════════════════════════════════════════════════
    // ORIENTACAO — ponto unico de verdade
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gira o foguete para que a PONTA aponte para o angulo informado,
    /// medido em graus a partir do eixo +X (direita), no sentido anti-horario.
    /// O offsetSpriteGraus compensa a orientacao em que o sprite foi desenhado.
    /// Tanto a mira quanto o voo passam por aqui, entao visual e trajetoria
    /// nunca podem divergir.
    /// </summary>
    private void AplicarRotacaoVisual(float anguloMundoGraus)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, anguloMundoGraus + offsetSpriteGraus);
    }

    /// <summary>Vetor unitario correspondente ao angulo, em coordenadas de mundo.</summary>
    private Vector2 DirecaoDoAngulo(float anguloGraus)
    {
        float rad = anguloGraus * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    /// <summary>
    /// Converte a massa escolhida na velocidade de saida.
    /// Massa minima -> velocidadeMaxima; massa maxima -> velocidadeMinima.
    /// A relacao "mais pesado, mais devagar" vem de um empurrao de intensidade
    /// fixa: o mesmo impulso acelera menos um corpo de maior inercia.
    /// Os dois extremos sao definidos no Inspector para que nenhuma parte do
    /// slider produza um lancamento inutil.
    /// </summary>
    private float VelocidadeParaMassa(float massa)
    {
        float t = Mathf.InverseLerp(massaMinima, massaMaxima, massa);
        return Mathf.Lerp(velocidadeMaxima, velocidadeMinima, t);
    }

    // ════════════════════════════════════════════════════════════════════════
    // CONFIGURACAO
    // ════════════════════════════════════════════════════════════════════════

    private void ConfigurarRastro()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = larguraRastro;
        lineRenderer.endWidth = larguraRastro * 0.5f;
        lineRenderer.startColor = corRastro;
        lineRenderer.endColor = new Color(corRastro.r, corRastro.g, corRastro.b, 0f);
        lineRenderer.useWorldSpace = true;
    }

    private void ConfigurarUI()
    {
        if (sliderMassa != null)
        {
            sliderMassa.minValue = massaMinima;
            sliderMassa.maxValue = massaMaxima;
            sliderMassa.value    = massaMinima;
        }

        if (sliderAngulo != null)
        {
            sliderAngulo.minValue = anguloMinimo;
            sliderAngulo.maxValue = anguloMaximo;
            sliderAngulo.value    = anguloMinimo;
        }
    }

    private void AtualizarMassa(float valor)
    {
        rb.mass = valor;

        if (txtMassa != null)
            txtMassa.text = $"Massa: {valor:F1} kg";
    }

    private void AtualizarAngulo(float valor)
    {
        if (txtAngulo != null)
            txtAngulo.text = $"Angulo: {valor:F0}°";

        // O FOGUETE gira junto com o slider: a ponta mostra para onde ele vai.
        AplicarRotacaoVisual(valor);

        if (setaDirecao != null)
            setaDirecao.rotation = Quaternion.Euler(0f, 0f, valor);
    }

    // ════════════════════════════════════════════════════════════════════════
    // LANCAMENTO
    // ════════════════════════════════════════════════════════════════════════

    public void Lancar()
    {
        if (foiLancado) return;

        foiLancado = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        float angulo = sliderAngulo != null ? sliderAngulo.value : 0f;

        // Mesma funcao que orientou o sprite na mira: a direcao do voo
        // e, por construcao, a direcao para onde a ponta esta apontando.
        Vector2 direcao    = DirecaoDoAngulo(angulo);
        float   velocidade = VelocidadeParaMassa(rb.mass);

        rb.linearVelocity = direcao * velocidade;

        Debug.Log($"[Foguete] Lancado! Angulo: {angulo:F0}° | Direcao: {direcao} | " +
                  $"Massa: {rb.mass:F1} kg | Velocidade inicial: {velocidade:F2} u/s");

        if (painelControles != null) painelControles.SetActive(false);
        if (btnReiniciar    != null) btnReiniciar.gameObject.SetActive(true);

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, transform.position);
        contadorPontos = 1;
    }

    // ════════════════════════════════════════════════════════════════════════
    // RASTRO
    // ════════════════════════════════════════════════════════════════════════

    private void DesenharRastro()
    {
        if (contadorPontos >= maxPontosRastro) return;

        contadorPontos++;
        lineRenderer.positionCount = contadorPontos;
        lineRenderer.SetPosition(contadorPontos - 1, transform.position);
    }

    private void LimparRastro()
    {
        lineRenderer.positionCount = 0;
        contadorPontos = 0;
    }

    // ════════════════════════════════════════════════════════════════════════
    // PARADA E REINICIO
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Congela o foguete imediatamente, sem reposicionar.</summary>
    private void Congelar()
    {
        foiLancado = false;
        rb.linearVelocity  = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Reiniciar()
    {
        Debug.Log($"[Foguete] Reiniciando para posicao: {posicaoInicial}");

        Congelar();

        transform.position = posicaoInicial;

        LimparRastro();

        // Restaura UI e reaplica a mira a partir do slider,
        // garantindo que a ponta volte a apontar para o angulo escolhido.
        if (painelControles != null) painelControles.SetActive(true);
        if (btnReiniciar    != null) btnReiniciar.gameObject.SetActive(false);

        if (sliderAngulo != null) AtualizarAngulo(sliderAngulo.value);
        if (sliderMassa  != null) AtualizarMassa(sliderMassa.value);

        Debug.Log($"[Foguete] Reiniciado! Posicao atual: {transform.position}");
    }

    // ════════════════════════════════════════════════════════════════════════
    // COLISOES
    // ════════════════════════════════════════════════════════════════════════

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!foiLancado) return;

        if (collision.gameObject.CompareTag("Planeta") ||
            collision.gameObject.CompareTag("BuracoNegro"))
        {
            Debug.Log($"[Foguete] Colisao com: {collision.gameObject.name}");

            // Para na hora: sem isso o foguete continua sendo puxado pela
            // gravidade durante os segundos do painel de feedback.
            Congelar();

            GerenciadorFase.Instancia?.RegistrarColisao();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!foiLancado) return;

        if (other.CompareTag("Destino"))
        {
            Debug.Log("[Foguete] Chegou ao destino!");

            Congelar();

            GerenciadorFase.Instancia?.RegistrarChegada();
        }
    }
} 
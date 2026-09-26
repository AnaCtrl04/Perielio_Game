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

    [Header("Configuracoes de Lancamento")]
    [SerializeField] private float impulsoInicial = 5f;
    [SerializeField] private float massaMinima = 1f;
    [SerializeField] private float massaMaxima = 100f;

    [Header("Rastro de Trajetoria")]
    [SerializeField] private int maxPontosRastro = 500;
    [SerializeField] private Color corRastro = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private float larguraRastro = 0.05f;

    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private bool foiLancado = false;
    private Vector3 posicaoInicial;
    private Quaternion rotacaoInicial;
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
        rotacaoInicial = transform.rotation;

        Debug.Log($"[Foguete] Posicao inicial salva: {posicaoInicial}");

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        ConfigurarRastro();
        ConfigurarUI();

        sliderMassa.onValueChanged.AddListener(AtualizarMassa);
        sliderAngulo.onValueChanged.AddListener(AtualizarAngulo);

        if (btnReiniciar != null)
            btnReiniciar.gameObject.SetActive(false);

        AtualizarMassa(sliderMassa.value);
        AtualizarAngulo(sliderAngulo.value);
    }

    private void Update()
    {
        if (!foiLancado) return;

        if (rb.linearVelocity != Vector2.zero)
        {
            float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo - 90f);
        }

        DesenharRastro();
    }

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
        sliderMassa.minValue = massaMinima;
        sliderMassa.maxValue = massaMaxima;
        sliderMassa.value = massaMinima;

        sliderAngulo.minValue = 0f;
        sliderAngulo.maxValue = 360f;
        sliderAngulo.value = 90f;
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

        if (setaDirecao != null)
            setaDirecao.rotation = Quaternion.Euler(0f, 0f, valor);
    }

    public void Lancar()
    {
        if (foiLancado) return;

        foiLancado = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        float anguloRad = sliderAngulo.value * Mathf.Deg2Rad;
        Vector2 direcao = new Vector2(
            Mathf.Cos(anguloRad),
            Mathf.Sin(anguloRad)
        ).normalized;

        float forca = impulsoInicial;
        rb.AddForce(direcao * forca, ForceMode2D.Impulse);

        Debug.Log($"[Foguete] Lancado! Angulo: {sliderAngulo.value}° | Massa: {rb.mass}kg | Forca: {forca}");

        if (painelControles != null) painelControles.SetActive(false);
        if (btnReiniciar != null)    btnReiniciar.gameObject.SetActive(true);

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, transform.position);
        contadorPontos = 1;
    }

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

    public void Reiniciar()
    {
        Debug.Log($"[Foguete] Reiniciando para posicao: {posicaoInicial}");

        foiLancado = false;

        // Para completamente o rigidbody
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Volta para posicao inicial
        transform.position = posicaoInicial;
        transform.rotation = rotacaoInicial;

        // Limpa rastro
        LimparRastro();

        // Restaura UI
        if (painelControles != null) painelControles.SetActive(true);
        if (btnReiniciar != null)    btnReiniciar.gameObject.SetActive(false);

        Debug.Log($"[Foguete] Reiniciado! Posicao atual: {transform.position}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!foiLancado) return;

        if (collision.gameObject.CompareTag("Planeta") ||
            collision.gameObject.CompareTag("BuracoNegro"))
        {
            Debug.Log($"[Foguete] Colisao com: {collision.gameObject.name}");
            foiLancado = false;
            GerenciadorFase.Instancia?.RegistrarColisao();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!foiLancado) return;

        if (other.CompareTag("Destino"))
        {
            Debug.Log("[Foguete] Chegou ao destino!");
            GerenciadorFase.Instancia?.RegistrarChegada();
        }
    }
}
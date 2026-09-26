using UnityEngine;

/// <summary>
/// Simula a Lei da Gravitação Universal de Newton entre o foguete e todos os planetas.
/// F = G * (M1 * M2) / d²
/// G é ajustado para escala de tela, mantendo a proporcionalidade pedagógica:
/// planetas com maior massa SEMPRE atraem mais que planetas menores (RN06).
/// Deve ser anexado ao foguete.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GravidadeController : MonoBehaviour
{
    // ─── CONFIGURAÇÕES ───────────────────────────────────────────────────────
    [Header("Constante Gravitacional")]
    [Tooltip("Ajuste para escala de tela. Valores maiores = gravidade mais intensa.")]
    [SerializeField] private float constanteG = 6.674f;

    [Header("Limites de Segurança")]
    [SerializeField] private float distanciaMinima = 0.5f;  // Evita força infinita
    [SerializeField] private float forcaMaxima = 50f;        // Evita comportamento errático (RNF01)

    [Header("Debug")]
    [SerializeField] private bool mostrarVetoresNoEditor = true;

    // ─── COMPONENTES ─────────────────────────────────────────────────────────
    private Rigidbody2D rbFoguete;
    private FogueteController fogueteController;

    // ─── PLANETAS NA CENA ────────────────────────────────────────────────────
    private CorpoCeleste[] corposCelestes;

    // ════════════════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        rbFoguete = GetComponent<Rigidbody2D>();
        fogueteController = GetComponent<FogueteController>();
    }

    private void Start()
    {
        // Busca todos os corpos celestes na cena ao iniciar a fase
        AtualizarCorposCelestes();
    }

    /// <summary>
    /// FixedUpdate garante física consistente em diferentes hardware (RNF01).
    /// A gravidade só age quando o foguete está em voo.
    /// </summary>
    private void FixedUpdate()
    {
        if (fogueteController == null || !fogueteController.FoiLancado) return;

        AplicarGravidadeTotal();
    }

    // ════════════════════════════════════════════════════════════════════════
    // SIMULAÇÃO GRAVITACIONAL
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Aplica a força gravitacional de TODOS os corpos celestes ao foguete.
    /// Cada planeta contribui com uma força proporcional à sua massa e
    /// inversamente proporcional ao quadrado da distância (Lei de Newton).
    /// </summary>
    private void AplicarGravidadeTotal()
    {
        if (corposCelestes == null) return;

        foreach (CorpoCeleste corpo in corposCelestes)
        {
            if (corpo == null || !corpo.gameObject.activeInHierarchy) continue;

            // Vetor da direção: do foguete até o planeta
            Vector2 direcao = (Vector2)(corpo.transform.position - transform.position);
            float distancia = direcao.magnitude;

            // Garante distância mínima para evitar força infinita
            if (distancia < distanciaMinima) continue;

            // F = G * (M_planeta * M_foguete) / d²
            float forca = constanteG * (corpo.Massa * rbFoguete.mass) / (distancia * distancia);

            // Limita a força máxima para evitar comportamento errático (RNF01)
            forca = Mathf.Min(forca, forcaMaxima);

            // Aplica força na direção do planeta
            rbFoguete.AddForce(direcao.normalized * forca);

            // Debug visual no editor
            if (mostrarVetoresNoEditor)
                Debug.DrawLine(transform.position, corpo.transform.position,
                    Color.Lerp(Color.green, Color.red, forca / forcaMaxima));
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // UTILITÁRIOS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Busca todos os CorpoCeleste ativos na cena.
    /// Chame novamente se planetas aparecerem/desaparecerem durante a fase.
    /// </summary>
    public void AtualizarCorposCelestes()
    {
        corposCelestes = FindObjectsByType<CorpoCeleste>(FindObjectsSortMode.None);
        Debug.Log($"[Gravidade] {corposCelestes.Length} corpos celestes encontrados na cena.");
    }

    /// <summary>
    /// Retorna a força gravitacional que um planeta exerceria sobre o foguete
    /// na posição atual. Usado para exibir os indicadores visuais de campo (RF06).
    /// </summary>
    public float CalcularForcaNoPonto(CorpoCeleste corpo, Vector2 ponto)
    {
        float distancia = Vector2.Distance(ponto, corpo.transform.position);
        distancia = Mathf.Max(distancia, distanciaMinima);
        return constanteG * (corpo.Massa * rbFoguete.mass) / (distancia * distancia);
    }
}

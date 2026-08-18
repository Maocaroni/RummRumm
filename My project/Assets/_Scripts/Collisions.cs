using UnityEngine;

public class Collisions : MonoBehaviour
{
    [Header("Rango de Escala Aleatoria para Cilindros")]
    [SerializeField] private float minYScale = 1.0f;
    [SerializeField] private float maxYScale = 6.0f;

    private Renderer myRenderer;
    private bool isBlack = false;

    private void Awake()
    {
        myRenderer = GetComponent<Renderer>();
        if (myRenderer == null)
        {
            myRenderer = GetComponentInChildren<Renderer>();
        }
    }

    // Interacciones físicas con el carro (Cilindros)
    private void OnCollisionEnter(Collision collision)
    {
        // Filtro estricto: Solo continua si el objeto colisionador es el Player
        if (!collision.gameObject.CompareTag("Player")) return;

        // Interacción 3: Este cilindro cambia su tamaño Y a un valor aleatorio
        if (gameObject.CompareTag("ScaleCylinder"))
        {
            Vector3 currentScale = transform.localScale;
            float randomYScale = Random.Range(minYScale, maxYScale);
            transform.localScale = new Vector3(currentScale.x, randomYScale, currentScale.z);
        }
        // Interacción 4: Este cilindro cambia solo a VERDE
        else if (gameObject.CompareTag("ColorCylinder"))
        {
            if (myRenderer != null)
            {
                myRenderer.material.color = Color.green;
            }
        }
    }

    // Interacciones al salir de las zonas (Cubos Triggers)
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Renderer playerRenderer = other.GetComponent<Renderer>();
        if (playerRenderer == null)
        {
            playerRenderer = other.GetComponentInChildren<Renderer>();
        }

        if (playerRenderer == null) return;

        // Interacción 1: Este cubo alterna el color del carro entre Negro y Rojo
        if (gameObject.CompareTag("ColorToggle"))
        {
            playerRenderer.material.color = isBlack ? Color.red : Color.black;
            isBlack = !isBlack;
        }
        // Interacción 2: Este cubo transparente cambia el color del carro a uno aleatorio
        else if (gameObject.CompareTag("ColorRandomZone"))
        {
            playerRenderer.material.color = GetRandomColor();
        }
    }

    private Color GetRandomColor()
    {
        return Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
    }
}
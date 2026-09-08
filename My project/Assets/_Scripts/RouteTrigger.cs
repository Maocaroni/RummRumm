using UnityEngine;

public class RouteTrigger : MonoBehaviour
{
    [Header("Tipo de Trigger")]
    [Tooltip("Usa 1 para Camino A, 2 para Camino B, o 3 si es la Meta Final")]
    [SerializeField] private int triggerType = 1; 

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el que entra es el carro (por tag "Player" o teniendo el componente CarMovement)
        if (other.CompareTag("Player") || other.GetComponent<CarMovement>() != null)
        {
            if (GameManager.instance != null)
            {
                if (triggerType == 1 || triggerType == 2)
                {
                    // Notifica al GameManager que pasó por la Ruta 1 o 2
                    GameManager.instance.NotifyRoutePassed(triggerType);
                    gameObject.SetActive(false); // Desactiva este trigger para que no se repita
                }
                else if (triggerType == 3)
                {
                    // Intenta activar la victoria en el GameManager
                    GameManager.instance.ReachVictoryZone();
                }
            }
        }
    }
}
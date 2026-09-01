using UnityEngine;

public class ItemAndPlatform : MonoBehaviour
{
    public enum ItemType { PlatformBoost, CottonSlow, Coin, CherryBoost }
    [Header("Tipo de Objeto")]
    public ItemType itemType;

    [Header("Configuración de Plataforma (Impulso Físico)")]
    [SerializeField] private float platformBoostForce = 20f;  // Fuerza instantánea de golpe

    [Header("Configuración de Cereza (Velocidad Temporal)")]
    [SerializeField] private float cherrySpeedMultiplier = 1.5f; // Multiplicador de velocidad (ej. 1.5 para 50% más rápido)
    [SerializeField] private float cherryDuration = 3f;          // Cuántos segundos dura el efecto

    [Header("Configuración de Otros Ítems")]
    [SerializeField] private float slowMultiplier = 0.4f;      // Multiplicador para ralentizar (ej. 0.4)
    [SerializeField] private int coinValue = 1;               // Valor de la moneda

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        CarMovement carMovement = other.GetComponent<CarMovement>();
        if (carMovement == null) return;

        switch (itemType)
        {
            case ItemType.PlatformBoost:
                // Impulso físico seco e instantáneo
                carMovement.ApplyForceBoost(platformBoostForce);
                Destroy(gameObject);
                break;

            case ItemType.CottonSlow:
                // Ralentiza el carro durante 1 segundo
                carMovement.ApplySpeedBoost(slowMultiplier, 1f);
                // Destroy(gameObject); // Descomenta si quieres que desaparezca
                break;

            case ItemType.Coin:
                Debug.Log("Moneda recolectada. Puntos sumados: " + coinValue);
                Destroy(gameObject);
                break;

            case ItemType.CherryBoost:
                // Aumento sostenido de velocidad durante el tiempo configurado
                carMovement.ApplySpeedBoost(cherrySpeedMultiplier, cherryDuration);
                Destroy(gameObject);
                break;
        }
    }
}
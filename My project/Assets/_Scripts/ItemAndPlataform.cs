using UnityEngine;

public class ItemAndPlatform : MonoBehaviour
{
    public enum ItemType { PlatformBoost, CottonSlow, Coin, CherryBoost }
    [Header("Tipo de Objeto")]
    public ItemType itemType;

    [Header("Configuración de Valores")]
    [SerializeField] private float boostMultiplier = 1.5f; // Multiplicador para el impulso (ej. 1.5 o 2.0)
    [SerializeField] private float slowMultiplier = 0.4f;   // Multiplicador para ralentizar (ej. 0.4)
    [SerializeField] private int coinValue = 1;            // Valor de la moneda

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        CarMovement carMovement = other.GetComponent<CarMovement>();
        if (carMovement == null) return;

        switch (itemType)
        {
            case ItemType.PlatformBoost:
                // Impulso de plataforma por 1 segundo
                carMovement.ApplySpeedBoost(boostMultiplier, 1f);
                break;

            case ItemType.CottonSlow:
                // Algodón de azúcar: ralentiza bastante durante 1 segundo
                carMovement.ApplySpeedBoost(slowMultiplier, 1f);
                // Si quieres que el algodón desaparezca al tocarlo, descomenta la siguiente línea:
                // Destroy(gameObject);
                break;

            case ItemType.Coin:
                // Lógica de monedas (aquí puedes sumar a tu GameManager o sistema de puntos)
                Debug.Log("Moneda recolectada. Puntos sumados: " + coinValue);
                Destroy(gameObject);
                break;

            case ItemType.CherryBoost:
                // Cerezas: aumento de velocidad por 3 segundos
                carMovement.ApplySpeedBoost(boostMultiplier, 3f);
                Destroy(gameObject);
                break;
        }
    }
}
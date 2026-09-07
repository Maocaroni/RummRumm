using UnityEngine;

public class ItemAndPlatform : MonoBehaviour
{
    public enum ItemType { PlatformBoost, CottonSlow, Coin, CherryBoost }
    [Header("Tipo de Objeto")]
    public ItemType itemType;

    [Header("Configuración de Plataforma (Impulso Físico)")]
    [SerializeField] private float platformBoostForce = 20f;  
    [SerializeField] private float upwardBoostForce = 5f;    

    [Header("Configuración de Cereza (Velocidad Temporal)")]
    [SerializeField] private float cherrySpeedMultiplier = 1.5f; 
    [SerializeField] private float cherryDuration = 3f;           

    [Header("Configuración de Algodón (Zona de Slow)")]
    [SerializeField] private float cottonDrag = 8f;             
    [SerializeField] private float lingerDuration = 2f;         
    [SerializeField] private float immunityDuration = 5f;       

    [Header("Configuración de Otros Ítems")]
    [SerializeField] private int coinValue = 1;                 

    private void OnTriggerEnter(Collider other)
    {
        CarMovement carMovement = other.GetComponentInParent<CarMovement>();
        
        if (carMovement == null || !other.CompareTag("Player")) return;

        switch (itemType)
        {
            case ItemType.PlatformBoost:
                Vector3 boostDirection = (other.transform.forward + Vector3.up * 0.3f).normalized;
                carMovement.ApplyForceBoost(boostDirection * platformBoostForce);
                break;

            case ItemType.CottonSlow:
                carMovement.EnterCottonZone(cottonDrag);
                break;

            case ItemType.Coin:
                if (GameManager.instance != null)
                {
                    GameManager.instance.AddScore(coinValue);
                }
                Destroy(gameObject);
                break;

            case ItemType.CherryBoost:
                carMovement.ApplySpeedBoost(cherrySpeedMultiplier, cherryDuration);
                Destroy(gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (itemType != ItemType.CottonSlow) return;

        CarMovement carMovement = other.GetComponentInParent<CarMovement>();
        if (carMovement != null && other.CompareTag("Player"))
        {
            carMovement.ExitCottonZone(lingerDuration, immunityDuration);
        }
    }
}
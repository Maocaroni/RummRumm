using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI carNameText;
    public Scrollbar speedScrollbar;
    public Scrollbar brakeScrollbar;
    public Scrollbar angleScrollbar;

    [Header("3D Preview Setup")]
    public Transform previewSpawnPoint; 
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private float previewScale = 5f; 

    [Header("Camera Settings for Menu")]
    public Camera gameCamera; // Arrastra aquí la cámara principal
    public Transform menuCameraPositionPoint; // El objeto vacío ubicado frente al carro del menú

    [Header("References")]
    public CameraControler cam; // El script CameraControler que está en la cámara
    public CarSo[] cars;
    public Transform initialPos;

    private CarSo selectedCar;
    private GameObject currentCarPreview;

    [Header("Max Stats for UI")]
    [SerializeField] private float maxScrollbar = 2000;
    [SerializeField] private float maxScrollbarAngle = 60;

    private int carIndex;

    // Usamos Awake para forzar la posición de la cámara antes de que empiece cualquier otra cosa
    private void Awake()
    {
        if (gameCamera != null && menuCameraPositionPoint != null)
        {
            gameCamera.transform.position = menuCameraPositionPoint.position;
            gameCamera.transform.rotation = menuCameraPositionPoint.rotation;
        }

        if (cam != null)
        {
            cam.enabled = false; // Apagamos el script de seguimiento para que no mueva la cámara
        }
    }

    private void Start()
    {
        carIndex = 0;
        UIUpdate();
    }

    private void Update()
    {
        if (currentCarPreview != null)
        {
            currentCarPreview.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    public void UIUpdate()
    {
        selectedCar = cars[carIndex];
        carNameText.text = selectedCar.carName;
        speedScrollbar.size = selectedCar.speed / maxScrollbar;
        brakeScrollbar.size = selectedCar.brakeForce / maxScrollbar;
        angleScrollbar.size = selectedCar.angle / maxScrollbarAngle;

        UpdateCarPreview();
    }

    private void UpdateCarPreview()
    {
        if (currentCarPreview != null)
        {
            Destroy(currentCarPreview);
        }

        if (previewSpawnPoint != null && selectedCar.carPrefab != null)
        {
            currentCarPreview = Instantiate(selectedCar.carPrefab, previewSpawnPoint.position, previewSpawnPoint.rotation);
            currentCarPreview.transform.localScale = Vector3.one * previewScale;

            // --- ELIMINAR FÍSICAS Y CONTROLES EN EL MENÚ ---
            Rigidbody rb = currentCarPreview.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            foreach (var childRb in currentCarPreview.GetComponentsInChildren<Rigidbody>())
            {
                Destroy(childRb);
            }

            foreach (var wheel in currentCarPreview.GetComponentsInChildren<WheelCollider>())
            {
                Destroy(wheel);
            }

            CarMovement movementScript = currentCarPreview.GetComponent<CarMovement>();
            if (movementScript != null) Destroy(movementScript);

            InputController inputScript = currentCarPreview.GetComponent<InputController>();
            if (inputScript != null) Destroy(inputScript);

            // --- CENTRAR EL PIVOTE ---
            Renderer[] renderers = currentCarPreview.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds carBounds = renderers[0].bounds;
                foreach (Renderer rend in renderers)
                {
                    carBounds.Encapsulate(rend.bounds);
                }
                Vector3 centerOffset = currentCarPreview.transform.position - carBounds.center;
                currentCarPreview.transform.position += centerOffset;
            }

            currentCarPreview.transform.SetParent(previewSpawnPoint);
        }
    }

    public void CharacterChange(bool isButtonRight)
    {
        if (isButtonRight)
        {
            carIndex = (carIndex + 1) % cars.Length;
        }
        else
        {
            carIndex = (carIndex - 1 + cars.Length) % cars.Length;
        }

        UIUpdate();
    }

    public void SelectCar()
    {
        // 1. Instancia el carro real con físicas en la pista
        GameObject prefabSelected = Instantiate(selectedCar.carPrefab, initialPos.position, initialPos.rotation);
        
        // 2. Asigna el objetivo al script de la cámara y vuelve a activarlo para que empiece a seguir al carro
        if (cam != null)
        {
            cam.target = prefabSelected.transform;
            cam.enabled = true; 
        }
        
        // 3. Limpia la vista previa y apaga el menú
        if (currentCarPreview != null)
        {
            Destroy(currentCarPreview);
        }
        
        gameObject.SetActive(false); 
    }
}
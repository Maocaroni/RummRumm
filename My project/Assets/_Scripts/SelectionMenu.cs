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

    [Header("Canvas Settings")]
    [SerializeField] private Canvas mainCanvas;

    [Header("3D Preview Setup")]
    public Transform previewSpawnPoint; 
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private float previewScale = 5f; 

    [Header("Camera Settings for Menu")]
    public Camera gameCamera; 
    public Transform menuCameraPositionPoint; 

    [Header("References")]
    public CameraControler cam; 
    public CarSo[] cars;
    public Transform initialPos;
    [SerializeField] private GameObject hudPanel; 

    private CarSo selectedCar;
    private GameObject currentCarPreview;

    [Header("Max Stats for UI")]
    [SerializeField] private float maxScrollbar = 2000;
    [SerializeField] private float maxScrollbarAngle = 60;

    private int carIndex;

    private void Awake()
    {
        if (gameCamera != null && menuCameraPositionPoint != null)
        {
            gameCamera.transform.position = menuCameraPositionPoint.position;
            gameCamera.transform.rotation = menuCameraPositionPoint.rotation;
        }

        if (cam != null)
        {
            cam.enabled = false; 
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
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
        if (mainCanvas != null)
        {
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        GameObject prefabSelected = Instantiate(selectedCar.carPrefab, initialPos.position, initialPos.rotation);
        
        if (cam != null)
        {
            cam.target = prefabSelected.transform;
            cam.enabled = true; 
        }
        
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        if (currentCarPreview != null)
        {
            Destroy(currentCarPreview);
        }
        
        gameObject.SetActive(false); 
    }
}
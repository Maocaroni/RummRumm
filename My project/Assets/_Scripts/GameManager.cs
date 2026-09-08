using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Paneles del Canvas")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("UI & Puntaje")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private int currentScore = 0;

    [Header("UI & Velocímetro")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private float speedMultiplier = 3.6f;

    [Header("Referencias de Reinicio")]
    [SerializeField] private Transform startPosition; 
    [SerializeField] private GameObject playerCar;

    [Header("Configuración de Rutas y Victoria")]
    [SerializeField] private GameObject victoryColliderObject; // El objeto con el Box Collider de la meta
    private bool routeA_Passed = false;
    private bool routeB_Passed = false;

    private bool isPaused = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel) pausePanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false); 

        // Aseguramos que la meta empiece desactivada hasta que elijan ruta
        if (victoryColliderObject) victoryColliderObject.SetActive(false);

        UpdateScoreUI();
        
        if (playerCar == null)
        {
            FindPlayerCar();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        UpdateSpeedUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    private void UpdateSpeedUI()
    {
        if (speedText == null) return;

        if (playerCar != null)
        {
            Rigidbody rb = playerCar.GetComponentInChildren<Rigidbody>();
            
            if (rb != null)
            {
                float currentSpeed = rb.velocity.magnitude * speedMultiplier;
                speedText.text = Mathf.Round(currentSpeed) + " km/h";
            }
            else
            {
                speedText.text = "0 km/h";
            }
        }
        else
        {
            speedText.text = ""; 
        }
    }

    public void SetPlayerCar(GameObject newCar)
    {
        playerCar = newCar;
    }

    public void FindPlayerCar()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCar = player;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (playerCar == null)
        {
            FindPlayerCar();
        }

        if (pausePanel) pausePanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(true);
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel) pausePanel.SetActive(true);
        if (hudPanel) hudPanel.SetActive(false);
    }

    public void TriggerVictory()
    {
        Time.timeScale = 0f;
        
        if (victoryPanel) victoryPanel.SetActive(true);
        if (hudPanel) hudPanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Aseguramos que el panel de victoria se desactive al reiniciar
        if (victoryPanel) victoryPanel.SetActive(false);

        // Reiniciamos las rutas al reiniciar el juego por si acaso
        routeA_Passed = false;
        routeB_Passed = false;
        if (victoryColliderObject) victoryColliderObject.SetActive(false);

        if (playerCar == null)
        {
            FindPlayerCar();
        }

        if (playerCar != null && startPosition != null)
        {
            Rigidbody rb = playerCar.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            playerCar.transform.position = startPosition.position;
            playerCar.transform.rotation = startPosition.rotation;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        ResumeGame();
    }

    public void GoToMainMenu(string menuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // --- SISTEMA DE RUTAS Y VICTORIA INTELIGENTE ---

    public void NotifyRoutePassed(int routeID)
    {
        if (routeID == 1) routeA_Passed = true;
        if (routeID == 2) routeB_Passed = true;

        // Si pasa por cualquiera de los dos caminos, activamos el collider de la meta final
        if ((routeA_Passed || routeB_Passed) && victoryColliderObject != null)
        {
            victoryColliderObject.SetActive(true);
            Debug.Log("¡Ruta completada! Meta de victoria habilitada.");
        }
    }

    public void ReachVictoryZone()
    {
        // Solo da la victoria si previamente pasó por el camino A o el camino B
        if (routeA_Passed || routeB_Passed)
        {
            TriggerVictory();
        }
        else
        {
            Debug.LogWarning("¡Intentaste cruzar la meta sin pasar por ningún camino válido!");
        }
    }
}
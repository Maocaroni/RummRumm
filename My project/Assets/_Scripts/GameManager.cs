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
            // Mientras no haya carro seleccionado, el texto se queda en blanco en lugar de decir "SIN CARRO"
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
        
        // Si el carro no está asignado, lo busca de inmediato en la escena
        if (playerCar == null)
        {
            FindPlayerCar();
        }

        if (pausePanel) pausePanel.SetActive(false);
        if (victoryPanel) pausePanel.SetActive(false);
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
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Paneles del Canvas")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Referencias de Reinicio")]
    [SerializeField] private Transform startPosition; 
    private GameObject playerCar;

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
        // Inicializa el estado sin encender el HUD para que no aparezca en la selección de carros
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel) pausePanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false); // Apagado por defecto

        FindPlayerCar();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
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

        if (playerCar == null)
        {
            FindPlayerCar();
        }

        if (playerCar != null && startPosition != null)
        {
            Rigidbody rb = playerCar.GetComponent<Rigidbody>();
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
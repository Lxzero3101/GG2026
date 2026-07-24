using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Won, Lost }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("UI References")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Result Messages")]
    [SerializeField] private string winMessage = "YOU WIN";
    [SerializeField] private string loseMessage = "YOU LOSE";

    [Header("Gameplay References")]
    [SerializeField] private MonoBehaviour pullSystem;   // drag PullSystem here
    [SerializeField] private MonoBehaviour pointerMovement; // drag PointerMovement here

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public bool IsGameOver()
    {
        return CurrentState != GameState.Playing;
    }

    public void PlayerWin()
    {
        if (IsGameOver()) return;
        CurrentState = GameState.Won;
        EndGame(winMessage);
    }

    public void EnemyWin()
    {
        if (IsGameOver()) return;
        CurrentState = GameState.Lost;
        EndGame(loseMessage);
    }

    private void EndGame(string message)
    {
        StopGameplay();

        if (resultText != null)
            resultText.text = message;

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    private void StopGameplay()
    {
        if (pullSystem != null)
            pullSystem.enabled = false;

        if (pointerMovement != null)
            pointerMovement.enabled = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
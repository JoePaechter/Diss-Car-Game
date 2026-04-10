using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject UI_gameOver;
    public MonoBehaviour[] gamplaySystems;

    private InputAction restartAction;
    public CoinSpawner coinSpawner;
    public ScoreTracker scoreTracker;


    bool GameOver;


    public void Awake()
    {
        restartAction = new InputAction(
            type: InputActionType.Button, binding: "<XRController>{LeftHand}/primaryButton");
        
    }

    private void OnEnable()
    {
        restartAction.Enable();
    }

    private void OnDisable()
    {
        restartAction.Disable();
    }

    private void Update()
    {
        if(GameOver && restartAction.WasPressedThisFrame())
        {
            ResetGame();
        }
    }


    public void endGame()
    {
        Debug.Log("END GAME CALLED");
        if (GameOver) return;
        GameOver = true;

        foreach (var system in gamplaySystems)
            system.enabled = false;

        UI_gameOver.SetActive(true);

       
    }
    
    public void startGame()
    {
        UI_gameOver.SetActive(false);
        GameOver = false;

        foreach (var system in gamplaySystems)
        {
            system.enabled = true;

        }
        
    }

    public void ResetGame()
    {
        
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);


        // reset score
        scoreTracker.resetScore();
        // reset kart 

        startGame();
        // reset coins
        coinSpawner.resetCoinsAndPowerups();
    }

    
}

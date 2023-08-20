using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayGlobals : MonoBehaviour
{


    public static bool gameOver = false;
    CinemachineVirtualCamera cameraComponent;
    GameObject playerInstance;
    public GameObject playerPrefab;
    public static int PlayerHealth;
    public static int EnemiesDown;
    public static int PlayerAmmo;

    [Header("UI")]
    public Text keyText;
    public Text ammoText;
    public Text enemyText;
    public Text healthText;
    public Text lostText;
    public Button playButton;
    public Button menuButton;

    float destroyHeight;

    int optionSelected = 0;


    public static void resetGlobals()
    {
        PlayerHealth = 100;
        EnemiesDown = 0;
        PlayerAmmo = 100;
        gameOver = false;
    }

    void Start()
    {

        StartNewGame();

    }

    public void ReloadScene()
    {
        optionSelected = 1;
        Destroy(this);
    }

    public void GoBackMenu()
    {
        optionSelected = 2;
        Destroy(this);
    }

    void StartNewGame()
    {
        resetGlobals();
        optionSelected = 0;
        ammoText.text = PlayerAmmo + "";
        enemyText.text = EnemiesDown + "";
        playerInstance = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        FindVirtualCamera(playerInstance);
        PlayerController.OnHealthUpdated += UpdateHealthUI;
        PlayerController.OnScoreUpdated += UpdateAmmoUI;
        PlayerController.OnDeadPlayer += DeadPlayer;
        EnemyController.OnInstanceCreatedEnemy += EnemyInstanceCreated;
        EnemyController.OnDeadEnemy += DeadEnemy;
        lostText.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(false);

    }

    void FindVirtualCamera(GameObject player)
    {
        
        cameraComponent = FindObjectOfType<CinemachineVirtualCamera>();

        if (cameraComponent != null)
        {
            cameraComponent.Follow = player.transform;
            cameraComponent.LookAt = player.transform;
        }
        else
        {
            Debug.LogWarning("Camera component not found in the scene.");
        }
    }

    private void OnDestroy()
    {
        PlayerController.OnHealthUpdated -= UpdateHealthUI;
        PlayerController.OnScoreUpdated -= UpdateAmmoUI;
        PlayerController.OnDeadPlayer -= DeadPlayer;
        EnemyController.OnInstanceCreatedEnemy -= EnemyInstanceCreated;
        EnemyController.OnDeadEnemy -= DeadEnemy;

        if (optionSelected == 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        } else
        {
            SceneManager.LoadScene("MainMenu");
        }
        
    }


    private void UpdateHealthUI(int newHealth)
    {
        if (newHealth < 0)
        {
            newHealth = 0;
        }
        healthText.text = newHealth.ToString();
    }


    private void UpdateAmmoUI(int newScore)
    {
        ammoText.text = newScore.ToString();
    }


    private void DeadPlayer()
    {
        lostText.gameObject.SetActive(true);
        playButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
        onPlayerDying();
    }

    void onPlayerDying()
    {
        cameraComponent.Follow = null;
        cameraComponent.LookAt = null;
        destroyHeight = Terrain.activeTerrain.SampleHeight(playerInstance.transform.position) - 5;
        Collider[] colliders = playerInstance.transform.GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            Destroy(c);
        }

        InvokeRepeating("IntoGround", 1, 1.0f);
    }


    void IntoGround()
    {
        if (playerInstance != null) { 
            playerInstance.transform.Translate(0, -0.001f, 0);
            if (playerInstance.transform.position.y < destroyHeight)
            {
                Destroy(playerInstance);
            }
        }
    }


    private void DeadEnemy()
    {
        EnemiesDown++;
        enemyText.text = EnemiesDown + "";
    }

    private void EnemyInstanceCreated(EnemyController enemy)
    {
        enemy.target = playerInstance;
    }

    

}

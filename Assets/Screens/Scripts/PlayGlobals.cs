using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayGlobals : MonoBehaviour
{

    public GameObject playerPrefab;
    public GameObject playerInstance;
    public static int PlayerHealth;
    public static int EnemiesDown;
    public static int PlayerAmmo;

    public Text ammoText;


    public static void resetGlobals()
    {
        PlayerHealth = 100;
        EnemiesDown = 0;
        PlayerAmmo = 100;
    }

    void Start()
    {
        
        resetGlobals();
        playerInstance = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        FindVirtualCamera(playerInstance);
        PlayerController.OnScoreUpdated += UpdateScoreUI;
        EnemyController.OnInstanceCreatedEnemy += EnemyInstanceCreated;

    }

    void FindVirtualCamera(GameObject player)
    {
        
        CinemachineVirtualCamera cameraComponent = FindObjectOfType<CinemachineVirtualCamera>();

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
        PlayerController.OnScoreUpdated -= UpdateScoreUI;
        EnemyController.OnInstanceCreatedEnemy -= EnemyInstanceCreated;
    }

    private void UpdateScoreUI(int newScore)
    {
        ammoText.text = newScore.ToString();
    }

    private void EnemyInstanceCreated(EnemyController enemy)
    {
        enemy.target = playerInstance;
    }

    

}

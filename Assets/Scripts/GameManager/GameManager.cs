using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    PlayerData savedData;
    public float[] valueChanges = new float[4];
    public int totalLeftPlayerSacrifices;
    public bool tapeUnlocked;

    [SerializeField] private Animator transitionAnimatior;
    void Awake()
    {
        SceneManager.sceneLoaded += applySavedData;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        savedData = SaveSystem.LoadGame();
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (savedData != null)
            {
                MainMenuManager mainMenuManager = GameObject.FindAnyObjectByType<MainMenuManager>().GetComponent<MainMenuManager>();
                mainMenuManager.continueButton = true;
                mainMenuManager.prevPlayerData = savedData;
            }

            if (savedData == null)
            {
                changePlayerValue(0, GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().dashForceMag);
                changePlayerValue(1, GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().jumpForceMag);
                changePlayerValue(2, GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().paperCutDamage);
                changePlayerValue(3, GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().tapeStickDuration);
                updateSacrificePlayer(GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().totalLeftPlayerSacrifices); 
            }
        }

    }
    

    void OnDestroy()
    {
        // clean up to avoid double-calls if reloaded manually
        SceneManager.sceneLoaded -= applySavedData;
    }

    public void applySavedData(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 0)
        {
            valueChanges = savedData.valueChanges;
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            if (valueChanges[0] != 0)
                player.dashForceMag = valueChanges[0];

            if (valueChanges[1] != 0)
                player.jumpForceMag = valueChanges[1];

            if (valueChanges[2] != 0)
                player.paperCutDamage = valueChanges[2];

            if (valueChanges[3] != 0)
                player.tapeStickDuration = valueChanges[3];


            player.totalLeftPlayerSacrifices = savedData.totalLeftPlayerSacrifices;

            Debug.Log(valueChanges);
        }
    }
    public void updateSacrificePlayer(int update)
    {
        totalLeftPlayerSacrifices = update;
    }
    //0:- dash, 1 accordian, 2 paper cut, 3 tape
    public void changePlayerValue(int ability, float newValue)
    {
        Debug.Log(ability);
        valueChanges[ability] = newValue;
        Debug.Log(valueChanges);
    }
    public void updateTapeStatus(bool unlocked)
    {
        tapeUnlocked = unlocked;
    }

    void Start()
    {
        transitionAnimatior.SetTrigger("fadeOut");
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame(this);
    }

    public void setTimeScale(float timeScale = 1f)
    {
        Time.timeScale = timeScale;
    }

    public void resetTimeScale()
    {
        Time.timeScale = 1f;
    }


    public void sceneChange(int buildIndex)
    {
        SaveGame();
        transitionAnimatior.SetTrigger("fadeIn");

        StartCoroutine(loadSceneDelay(1f, buildIndex));
        Debug.Log("changing scene");
    }

    private IEnumerator loadSceneDelay(float delay, int buildIndex)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(buildIndex);
        transitionAnimatior.SetTrigger("fadeOut");


    }
}

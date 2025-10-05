using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    public bool continueButton = false;
    public PlayerData prevPlayerData;

    public TextMeshProUGUI playerButtonText;

    void Awake()
    {
        if (continueButton == true)
        {
            playerButtonText.text = "continue";
        }
    }

    public void playButton()
    {
        if (continueButton == true)
        {
            //TODO: implement scene transitions
            SceneManager.LoadScene(prevPlayerData.sceneIndex);
        }
        else
        {
            SceneManager.LoadScene(1);
        }

    }

    public void settingsButton()
    {

    }

    public void exitButton()
    {
        Application.Quit();
    }

}

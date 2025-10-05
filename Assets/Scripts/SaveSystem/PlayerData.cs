using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerData
{
    /// <summary>
    /// 1 - 1A
    /// 2 - 1B
    /// 3 - 2A
    /// 4 - 2B
    /// 5 - 2C
    /// </summary>
    public int sceneIndex;
    public float[] valueChanges = new float[4];

    public bool tapeUnlocked = false;
    public int totalLeftPlayerSacrifices;

    public PlayerData(GameManager gameManager)
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        valueChanges = gameManager.valueChanges;
        tapeUnlocked = gameManager.tapeUnlocked;
        totalLeftPlayerSacrifices = gameManager.totalLeftPlayerSacrifices;
    }
}

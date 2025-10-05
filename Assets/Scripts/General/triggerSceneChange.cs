using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class triggerSceneChange : MonoBehaviour
{
    [SerializeField] private int newScene;
    void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.Instance.sceneChange(newScene);
    }
}

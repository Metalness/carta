using UnityEngine;

public class AnimationEventSoundTrigger : MonoBehaviour
{
    [SerializeField] private SoundManager.SoundType soundType1;
    [SerializeField] private float vol1 = 1f;
    [SerializeField] private SoundManager.SoundType soundType2;
    [SerializeField] private float vol2 = 1f;

    [SerializeField] private SoundManager.SoundType soundType3;
    [SerializeField] private float vol3 = 1f;

    void triggerEvent1()
    {
        SoundManager.PlaySound(soundType1, vol1);
    }

    void triggerEvent2()
    {
        SoundManager.PlaySound(soundType2,vol2);
    }

    void triggerEvent3()
    {
        SoundManager.PlaySound(soundType3,vol3);
    }
    
}

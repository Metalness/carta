using UnityEngine;
using System;


[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private AudioSource audioSource;

    public enum SoundType
    {
        paperSliding,
        paperCut,
        bladeSlide,
        pressImpact,
        staplerShoot,
        staplerJump,
        playerStaplerHit
    }
    public static SoundManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        //instance.audioSource.PlayOneShot(instance.soundList[((int)sound)], volume);

        AudioClip[] clips = instance.soundList[(int)sound].sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(randomClip, volume);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);

        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}


[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [SerializeField] public AudioClip[] sounds;
}

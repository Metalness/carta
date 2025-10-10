using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Serializable]
    public class MusicTrack
    {
        public string name;
        public AudioClip intro;
        public AudioClip loop;
        public AudioClip outro;
    }

    [SerializeField] private MusicTrack[] tracks;
    private AudioSource musicSource;
    private MusicTrack currentTrack;
    private Coroutine playRoutine;

    public static MusicManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = GetComponent<AudioSource>();
        musicSource.loop = false;
    }

    public void PlayImmediate(string trackName, float delay = 0f)
    {
        InterruptAndPlay(PlayTrackRoutine(trackName, delay, false));
    }

    public void PlayTrackWithOutro(string nextTrackName)
    {
        InterruptAndPlay(PlayTrackRoutine(nextTrackName, 0, true));
    }

    public void CrossfadeImmediate(string nextTrackName, float fadeTime = 1f)
    {
        InterruptAndPlay(CrossfadeRoutine(nextTrackName, fadeTime));
    }

    public void PlayOnce(string trackName)
    {
        InterruptAndPlay(PlayOnceRoutine(trackName));
    }

    public void StopMusic(bool playOutro = true)
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = StartCoroutine(StopRoutine(playOutro));
    }

    private void InterruptAndPlay(IEnumerator routine)
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = StartCoroutine(routine);
    }

    private IEnumerator PlayTrackRoutine(string trackName, float delay, bool waitForOutro)
    {
        yield return new WaitForSeconds(delay);

        MusicTrack nextTrack = Array.Find(tracks, t => t.name == trackName);
        if (nextTrack == null) yield break;

        if (currentTrack != null && waitForOutro)
        {
            // wait for current loop to finish
            while (musicSource.isPlaying && musicSource.clip == currentTrack.loop)
                yield return null;

            // out
            if (currentTrack.outro != null)
            {
                musicSource.clip = currentTrack.outro;
                musicSource.loop = false;
                musicSource.Play();
                yield return new WaitForSeconds(musicSource.clip.length);
            }
        }

        currentTrack = nextTrack;

        // intro
        if (currentTrack.intro != null)
        {
            musicSource.clip = currentTrack.intro;
            musicSource.loop = false;
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }

        // continous loop
        if (currentTrack.loop != null)
        {
            musicSource.clip = currentTrack.loop;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    private IEnumerator PlayOnceRoutine(string trackName)
    {
        MusicTrack nextTrack = Array.Find(tracks, t => t.name == trackName);
        if (nextTrack == null) yield break;

        currentTrack = nextTrack;

        // intro
        if (currentTrack.intro != null)
        {
            musicSource.clip = currentTrack.intro;
            musicSource.loop = false;
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }

        // loop single
        if (currentTrack.loop != null)
        {
            musicSource.clip = currentTrack.loop;
            musicSource.loop = false;
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }

        // outro
        if (currentTrack.outro != null)
        {
            musicSource.clip = currentTrack.outro;
            musicSource.loop = false;
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }

        currentTrack = null;
        musicSource.Stop();
    }

    private IEnumerator CrossfadeRoutine(string trackName, float fadeTime)
    {
        MusicTrack nextTrack = Array.Find(tracks, t => t.name == trackName);
        if (nextTrack == null) yield break;

        float startVol = musicSource.volume;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVol;

        currentTrack = nextTrack;

        // intro
        if (currentTrack.intro != null)
        {
            musicSource.clip = currentTrack.intro;
            musicSource.loop = false;
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }

        // loop
        if (currentTrack.loop != null)
        {
            musicSource.clip = currentTrack.loop;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    private IEnumerator StopRoutine(bool playOutro)
    {
        if (currentTrack != null && playOutro && currentTrack.outro != null)
        {
            musicSource.clip = currentTrack.outro;
            musicSource.loop = false;
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }

        musicSource.Stop();
        currentTrack = null;
    }
}

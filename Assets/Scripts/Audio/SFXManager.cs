using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    [SerializeField][Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource[] audioSources;
    private int nextIndex;

    private const int PoolSize = 5;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSources = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].loop = false;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource source = audioSources[nextIndex];
        source.clip = clip;
        source.volume = volume;
        source.Play();

        nextIndex = (nextIndex + 1) % PoolSize;
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;

        AudioSource source = audioSources[nextIndex];
        source.clip = clip;
        source.volume = volume * Mathf.Clamp01(volumeScale);
        source.Play();

        nextIndex = (nextIndex + 1) % PoolSize;
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
    }
}

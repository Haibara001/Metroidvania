using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    [SerializeField] private AudioClip bgmClip;
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private bool playOnAwake = true;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;

        if (playOnAwake && bgmClip != null)
        {
            audioSource.Play();
        }
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        audioSource.volume = volume;
    }

    public void ChangeBGM(AudioClip newClip)
    {
        if (audioSource.clip == newClip) return;
        audioSource.clip = newClip;
        audioSource.Play();
    }

    public void Stop() => audioSource.Stop();
    public void Play() => audioSource.Play();
}

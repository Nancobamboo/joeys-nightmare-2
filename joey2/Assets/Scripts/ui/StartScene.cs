using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StartScene : MonoBehaviour
{
    [SerializeField] private string bgmPath = "Audio/Music/True Self Dream";
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.6f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
    }

    private void Start()
    {
        var clip = Resources.Load<AudioClip>(bgmPath);
        if (clip == null)
        {
            Debug.LogError($"StartScene: Failed to load BGM clip from path '{bgmPath}'");
            return;
        }

        audioSource.clip = clip;
        audioSource.volume = bgmVolume;
        audioSource.Play();
    }

    /// <summary>
    /// Load Battle scene - called from button OnClick event
    /// </summary>
    public void LoadBattleScene()
    {
        Debug.Log("StartScene: LoadBattleScene called!");
        
        // Reset level to 1 when starting new game from menu
        PData.Instance.currentLevel = 1;
        
        SceneLoader.Instance.LoadScene("Battle");
    }
}

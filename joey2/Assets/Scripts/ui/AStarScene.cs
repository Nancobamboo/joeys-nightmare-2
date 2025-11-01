using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AStarScene : MonoBehaviour
{
    [SerializeField] private string audioPath = "Audio/SFX/Main/AStar";
    [SerializeField] private string nextSceneName = "Start";
    
    [Header("Scan Line Effect")]
    [SerializeField] private bool enableScanLine = true;
    [SerializeField] private float scanSpeed = 400f;
    [SerializeField] private Color scanLineColor = new Color(1f, 1f, 1f, 0.8f);
    
    private AudioSource audioSource;
    private AudioClip audioClip;
    private ScanLineEffect scanLineEffect;

    private void Start()
    {
        // Initialize scan line effect
        if (enableScanLine)
        {
            InitializeScanLine();
        }
        
        // Load audio clip from Resources
        audioClip = Resources.Load<AudioClip>(audioPath);
        
        if (audioClip == null)
        {
            Debug.LogError($"AStarScene: Failed to load audio clip from path '{audioPath}'");
            // If audio fails to load, switch to next scene immediately
            StartCoroutine(SwitchSceneAfterDelay(0.1f));
            return;
        }

        // Create AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.playOnAwake = false;
        
        // Play audio immediately
        audioSource.Play();
        
        Debug.Log($"AStarScene: Playing audio clip '{audioClip.name}' (length: {audioClip.length}s)");
        
        // Wait for audio to finish, then switch scene
        StartCoroutine(SwitchSceneAfterDelay(audioClip.length));
    }

    private void InitializeScanLine()
    {
        GameObject scanLineObj = new GameObject("ScanLineEffect");
        scanLineObj.transform.SetParent(transform);
        scanLineEffect = scanLineObj.AddComponent<ScanLineEffect>();
        
        // Force white color for scan line
        scanLineColor = Color.white;
        
        // Configure scan line properties before initialization
        scanLineEffect.ScanSpeed = scanSpeed;
        scanLineEffect.ScanLineColor = scanLineColor;
        
        // Manually initialize to ensure properties are set
        scanLineEffect.Initialize();
    }

    private IEnumerator SwitchSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"AStarScene: Switching to scene '{nextSceneName}'");
        SceneLoader.Instance.LoadScene(nextSceneName);
    }
}


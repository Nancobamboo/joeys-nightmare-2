// Scripts/CardEffects/SFX.cs
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class SFX : MonoSingleton<SFX>
{
    // 音效缓存字典，key是音效路径，value是AudioClip
    private static Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

    /// <summary>
    /// 播放音效（带缓存）
    /// </summary>
    /// <param name="audioPath">音效资源路径（Resources目录下的相对路径）</param>
    /// <param name="volume">音量（0-1）</param>
    /// <param name="startTime">从哪个时间点开始播放（秒）</param>
    /// <param name="is3D">是否为3D音效（false=2D音效）</param>
    /// <param name="position">3D音效的播放位置（仅在is3D=true时有效）</param>
    public static void PlayAudio(string audioPath, float volume = 1.0f, float startTime = 0f)
    {
        // 如果缓存中没有，则加载并缓存
        if (!audioClipCache.ContainsKey(audioPath))
        {
            AudioClip clip = Resources.Load<AudioClip>(audioPath);
            if (clip == null)
            {
                Debug.LogWarning($"SFX: Failed to load audio from '{audioPath}'");
                return;
            }
            audioClipCache[audioPath] = clip;
            // Debug.Log($"SFX: Loaded and cached audio '{audioPath}'");
        }

        AudioClip audioClip = audioClipCache[audioPath];
        
        // 创建临时游戏对象播放音效
        GameObject audioObj = new GameObject($"TempAudio_{audioPath}");
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.time = startTime;
        
        audioSource.Play();
        
        // 播放完成后销毁临时对象
        Object.Destroy(audioObj, audioClip.length - startTime + 0.1f);
    }

    /// <summary>
    /// 播放音效（协程版本，可以等待播放完成）
    /// </summary>
    public static IEnumerator PlayAudioCoroutine(string audioPath, float volume = 1.0f, float startTime = 0f, bool waitForComplete = true)
    {
        // 如果缓存中没有，则加载并缓存
        if (!audioClipCache.ContainsKey(audioPath))
        {
            AudioClip clip = Resources.Load<AudioClip>(audioPath);
            if (clip == null)
            {
                Debug.LogWarning($"SFX: Failed to load audio from '{audioPath}'");
                yield break;
            }
            audioClipCache[audioPath] = clip;
            // Debug.Log($"SFX: Loaded and cached audio '{audioPath}'");
        }

        AudioClip audioClip = audioClipCache[audioPath];
        
        // 创建临时游戏对象播放音效
        GameObject audioObj = new GameObject($"TempAudio_{audioPath}");
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.time = startTime;
        
        audioSource.Play();
        
        // 如果需要等待播放完成
        if (waitForComplete)
        {
            yield return new WaitForSeconds(audioClip.length - startTime);
        }
        
        // 销毁临时对象
        Object.Destroy(audioObj);
    }

    /// <summary>
    /// 预加载音效到缓存
    /// </summary>
    public static void PreloadAudio(string audioPath)
    {
        if (!audioClipCache.ContainsKey(audioPath))
        {
            AudioClip clip = Resources.Load<AudioClip>(audioPath);
            if (clip != null)
            {
                audioClipCache[audioPath] = clip;
                Debug.Log($"SFX: Preloaded audio '{audioPath}'");
            }
            else
            {
                Debug.LogWarning($"SFX: Failed to preload audio from '{audioPath}'");
            }
        }
    }

    /// <summary>
    /// 批量预加载音效
    /// </summary>
    public static void PreloadAudios(string[] audioPaths)
    {
        foreach (string path in audioPaths)
        {
            PreloadAudio(path);
        }
    }

    /// <summary>
    /// 清除所有音效缓存
    /// </summary>
    public static void ClearCache()
    {
        audioClipCache.Clear();
        Debug.Log("SFX: Cleared all audio cache");
    }

    /// <summary>
    /// 清除指定音效缓存
    /// </summary>
    public static void ClearCache(string audioPath)
    {
        if (audioClipCache.ContainsKey(audioPath))
        {
            audioClipCache.Remove(audioPath);
            Debug.Log($"SFX: Cleared audio cache for '{audioPath}'");
        }
    }

    /// <summary>
    /// 获取当前缓存的音效数量
    /// </summary>
    public static int GetCacheCount()
    {
        return audioClipCache.Count;
    }
}
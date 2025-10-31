// Scripts/Singleton/CameraShake.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// 屏幕振动管理器（全局单例）- 震动Normal对象版本（使用旋转）
/// </summary>
public class CameraShake : MonoSingleton<CameraShake>
{
    public GameObject normalObject;
    public Transform normalTransform;
    public Quaternion originalRotation;

    public void Start()
    {
        // 查找名为 Normal 的对象
        normalObject = GameObject.Find("Normal");
        if (normalObject != null)
        {
            normalTransform = normalObject.transform;
            originalRotation = normalTransform.localRotation;
            // Debug.Log($"CameraShake: 找到 Normal 对象, 当前rotation: {originalRotation.eulerAngles}");
        }
        else
        {
            Debug.LogError("CameraShake: 场景中找不到名为 Normal 的对象");
        }
    }

    /// <summary>
    /// 轻微屏幕抖动（受击时）
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    /// <param name="magnitude">震动幅度（旋转角度）</param>
    public IEnumerator ShakeLight(float duration = 0.15f, float magnitude = 0.5f)
    {
        return Shake(duration, magnitude);
    }

    /// <summary>
    /// 中等屏幕抖动
    /// </summary>
    public IEnumerator ShakeMedium(float duration = 0.2f, float magnitude = 1.0f)
    {
        return Shake(duration, magnitude);
    }

    /// <summary>
    /// 强力屏幕抖动
    /// </summary>
    public IEnumerator ShakeStrong(float duration = 0.3f, float magnitude = 1.5f)
    {
        return Shake(duration, magnitude);
    }

    /// <summary>
    /// 通用屏幕抖动（使用旋转）
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    /// <param name="magnitude">振动幅度（旋转角度）</param>
    private IEnumerator Shake(float duration, float magnitude)
    {
        if (normalTransform == null)
        {
            Debug.LogWarning("CameraShake.Shake: normalTransform is null");
            yield break;
        }

        // 记录原始旋转
        originalRotation = normalTransform.localRotation;
        
        // Debug.Log($"CameraShake.Shake: Starting shake with duration={duration}, magnitude={magnitude}, originalRotation={originalRotation.eulerAngles}");
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 随机旋转角度
            float randomZ = Random.Range(-1f, 1f) * magnitude;
            
            // 应用旋转（只绕Z轴旋转，适合2D屏幕震动）
            Quaternion shakeRotation = Quaternion.Euler(0f, 0f, randomZ);
            normalTransform.localRotation = originalRotation * shakeRotation;
            
            // Debug.Log($"CameraShake.Shake: Frame shake rotation={randomZ}, newRotation={normalTransform.localRotation.eulerAngles}");

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复Normal原始旋转
        normalTransform.localRotation = originalRotation;
        
        // Debug.Log($"CameraShake.Shake: Shake completed, restored to {originalRotation.eulerAngles}");
    }
}
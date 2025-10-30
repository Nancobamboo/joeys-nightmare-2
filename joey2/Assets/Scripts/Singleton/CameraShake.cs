// Scripts/Singleton/CameraShake.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// 屏幕振动管理器（全局单例）
/// </summary>
public class CameraShake : MonoSingleton<CameraShake>
{
    private Camera shakeCamera;
    private Vector3 originalPosition;

    private void Start()
    {
        // 优先查找MainCamera，如果没有则查找任意激活的相机
        shakeCamera = Camera.main;
        if (shakeCamera == null)
        {
            // 如果没有MainCamera标签，则使用场景中第一个找到的相机
            shakeCamera = FindObjectOfType<Camera>();
            if (shakeCamera == null)
            {
                Debug.LogError("CameraShake: 场景中找不到任何相机");
            }
            else
            {
                Debug.LogWarning($"CameraShake: 场景中找不到 Main Camera，使用 {shakeCamera.name} 代替");
            }
        }
    }

    /// <summary>
    /// 轻微屏幕抖动（受击时）
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    /// <param name="magnitude">震动幅度</param>
    public IEnumerator ShakeLight(float duration = 0.15f, float magnitude = 0.1f)
    {
        return Shake(duration, magnitude);
    }

    /// <summary>
    /// 中等屏幕抖动
    /// </summary>
    public IEnumerator ShakeMedium(float duration = 0.2f, float magnitude = 0.2f)
    {
        return Shake(duration, magnitude);
    }

    /// <summary>
    /// 强力屏幕抖动
    /// </summary>
    public IEnumerator ShakeStrong(float duration = 0.3f, float magnitude = 0.35f)
    {
        return Shake(duration, magnitude);
    }

    /// <summary>
    /// 通用屏幕抖动
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    /// <param name="magnitude">振动幅度</param>
    private IEnumerator Shake(float duration, float magnitude)
    {
        if (shakeCamera == null)
        {
            Debug.LogWarning("CameraShake.Shake: shakeCamera is null");
            yield break;
        }

        originalPosition = shakeCamera.transform.localPosition;
        Debug.Log($"CameraShake.Shake: Starting shake with duration={duration}, magnitude={magnitude}");
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float randomX = Random.Range(-1f, 1f) * magnitude;
            float randomY = Random.Range(-1f, 1f) * magnitude;

            shakeCamera.transform.localPosition = originalPosition + new Vector3(randomX, randomY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复摄像机原始位置
        shakeCamera.transform.localPosition = originalPosition;
        Debug.Log("CameraShake.Shake: Shake completed");
    }
}
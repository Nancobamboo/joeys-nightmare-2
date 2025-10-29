// Scripts/Singleton/CameraShake.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// 屏幕振动管理器（全局单例）
/// </summary>
public class CameraShake : MonoSingleton<CameraShake>
{
    private Camera mainCamera;
    private Vector3 originalPosition;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CameraShake: 场景中找不到 Main Camera");
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
        if (mainCamera == null)
            yield break;

        originalPosition = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float randomX = Random.Range(-1f, 1f) * magnitude;
            float randomY = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = originalPosition + new Vector3(randomX, randomY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复摄像机原始位置
        mainCamera.transform.localPosition = originalPosition;
    }
}
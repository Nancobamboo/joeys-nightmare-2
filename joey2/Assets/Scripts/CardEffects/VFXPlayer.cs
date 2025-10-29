// Scripts/CardEffects/VFXPlayer.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VFX 播放器 - 管理攻击和受击特效的并行播放和时序
/// </summary>
public class VFXPlayer : MonoSingleton<VFXPlayer>
{
    /// <summary>
    /// 播放一个 VFX 序列
    /// 攻击特效并行播放 → 等待最长的攻击特效结束 → 播放受击特效
    /// </summary>
    public  IEnumerator PlayVFXSequence(
        VFXSequence sequence,
        GameObject attacker,
        GameObject target = null)
    {
        if (sequence == null) yield break;

        // 1. 并行播放所有攻击特效
        float maxAttackDuration = 0f;
        var attackCoroutines = new List<IEnumerator>();
        
        foreach (var vfx in sequence.attackVFXs)
        {
            float totalTime = vfx.delay + vfx.duration;
            maxAttackDuration = Mathf.Max(maxAttackDuration, totalTime);
            attackCoroutines.Add(PlaySingleVFX(vfx, attacker));
        }

        // 并行启动所有攻击特效
        foreach (var co in attackCoroutines)
        {
            StartCoroutine(co);
        }

        // 等待最长的攻击特效完成
        yield return new WaitForSeconds(maxAttackDuration);

        // 2. 播放所有受击特效
        foreach (var vfx in sequence.hitVFXs)
        {
            StartCoroutine(PlaySingleVFX(vfx, target));
        }

        // 等待最长的受击特效完成
        float maxHitDuration = 0f;
        foreach (var vfx in sequence.hitVFXs)
        {
            float totalTime = vfx.delay + vfx.duration;
            maxHitDuration = Mathf.Max(maxHitDuration, totalTime);
        }

        yield return new WaitForSeconds(maxHitDuration);
    }

    /// <summary>
    /// 播放单个 VFX
    /// </summary>
    private static IEnumerator PlaySingleVFX(VFXConfig config, GameObject target)
    {
        if (config.duration <= 0) yield break;

        // 延迟
        if (config.delay > 0)
            yield return new WaitForSeconds(config.delay);

        Vector3 playPosition = Vector3.zero;
        if (target != null)
        {
            playPosition = target.transform.position + config.positionOffset;
        }

        switch (config.type)
        {
            case VFXType.ParticleSystem:
                yield return PlayParticleSystem(config, playPosition);
                break;

            case VFXType.Animation:
                yield return PlayAnimationClip(config, target);
                break;

            case VFXType.SpriteAnimation:
                yield return PlaySpriteAnimation(config, target, playPosition);
                break;

            case VFXType.AudioClip:
                yield return PlayAudioClip(config);
                break;

            case VFXType.Shake:
                yield return PlayShake(config);
                break;
        }
    }

    /// <summary>
    /// 播放粒子系统
    /// </summary>
    private static IEnumerator PlayParticleSystem(VFXConfig config, Vector3 position)
    {
        if (config.particleSystemPrefab == null) yield break;

        var ps = Instantiate(config.particleSystemPrefab, position, Quaternion.identity);
        yield return new WaitForSeconds(config.duration);
        Destroy(ps.gameObject);
    }

    /// <summary>
    /// 播放 Animator 动画
    /// </summary>
    private static IEnumerator PlayAnimationClip(VFXConfig config, GameObject target)
    {
        if (target == null || config.animationClip == null) yield break;

        var animator = target.GetComponent<Animator>();
        if (animator == null) yield break;

        animator.SetTrigger(config.id);
        yield return new WaitForSeconds(config.duration);
    }

    /// <summary>
    /// 播放逐帧 Sprite 动画
    /// </summary>
    private static IEnumerator PlaySpriteAnimation(VFXConfig config, GameObject target, Vector3 position)
    {
        // 这里需要你提供 Sprite 序列资源
        // 示例实现
        yield return new WaitForSeconds(config.duration);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    private static IEnumerator PlayAudioClip(VFXConfig config)
    {
        if (config.audioClip == null) yield break;

        // 创建临时 AudioSource 或使用全局音效管理器
        var audioSource = new GameObject("AudioTemp").AddComponent<AudioSource>();
        audioSource.PlayOneShot(config.audioClip);
        
        yield return new WaitForSeconds(config.audioClip.length);
        Destroy(audioSource.gameObject);
    }

    /// <summary>
    /// 屏幕震动
    /// </summary>
    private static IEnumerator PlayShake(VFXConfig config)
    {
        // 实现屏幕震动逻辑
        yield return new WaitForSeconds(config.duration);
    }
}
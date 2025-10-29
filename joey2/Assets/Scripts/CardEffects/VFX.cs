// Scripts/CardEffects/VFX.cs
using System.Collections;
using UnityEngine;

public class VFX : MonoSingleton<VFX>
{
    /// <summary>
    /// 播放打击特效（新系统）
    /// </summary>
    public  IEnumerator PlayHitWithSequence(
        GameObject attacker,
        GameObject target,
        VFXSequence sequence)
    {
        if (sequence != null)
        {
            yield return VFXPlayer.Instance.PlayVFXSequence(sequence, attacker, target);
        }
        else
        {
            // 后备方案：简单的击中特效
            yield return PlayHit(attacker, target);
        }
    }

    /// <summary>
    /// 原始的简单击中特效（保持向后兼容）
    /// </summary>
    public static IEnumerator PlayHit(GameObject src, GameObject dst)
    {
        // TODO: 播放击中特效、抖动、音效等
        yield return null;
    }

    public static IEnumerator PlayLifeSteal(GameObject src)
    {
        // TODO: 播放吸血特效
        yield return null;
    }
}
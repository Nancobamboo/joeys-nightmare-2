// Scripts/CardEffects/VFX.cs
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
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
    public static IEnumerator PlayHit(GameObject cardGO ,GameObject targetCardGO=null,int damage=0,bool monsterAttack=false,Dictionary<string,object> extra=null)
    {
        if (cardGO == null) yield break;
        yield return PlayAnimator(cardGO, "UI_Carditem_gongji");
        yield return new WaitForSeconds(0.4f);
        GameEvents.RaiseAttackPre(cardGO,targetCardGO,damage,monsterAttack,extra);
        yield return new WaitForSeconds(0.4f);
        GameEvents.RaiseAttackPreFinish(cardGO);
    }


    public static IEnumerator PlayAnimator(GameObject cardGO,string animationName)
    {
        // TODO: 播放击中特效、抖动、音效等
        Animator animator = cardGO.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"PlayAnimator: No AnimatorController assigned to {cardGO.name}");
            }
            else
            {
                animator.Play(animationName);
                // Debug.Log("PlayMonsterHit: Playing attack animation");
                yield return null;
            }
        }
    }

    public static IEnumerator PlayAnimatorReverse(GameObject cardGO, string animationName)
    {
        Animator animator = cardGO.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"PlayAnimatorReverse: No AnimatorController assigned to {cardGO.name}");
            }
            else
            {
                Debug.Log("PlayAnimatorReverse: Playing animation in reverse");
                // 设置倒放速度
                animator.speed = -1f;
                // 从动画的结尾开始播放（normalizedTime = 1 表示 100%）
                animator.Play(animationName, 0, 1f);
                
                // Debug.Log($"PlayAnimatorReverse: Playing {animationName} in reverse");
                yield return null; // 等一帧让动画开始
                yield return new WaitForSeconds(0.7f);
                animator.speed = 1f;
                Debug.Log("PlayAnimatorReverse: Playing animation in reverse");
            }
        }
    }



    public static IEnumerator PlayMonsterHit(GameObject cardGO)
    {
        if (cardGO == null) yield break;
        yield return PlayAnimator(cardGO, "UI_Carditem_guaiwugongji");
        yield return new WaitForSeconds(0.4f);
        GameEvents.RaiseMonsterAttackPre(cardGO);
        yield return new WaitForSeconds(0.4f);
        GameEvents.RaiseMonsterAttackPreFinish(cardGO);

    }





    public static IEnumerator PlayLifeSteal(GameObject src)
    {
        // TODO: 播放吸血特效
        yield return null;
    }
}
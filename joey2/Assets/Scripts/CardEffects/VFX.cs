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
    public static IEnumerator PlayHit(GameObject cardGO ,GameObject targetCardGO=null,int damage=0,bool monsterAttack=false)
    {
        if (cardGO == null) yield break;
        yield return PlayAnimator(cardGO, "UI_Carditem_gongji");
        yield return new WaitForSeconds(0.4f);
        GameEvents.RaiseAttackPre(cardGO,targetCardGO,damage,monsterAttack);
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
                yield return null;
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
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
        // TODO: 播放击中特效、抖动、音效等
        Animator animator = cardGO.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"PlayDamageVFX: No AnimatorController assigned to {cardGO.name}");
            }
            else
            {
                animator.Play("UI_Carditem_gongji");
                // Debug.Log("PlayHit: Playing attack animation");
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);
            GameEvents.RaiseAttackPre(cardGO,targetCardGO,damage,monsterAttack);
            yield return new WaitForSeconds(0.4f);
            GameEvents.RaiseAttackPreFinish(cardGO);
        }

    }

    public static IEnumerator PlayMonsterHit(GameObject cardGO ,GameObject targetCardGO=null,int damage=0,bool monsterAttack=false)
    {
        if (cardGO == null) yield break;
        // TODO: 播放击中特效、抖动、音效等
        Animator animator = cardGO.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"PlayDamageVFX: No AnimatorController assigned to {cardGO.name}");
            }
            else
            {
                animator.Play("UI_Carditem_guaiwugongji");
                // Debug.Log("PlayHit: Playing attack animation");
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);
            GameEvents.RaiseMonsterAttackPre(cardGO,targetCardGO,damage,monsterAttack);
            yield return new WaitForSeconds(0.4f);
            GameEvents.RaiseMonsterAttackPreFinish(cardGO);
        }

    }





    public static IEnumerator PlayLifeSteal(GameObject src)
    {
        // TODO: 播放吸血特效
        yield return null;
    }
}
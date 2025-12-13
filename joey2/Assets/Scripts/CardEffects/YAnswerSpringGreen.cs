using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YAnswerSpringGreen : YDefaultEffect
{
    private bool m_IsThrowingWeapon = false;
    
    public YAnswerSpringGreen()
    {
        Id = ECardEffectId.AnswerSpringGreen;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        // 防止短时间内多次触发，避免与SingleDelayAction的连锁反应
        if (!m_IsThrowingWeapon)
        {
            ThrowWeaponDelayedAsync().Forget();
        }
        return base.OnTakeDamage(effectType, damage);
    }
    
    private async UniTaskVoid ThrowWeaponDelayedAsync()
    {
        m_IsThrowingWeapon = true;
        await UniTask.WaitForSeconds(0.3f);
        YActionSystem.Instance.DispatchAction(EActionId.ThrowWeaponToEnv);
        m_IsThrowingWeapon = false;
    }
}


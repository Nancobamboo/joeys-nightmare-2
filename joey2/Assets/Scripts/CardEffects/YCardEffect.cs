// Scripts/CardEffects/YCardEffect.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public enum ECardEffectId
{
    BounceToRandomEnemy_OnDealDamage,
    ExtraDamage_OnDealDamage,
    Boom_OnDead,
    DealRandomEnemyEqualToAttack_OnTop,
    DealDamage_UseDefence,
    Boom_OnPlay,
    DoubleAttack_OnDealDamage,
    ExtraDamage_NoDefence,
    Electric,
    DoubleAttack_OnPlay,
    HookEquipWeaponFromDiscard_OnDefence,
    HookEquipWeaponFromDiscard_OnPlay,
    LifeSteal_OnDealDamage,
    HealPlayer_OnPlay
}

public enum EEffectType
{
    Damage,
    Heal,
    Buff,
    Debuff,
    ReflectDamage,
    LifeSteal,
    Boom,
    ExtraTime,
    Electric,
    Other
}

public enum EVFXName
{
    VFX_appear,
    VFX_boom,
    VFX_disappear,
    VFX_Dun,
    VFX_Dunsui,
    VFX_glow,
    VFX_LeiDan,
    VFX_Shouji
}

public enum ECardAnimName
{
    None,
    UI_Carditem_shouji,
    UI_Carditem_xiaoshi,
    UI_Carditem_dunpai,
    UI_Carditem_gongji,
    UI_Carditem_guaiwugongji,
    UI_Carditem_diaoluo_anim,
    UI_Carditem_feitian,
    UI_Carditem_pailai
}

public enum EVFXLife
{
    CardLife,
    SelfLife
}



public class YCardEffect
{
    public ECardEffectId Id;
    public UICardSimpleControl CardControl;
    public bool IsEffecting;

    public void SetData(UICardSimpleControl cardControl)
    {
        CardControl = cardControl;
    }

    public virtual float OnBecomeTopOfPile()
    {
        return 0.5f;
    }

    public virtual float OnEnterBag()
    {
        return 0f;
    }

    public virtual float OnDealDamage()
    {
        return 0.4f;
    }

    public virtual float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        return 0.45f;
    }

    public virtual float OnKill()
    {
        return 0.5f;
    }

    public virtual float OnBeDying()
    {
        return 0.5f;
    }

    public virtual float OnDead()
    {
        return 0.5f;
    }

    public virtual float UseDefence()
    {
        return 0.5f;
    }

    public virtual float UseSkill()
    {
        return 0.5f;
    }

    public virtual float UseItem()
    {
        return 0.5f;
    }

    public virtual float UseAttack()
    {
        return 0.5f;
    }

    public virtual void OnUseFinished()
    {
    }

    public virtual int GetEffectValue(EEffectType effectType)
    {
        switch (effectType)
        {
            case EEffectType.Damage:
                return 0;
            case EEffectType.Heal:
                return 0;
            case EEffectType.Buff:
                return 0;
            case EEffectType.Debuff:
                return 0;
            case EEffectType.ReflectDamage:
                return 0;
            case EEffectType.LifeSteal:
                return 0;
            case EEffectType.ExtraTime:
                return 0;
            case EEffectType.Other:
                return 0;
            default:
                return 0;
        }
    }

}


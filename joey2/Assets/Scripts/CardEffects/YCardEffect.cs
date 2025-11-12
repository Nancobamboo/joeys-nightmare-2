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

    public virtual void OnBecomeTopOfPile()
    {
    }

    public virtual void OnEnterBag()
    {

    }


    public virtual void OnDealDamage()
    {
    }

    public virtual void OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
    }


    public virtual void OnKill()
    {
    }

    public virtual void OnDead()
    {
    }

    public virtual void UseDefence()
    {
    }

    public virtual void UseSkill()
    {
    }
    public virtual void UseItem()
    {
    }
    public virtual void UseAttack()
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
            case EEffectType.Other:
                return 0;
            default:
                return 0;
        }
    }

}


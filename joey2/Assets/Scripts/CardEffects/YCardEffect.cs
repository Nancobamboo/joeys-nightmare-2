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
    HealPlayer_OnPlay,
    AutoBoomMoney,
    StealMoney,
    GravityShoes,
    RomeoMonkey,
    JulietMonkey,
    BadMonkey,
    PermanentAttackBoost,
    PermanentDefenceBoost,
    MoveWeaponToEnv,
    Ghost,
    GrimReaper,
    GrimReaperClone,
    ExtraDamage_HalfHealth,
    ThrowWeaponToStack_OnDefence,
    HealPlayer_OnDefense,
    AddKnifeToEnv_OnDefense,
    ApeWine,
    GiftBox,
    DartScroll,
    HookDefenceFromDiscard_OnRemoveCard,
    AddKnifeToEnv_UseSkill,
    ThrowWeaponDefenceToStack_UseSkill
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
    ExtraAttackCnt,
    Electric,
    Other,
    Defence,
    Upper
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
    VFX_Shouji,
    VFX_Fanjia, //仞甲
}

public enum ECardAnimName
{
    Idle,
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
    private int[] m_EffectValues = new int[(int)EEffectType.Upper];

    public virtual void SetData(UICardSimpleControl cardControl)
    {
        CardControl = cardControl;
    }

    public virtual float OnBecomeTopOfPile()
    {
        return 0f;
    }

    public virtual float OnEnterBag()
    {
        return 0f;
    }

    public virtual float OnDealDamage()
    {
        return 0f;
    }

    public virtual float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        return 0f;
    }

    public virtual float OnKill()
    {
        return 0f;
    }

    public virtual float OnBeDying()
    {
        return 0f;
    }

    public virtual float OnDead()
    {
        return 0f;
    }

    public virtual float UseDefence(bool isOverflow = false)
    {
        return 0f;
    }

    public virtual float UseSkill()
    {
        return 0f;
    }

    public virtual float UseItem()
    {
        return 0f;
    }

    public virtual float UseAttack()
    {
        return 0f;
    }

    public virtual float OnUseFinished()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_diaoluo_anim, EVFXLife.SelfLife);
            return maxDelayTime;
        }
        return 0f;
    }

    public virtual float OnRemoveCard()
    {
        return 0f;
    }

    public virtual int GetEffectValue(EEffectType effectType)
    {
        int index = (int)effectType;
        if (index >= 0 && index < m_EffectValues.Length)
        {
            return m_EffectValues[index];
        }
        return 0;
    }

    public virtual void AddEffectValue(EEffectType effectType, int value)
    {
        int index = (int)effectType;
        if (index >= 0 && index < m_EffectValues.Length)
        {
            m_EffectValues[index] += value;
        }
    }

    public virtual int OnBuffValueChange(EBuffType buffType, int value)
    {
        return value;
    }
    

}


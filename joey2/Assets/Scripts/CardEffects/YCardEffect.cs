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
    Boom_OnKill,
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
    Other
}



public class YCardEffect
{
    public ECardEffectId Id;
    public UICardSimpleControl CardControl;
    public List<CancellationTokenSource> CancelTokenList = new List<CancellationTokenSource>();

    public void SetData(UICardSimpleControl cardControl)
    {
        CardControl = cardControl;
    }

    public virtual void OnBecomeTopOfPile()
    {
    }

    public virtual void OnPlay()
    {
    }

    public virtual void OnDealDamage()
    {
    }

    public virtual void OnTakeDamage()
    {
    }

    public async UniTaskVoid DelayStopVFX(float delayTime)
    {
        var cts = new CancellationTokenSource();
        CancelTokenList.Add(cts);
        await UniTask.WaitForSeconds(delayTime, cancellationToken: cts.Token);
        if (CardControl != null && CardControl.gameObject != null)
        {
            CardControl.StopVFX();
        }
    }

    public virtual void OnKill()
    {
    }

    public virtual void UseDefence()
    {
    }

    public virtual void OnTurnStart_Player()
    {
    }

    public virtual void OnTurnEnd_Player()
    {
    }

    public virtual void OnTurnStart_Enemy()
    {
    }

    public virtual void OnTurnEnd_Enemy()
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
            case EEffectType.Other:
                return 0;
            default:
                return 0;
        }
    }

    public void StopAllEffects()
    {
        for (int i = 0; i < CancelTokenList.Count; i++)
        {
            var cts = CancelTokenList[i];
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
        CancelTokenList.Clear();
    }
}


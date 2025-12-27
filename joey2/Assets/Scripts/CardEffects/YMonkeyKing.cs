using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YMonkeyKing : YDefaultEffect
{
    public YMonkeyKing()
    {
        Id = ECardEffectId.MonkeyKing;
    }
}

public partial class UIGamePhaseControl
{
    public bool IsMonkeyKingAttack(UICardSimpleControl monsterCard)
    {
        if (monsterCard?.CardEffect == null)
        {
            return false;
        }
        return monsterCard.CardEffect.Id == ECardEffectId.MonkeyKing;
    }

    void MonkeyKingRemoveDefence(object[] paraArray)
    {
        MonkeyKingRemoveDefenceAsync().Forget();
    }

    public async UniTask MonkeyKingRemoveDefenceAsync()
    {
        UICardSimpleControl kingCard = FindEnvCardByEffectId(ECardEffectId.MonkeyKing);
        if (kingCard == null || !kingCard.gameObject.activeSelf || kingCard.CardData.currentHealth <= 0)
        {
            return;
        }

        UICardSimpleControl defenceCard = GetLastBagCard(ECardType.defence);
        if (defenceCard == null)
        {
            return;
        }

        // 效果触发前的延迟，让玩家注意到
        await UniTask.WaitForSeconds(0.5f);

        var vfxNames = new List<EVFXName> { EVFXName.VFX_disappear };
        float vfxDelay = defenceCard.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_xiaoshi, EVFXLife.SelfLife);
        await UniTask.WaitForSeconds(vfxDelay > 0f ? vfxDelay : 0.8f);

        await RemoveBagCard(ECardType.defence, defenceCard);
        RemoveCardCts(defenceCard);
        
        await UniTask.WaitForSeconds(0.5f);
    }

    public async UniTask TriggerQueenAttackedEffects()
    {
        UICardSimpleControl kingCard = FindEnvCardByEffectId(ECardEffectId.MonkeyKing);
        if (kingCard != null && kingCard.gameObject.activeSelf && kingCard.CardData.currentHealth > 0)
        {
            await MonkeyKingRemoveDefenceAsync();
        }

        UICardSimpleControl turkeyCard = FindEnvCardByEffectId(ECardEffectId.TurkeyJack);
        if (turkeyCard != null && turkeyCard.gameObject.activeSelf && turkeyCard.CardData.currentHealth > 0)
        {
            await TurkeyJackExtraCounterAsync();
        }
    }

    public async UniTask TriggerKingAttackedEffects(UICardSimpleControl attackedCard)
    {
        UICardSimpleControl queenCard = FindEnvCardByEffectId(ECardEffectId.DonkeyQueen);
        if (queenCard != null && queenCard.gameObject.activeSelf && queenCard.CardData.currentHealth > 0)
        {
            await UniTask.WaitForSeconds(0.5f);
            await DonkeyQueenHealTargetAsync(attackedCard);
        }

        UICardSimpleControl turkeyCard = FindEnvCardByEffectId(ECardEffectId.TurkeyJack);
        if (turkeyCard != null && turkeyCard.gameObject.activeSelf && turkeyCard.CardData.currentHealth > 0)
        {
            await TurkeyJackExtraCounterAsync();
        }
    }
}


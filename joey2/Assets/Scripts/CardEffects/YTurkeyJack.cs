using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YTurkeyJack : YDefaultEffect
{
    private int m_CounterAttackCount;

    public YTurkeyJack(int counterAttackCount = 2)
    {
        m_CounterAttackCount = Mathf.Max(1, counterAttackCount);
        Id = ECardEffectId.TurkeyJack;
    }

    public int GetCounterAttackCount()
    {
        return m_CounterAttackCount;
    }
}

public partial class UIGamePhaseControl
{
    public int GetMonsterCounterAttackCount(UICardSimpleControl monsterCard)
    {
        if (monsterCard?.CardEffect is YTurkeyJack turkeyEffect)
        {
            return turkeyEffect.GetCounterAttackCount();
        }
        return 1;
    }

    public UICardSimpleControl FindEnvCardByEffectId(ECardEffectId effectId)
    {
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf &&
                lastCard.CardEffect != null && lastCard.CardEffect.Id == effectId &&
                lastCard.CardData.currentHealth > 0)
            {
                return lastCard;
            }
        }
        return null;
    }

    public int FindEnvIndexByCard(UICardSimpleControl card)
    {
        if (card == null) return -1;
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard == card)
            {
                return i;
            }
        }
        return -1;
    }

    async void TurkeyJackExtraCounter(object[] paraArray)
    {
        await TurkeyJackExtraCounterAsync();
    }

    public async UniTask TurkeyJackExtraCounterAsync()
    {
        UICardSimpleControl turkeyCard = FindEnvCardByEffectId(ECardEffectId.TurkeyJack);
        if (turkeyCard == null || !turkeyCard.gameObject.activeSelf || turkeyCard.CardData.currentHealth <= 0)
        {
            return;
        }

        int attack = turkeyCard.CardData.currentAttack;
        if (attack <= 0)
        {
            return;
        }

        int envIndex = FindEnvIndexByCard(turkeyCard);
        if (envIndex < 0)
        {
            return;
        }

        CancellationToken token = GetOrCreateCardToken(turkeyCard);
        await TakePlayerDamageAsync(attack, turkeyCard, envIndex, token, null);
        RemoveCardCts(turkeyCard);
    }
}


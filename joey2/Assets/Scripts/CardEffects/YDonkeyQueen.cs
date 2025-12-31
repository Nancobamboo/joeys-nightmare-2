using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YDonkeyQueen : YDefaultEffect
{
    private int m_HealAmount;

    public YDonkeyQueen(int healAmount = 6)
    {
        m_HealAmount = Mathf.Max(0, healAmount);
        Id = ECardEffectId.DonkeyQueen;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        YActionSystem.Instance.DispatchAction(EActionId.DonkeyQueenRefreshPlayerCards);
    }

    public override float OnDead()
    {
        YActionSystem.Instance.DispatchAction(EActionId.DonkeyQueenRefreshPlayerCards);
        return base.OnDead();
    }

    public int GetHealAmount()
    {
        return m_HealAmount;
    }
}

public partial class UIGamePhaseControl
{
    public bool IsDonkeyQueenAlive()
    {
        UICardSimpleControl queenCard = FindEnvCardByEffectId(ECardEffectId.DonkeyQueen);
        return queenCard != null && queenCard.gameObject.activeSelf && queenCard.CardData.currentHealth > 0;
    }

    public int ApplyDonkeyQueenDebuff(int value)
    {
        if (!IsDonkeyQueenAlive())
        {
            return value;
        }
        int reduction = value / 3;
        return Mathf.Max(0, value - reduction);
    }

    void DonkeyQueenRefreshPlayerCards(object[] paraArray)
    {
        RefreshAllBagCards();
    }

    private void RefreshAllBagCards()
    {
        List<UICardSimpleControl> attackCardList = GetBagCardList(ECardType.attack);
        if (attackCardList != null)
        {
            for (int i = 0; i < attackCardList.Count; i++)
            {
                attackCardList[i].RefreshCard();
            }
        }

        List<UICardSimpleControl> defenceCardList = GetBagCardList(ECardType.defence);
        if (defenceCardList != null)
        {
            for (int i = 0; i < defenceCardList.Count; i++)
            {
                defenceCardList[i].RefreshCard();
            }
        }

        if (m_FistCardCache != null)
        {
            m_FistCardCache.RefreshCard();
        }
    }

    void DonkeyQueenHealKing(object[] paraArray)
    {
        UICardSimpleControl targetCard = (UICardSimpleControl)paraArray[0];
        DonkeyQueenHealTargetAsync(targetCard).Forget();
    }

    public async UniTask DonkeyQueenHealTargetAsync(UICardSimpleControl targetCard)
    {
        UICardSimpleControl queenCard = FindEnvCardByEffectId(ECardEffectId.DonkeyQueen);

        if (queenCard == null || targetCard == null)
        {
            return;
        }

        if (!queenCard.gameObject.activeSelf || queenCard.CardData.currentHealth <= 0)
        {
            return;
        }

        if (!targetCard.gameObject.activeSelf || targetCard.CardData.currentHealth <= 0)
        {
            return;
        }

        int healAmount = 0;
        if (queenCard.CardEffect is YDonkeyQueen queenEffect)
        {
            healAmount = queenEffect.GetHealAmount();
        }

        if (healAmount <= 0)
        {
            return;
        }

        if (targetCard.CacheTrans != null)
        {
            JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, targetCard.CacheTrans, 1f);
        }
        await UniTask.WaitForSeconds(0.5f);

        targetCard.CardData.currentHealth += healAmount;
        if (targetCard.CardData.currentHealth > targetCard.CardData.health)
        {
            targetCard.CardData.currentHealth = targetCard.CardData.health;
        }
        targetCard.RefreshCard();

        await UniTask.WaitForSeconds(0.3f);
    }
}


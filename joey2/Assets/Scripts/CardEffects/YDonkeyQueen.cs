using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YDonkeyQueen : YDefaultEffect
{
    private int m_HealAmount;

    public YDonkeyQueen(int healAmount = 5)
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
        DonkeyQueenHealKingAsync().Forget();
    }

    public async UniTask DonkeyQueenHealKingAsync()
    {
        UICardSimpleControl queenCard = FindEnvCardByEffectId(ECardEffectId.DonkeyQueen);
        UICardSimpleControl kingCard = FindEnvCardByEffectId(ECardEffectId.MonkeyKing);

        if (queenCard == null || kingCard == null)
        {
            return;
        }

        if (!queenCard.gameObject.activeSelf || queenCard.CardData.currentHealth <= 0)
        {
            return;
        }

        if (!kingCard.gameObject.activeSelf || kingCard.CardData.currentHealth <= 0)
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

        if (kingCard.CacheTrans != null)
        {
            JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, kingCard.CacheTrans, 1f);
        }
        await UniTask.WaitForSeconds(0.5f);

        kingCard.CardData.currentHealth += healAmount;
        if (kingCard.CardData.currentHealth > kingCard.CardData.health)
        {
            kingCard.CardData.currentHealth = kingCard.CardData.health;
        }
        kingCard.RefreshCard();

        await UniTask.WaitForSeconds(0.3f);
    }
}


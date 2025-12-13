using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YTimidTurkey : YDefaultEffect
{
    private bool _swapScheduled = false; // 标志位，用于防止重复触发交换

    public YTimidTurkey()
    {
        Id = ECardEffectId.TimidTurkey;
    }

    public override float OnDealDamage()
    {
        return base.OnDealDamage();
    }

    private async UniTaskVoid SwapTopTwoEnvCardsDelayed()
    {
        if (CardControl == null || CardControl.CardData == null)
        {
            _swapScheduled = false; // 重置标志
            return;
        }

        // 等待攻击流程完成
        // 通过检查 IsEffecting 状态来判断攻击流程是否完成
        float waitTime = 0f;
        float maxWaitTime = 10f;
        float checkInterval = 0.1f;

        while (waitTime < maxWaitTime)
        {
            await UniTask.WaitForSeconds(checkInterval);
            waitTime += checkInterval;

            // 检查攻击流程是否完成
            if (CardControl != null && !CardControl.IsEffecting)
            {
                // 再等待一小段时间确保所有清理和动画完成
                await UniTask.WaitForSeconds(0.3f);
                break;
            }

            // 如果卡片已经被销毁，退出
            if (CardControl == null || CardControl.CardData == null || !CardControl.gameObject.activeSelf)
            {
                _swapScheduled = false; // 重置标志
                return;
            }
        }

        // 再次检查卡片是否仍然有效
        if (CardControl == null || CardControl.CardData == null || !CardControl.gameObject.activeSelf)
        {
            _swapScheduled = false; // 重置标志
            return;
        }

        // 检查下方是否有牌，如果没有则不触发交换
        int envIndex = CardControl.EnvIndex;
        int envCardCount = JoeyGameControl.Instance.GetEnvCardCount(envIndex);
        if (envCardCount < 2)
        {
            _swapScheduled = false; // 重置标志
            return;
        }

        float maxDelayTime = CardControl.PlayVFX(new List<EVFXName> { EVFXName.VFX_disappear }, ECardAnimName.UI_Carditem_dunpai, EVFXLife.CardLife);
        await UniTask.WaitForSeconds(maxDelayTime);

        // 执行交换
        if (CardControl != null && CardControl.CardData != null && CardControl.gameObject.activeSelf)
        {
            YActionSystem.Instance.DispatchAction(EActionId.SwapTopTwoEnvCards, CardControl);
        }

        // 交换完成后重置标志，以便下次战斗可以再次触发
        _swapScheduled = false;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            // 只有在还没有安排交换时才启动新的交换任务
            // 这样可以确保即使有多次伤害结算，也只会执行一次交换
            if (!_swapScheduled)
            {
                _swapScheduled = true;
                SwapTopTwoEnvCardsDelayed().Forget();
            }
        }
        return base.OnTakeDamage(effectType);
    }

    public override float OnDead()
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            Debug.Log("TimidTurkey on remove card");
            DataSystem.Instance.AddCoin(20);
        }
        return base.OnDead();
    }

}

public partial class UIGamePhaseControl
{
    public void SwapTopTwoEnvCards(UICardSimpleControl cardControl)
    {
        if (cardControl == null || cardControl.CardData == null)
        {
            return;
        }
        int envIndex = cardControl.EnvIndex;
        List<UICardSimpleControl> cardList = m_EnvCardDict[envIndex];
        for (int i = 0; i < cardList.Count; i++)
        {
            Debug.Log("TimidTurkey swap before card index: " + i + " card: " + cardList[i].CardData.cardName);
        }
        if (cardList != null && cardList.Count >= 2)
        {
            UICardSimpleControl lastCard = cardList[cardList.Count - 1];
            UICardSimpleControl secondLastCard = cardList[cardList.Count - 2];
            cardList[cardList.Count - 1] = secondLastCard;
            cardList[cardList.Count - 2] = lastCard;
            m_EnvCardDict[envIndex] = cardList;
            // 更新Transform的sibling index以反映UI中的顺序
            Transform parent = lastCard.CacheTrans.parent;
            int lastSiblingIndex = lastCard.CacheTrans.GetSiblingIndex();
            int secondLastSiblingIndex = secondLastCard.CacheTrans.GetSiblingIndex();
            lastCard.CacheTrans.SetSiblingIndex(secondLastSiblingIndex);
            secondLastCard.CacheTrans.SetSiblingIndex(lastSiblingIndex);
            lastCard.RefreshCard();
            secondLastCard.RefreshCard();
        }
        for (int i = 0; i < cardList.Count; i++)
        {
            Debug.Log("TimidTurkey swap after card index: " + i + " card: " + cardList[i].CardData.cardName);
        }
        cardControl.RefreshCard();
    }
}
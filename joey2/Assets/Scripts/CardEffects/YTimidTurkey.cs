using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YTimidTurkey : YDefaultEffect
{
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
                return;
            }
        }
        
        // 执行交换
        if (CardControl != null && CardControl.CardData != null && CardControl.gameObject.activeSelf)
        {
            YActionSystem.Instance.DispatchAction(EActionId.SwapTopTwoEnvCards, CardControl);
        }
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            SwapTopTwoEnvCardsDelayed().Forget();
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
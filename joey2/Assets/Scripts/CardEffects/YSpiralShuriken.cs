// Scripts/CardEffects/Effects/YSpiralShuriken.cs
// 螺旋手里剑效果：继承手里剑的特效（装备时触发1次自动攻击）
// 当这张卡在手牌里的时候，其他攻击卡进入手牌（自身进入不触发），螺旋手里剑都会进入环境
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YSpiralShuriken : YDealRandomEnemyEqualToAttack_OnTop
{
	public YSpiralShuriken()
	{
		Id = ECardEffectId.SpiralShuriken;
	}

	public override float OnEnterBag()
	{
		// 继承手里剑的OnEnterBag行为
		return base.OnEnterBag();
	}
}

public partial class UIGamePhaseControl
{
	/// <summary>
	/// 检查手牌中是否有螺旋手里剑，如果有则将其移动到环境
	/// </summary>
	private void CheckAndMoveSpiralShurikenToEnv(UICardSimpleControl newlyAddedCard)
	{
		// 如果新进入的卡本身就是螺旋手里剑，则不触发任何效果
		// 这实现了“螺旋手里剑不能触发螺旋手里剑”的逻辑
		if (newlyAddedCard != null && newlyAddedCard.CardData != null && newlyAddedCard.CardData.id == "1024")
		{
			Debug.Log($"[SpiralShuriken] Newly added card is Spiral Shuriken, ignoring trigger.");
			return;
		}

		// 获取所有手牌中的攻击卡
		List<UICardSimpleControl> attackCards = GetBagCardList(ECardType.attack);
		if (attackCards == null || attackCards.Count == 0)
		{
			Debug.Log($"[SpiralShuriken] No attack cards in hand");
			return;
		}

		Debug.Log($"[SpiralShuriken] Checking {attackCards.Count} attack cards in hand. Newly added: {newlyAddedCard?.CardData?.cardName} (ID: {newlyAddedCard?.CardData?.id})");

		// 查找螺旋手里剑（排除当前正在添加的卡，避免自己触发自己）
		// 这实现了“自身无法触发自身”的逻辑：
		// 如果 newlyAddedCard 是螺旋手里剑，它会被跳过，不会被移动到环境
		UICardSimpleControl spiralShuriken = null;
		foreach (UICardSimpleControl card in attackCards)
		{
			if (card == null || card == newlyAddedCard)
			{
				continue;
			}

			if (card.CardData == null)
			{
				continue;
			}

			// 检查卡牌ID或特效ID
			bool isSpiralShuriken = false;
			if (card.CardData.id == "1024")
			{
				isSpiralShuriken = true;
				Debug.Log($"[SpiralShuriken] Found by card ID: {card.CardData.cardName}");
			}
			else if (card.CardEffect != null && card.CardEffect.Id == ECardEffectId.SpiralShuriken)
			{
				isSpiralShuriken = true;
				Debug.Log($"[SpiralShuriken] Found by effect ID: {card.CardData.cardName}");
			}

			if (isSpiralShuriken)
			{
				spiralShuriken = card;
				break;
			}
		}

		if (spiralShuriken == null)
		{
			Debug.Log($"[SpiralShuriken] Spiral shuriken not found in hand");
			return;
		}

		// 将螺旋手里剑移动到环境
		Debug.Log($"[SpiralShuriken] Moving {spiralShuriken.CardData.cardName} to environment");
		
		// 使用延迟调用，然后异步移动到环境
		JoeyGameControl.Instance.AddGlobalDelayCall(() =>
		{
			MoveSpiralShurikenToEnvAsync(spiralShuriken).Forget();
		}, 0.1f);
	}

	/// <summary>
	/// 将螺旋手里剑移动到环境（异步）
	/// </summary>
	private async UniTaskVoid MoveSpiralShurikenToEnvAsync(UICardSimpleControl spiralShuriken)
	{
		if (spiralShuriken == null || spiralShuriken.CardData == null)
		{
			return;
		}

		// 先从手牌中移除
		int cardTypeInt = (int)ECardType.attack;
		if (m_BagCardDict.TryGetValue(cardTypeInt, out List<UICardSimpleControl> cardList))
		{
			cardList.Remove(spiralShuriken);
		}

		// 调用AddEnvCardFromBagAsync移动到环境
		await AddEnvCardFromBagAsync(spiralShuriken);
		spiralShuriken.Return();
	}
}


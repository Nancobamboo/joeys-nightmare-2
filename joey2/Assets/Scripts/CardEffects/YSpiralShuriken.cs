using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YSpiralShuriken : YDealRandomEnemyEqualToAttack_OnTop
{
	/// <summary>
	/// 获得其他武器时：此牌进入环境，并有 50% 概率永久 +1 攻击。
	/// </summary>
	public const int TriggerPermanentAttackRatio = 50; // percent
	public const int TriggerPermanentAttackBonus = 1;

	public YSpiralShuriken()
	{
		Id = ECardEffectId.SpiralShuriken;
	}
}

public partial class UIGamePhaseControl
{
	private UICardSimpleControl FindEnvCardControlByUniqueId(int uniqueId)
	{
		if (uniqueId == 0 || m_EnvCardDict == null) return null;

		foreach (var kvp in m_EnvCardDict)
		{
			List<UICardSimpleControl> envCardList = kvp.Value;
			if (envCardList == null) continue;

			for (int i = 0; i < envCardList.Count; i++)
			{
				UICardSimpleControl cardControl = envCardList[i];
				if (cardControl != null && cardControl.CardData != null && cardControl.CardData.UniqueId == uniqueId)
				{
					return cardControl;
				}
			}
		}

		return null;
	}

	private void CheckAndMoveSpiralShurikenToEnv(UICardSimpleControl newlyAddedCard)
	{
		if (newlyAddedCard != null && newlyAddedCard.CardData != null && newlyAddedCard.CardData.id == "1024")
		{
			return;
		}

		List<UICardSimpleControl> attackCards = GetBagCardList(ECardType.attack);
		if (attackCards == null || attackCards.Count == 0)
		{
			return;
		}

		UICardSimpleControl spiralShuriken = null;
		for (int i = 0; i < attackCards.Count; i++)
		{
			UICardSimpleControl card = attackCards[i];
			if (card == null || card == newlyAddedCard)
			{
				continue;
			}

			if (card.CardData == null)
			{
				continue;
			}

			bool isSpiralShuriken = false;
			if (card.CardData.id == "1024")
			{
				isSpiralShuriken = true;
			}
			else if (card.CardEffect != null && card.CardEffect.Id == ECardEffectId.SpiralShuriken)
			{
				isSpiralShuriken = true;
			}

			if (isSpiralShuriken)
			{
				spiralShuriken = card;
				break;
			}
		}

		if (spiralShuriken == null)
		{
			return;
		}

		JoeyGameControl.Instance.AddGlobalDelayCall(() =>
		{
			MoveSpiralShurikenToEnvAsync(spiralShuriken).Forget();
		}, 0.1f);
	}

	private async UniTaskVoid MoveSpiralShurikenToEnvAsync(UICardSimpleControl spiralShuriken)
	{
		if (spiralShuriken == null || spiralShuriken.CardData == null)
		{
			return;
		}

		int spiralUniqueId = spiralShuriken.CardData.UniqueId;

		int cardTypeInt = (int)ECardType.attack;
		if (m_BagCardDict.TryGetValue(cardTypeInt, out List<UICardSimpleControl> cardList))
		{
			cardList.Remove(spiralShuriken);
		}

		await AddEnvCardFromBagAsync(spiralShuriken);

		// 50% 概率永久 +1 攻击：参考噬魂剑/噬魂手里剑写法（加攻 + 刷新 + 播特效）
		if (ControlUtil.IsRandomSucceed(YSpiralShuriken.TriggerPermanentAttackRatio))
		{
			UICardSimpleControl envSpiral = FindEnvCardControlByUniqueId(spiralUniqueId);
			if (envSpiral != null && envSpiral.CardData != null)
			{
				envSpiral.CardData.currentAttack += YSpiralShuriken.TriggerPermanentAttackBonus;
				envSpiral.RefreshCard();
				if (envSpiral.CacheTrans != null)
				{
					JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, envSpiral.CacheTrans, 1f);
				}
			}
			else
			{
				// 兜底：如果未能找到环境里的UI卡牌，至少保证数据层攻击+1
				spiralShuriken.CardData.currentAttack += YSpiralShuriken.TriggerPermanentAttackBonus;
				if (spiralShuriken.CacheTrans != null)
				{
					JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, spiralShuriken.CacheTrans, 1f);
				}
			}
		}

		spiralShuriken.Return();
	}
}

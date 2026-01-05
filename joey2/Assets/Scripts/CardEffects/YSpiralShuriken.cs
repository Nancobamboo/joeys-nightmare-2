using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YSpiralShuriken : YDealRandomEnemyEqualToAttack_OnTop
{
	public const int TriggerAttackBonus = 2;

	public YSpiralShuriken()
	{
		Id = ECardEffectId.SpiralShuriken;
	}
}

public partial class UIGamePhaseControl
{
	private static readonly Dictionary<int, int> s_SpiralShurikenLevelBonusByUniqueId = new Dictionary<int, int>();

	/// <summary>
	/// 清除“螺旋手里剑”在本关内由该效果累计增加的攻击（只回滚本脚本自己加的那部分）。
	/// </summary>
	private void ResetSpiralShurikenLevelBonuses()
	{
		if (m_DataJoeyPlayer == null)
		{
			return;
		}

		if (s_SpiralShurikenLevelBonusByUniqueId.Count == 0)
		{
			return;
		}

		List<int> keys = new List<int>(s_SpiralShurikenLevelBonusByUniqueId.Keys);
		for (int i = 0; i < keys.Count; i++)
		{
			int uniqueId = keys[i];
			int bonus = s_SpiralShurikenLevelBonusByUniqueId[uniqueId];
			if (bonus == 0)
			{
				continue;
			}

			Card c = m_DataJoeyPlayer.GetSelfCardDictData(uniqueId);
			if (c != null && c.id == "1024")
			{
				c.SetAttack(Mathf.Max(0, c.currentAttack - bonus));
				continue;
			}

			if (m_CardDict != null && m_CardDict.TryGetValue(uniqueId, out Card uiCard) && uiCard != null && uiCard.id == "1024")
			{
				uiCard.SetAttack(Mathf.Max(0, uiCard.currentAttack - bonus));
			}
		}

		s_SpiralShurikenLevelBonusByUniqueId.Clear();
	}

	/// <summary>
	/// 强制清理“螺旋手里剑”本关累计的临时加攻，避免跨关继承。
	/// </summary>
	public void ForceResetSpiralShurikenLevelBonuses()
	{
		ResetSpiralShurikenLevelBonuses();
	}

	private void AddSpiralShurikenLevelBonus(Card spiralCardData, int bonus)
	{
		if (spiralCardData == null)
		{
			return;
		}
		if (spiralCardData.id != "1024")
		{
			return;
		}
		if (bonus == 0)
		{
			return;
		}

		int uid = spiralCardData.UniqueId;
		if (uid != 0)
		{
			if (!s_SpiralShurikenLevelBonusByUniqueId.TryGetValue(uid, out int cur))
			{
				cur = 0;
			}
			s_SpiralShurikenLevelBonusByUniqueId[uid] = cur + bonus;
		}

		spiralCardData.SetAttack(Mathf.Max(0, spiralCardData.currentAttack + bonus));
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

		int cardTypeInt = (int)ECardType.attack;
		if (m_BagCardDict.TryGetValue(cardTypeInt, out List<UICardSimpleControl> cardList))
		{
			cardList.Remove(spiralShuriken);
		}

		AddSpiralShurikenLevelBonus(spiralShuriken.CardData, YSpiralShuriken.TriggerAttackBonus);

		await AddEnvCardFromBagAsync(spiralShuriken);
		spiralShuriken.Return();
	}
}

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
	// 该卡“其他武器入手→自身进入环境”触发时，获得 +2 攻击（仅当前关卡有效）
	// 由于进入环境的实现是“复用同一份 CardData，新建一个 Env 的 CardControl”，
	// 所以必须写入 CardData.currentAttack 才能在捡回手牌后仍然保留本关加成。
	public const int TRIGGER_ATTACK_BONUS = 2;

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
	// -------- 螺旋手里剑：关卡内临时攻击加成追踪（仅用于回滚“本关加成”） --------
	// key: Card.UniqueId, value: 本关累计加成值
	private static readonly Dictionary<int, int> s_SpiralShurikenLevelBonusByUniqueId = new Dictionary<int, int>();
	private static int s_SpiralShurikenLastLevelSeed = 0;

	/// <summary>
	/// 每次进入新关卡时清理（回滚）螺旋手里剑的“本关临时 +攻”加成。
	/// </summary>
	private void ResetSpiralShurikenLevelBonusesIfNeeded()
	{
		if (m_DataJoeyPlayer == null)
		{
			return;
		}

		int currentSeed = m_DataJoeyPlayer.levelRandomSeed;
		if (currentSeed == 0)
		{
			// 兜底：seed 未初始化时不处理，避免误回滚。
			return;
		}

		if (s_SpiralShurikenLastLevelSeed == currentSeed)
		{
			return;
		}

		s_SpiralShurikenLastLevelSeed = currentSeed;

		if (s_SpiralShurikenLevelBonusByUniqueId.Count == 0)
		{
			return;
		}

		foreach (var kv in s_SpiralShurikenLevelBonusByUniqueId)
		{
			int uniqueId = kv.Key;
			int bonus = kv.Value;
			if (bonus == 0) continue;

			// 优先从玩家自持卡牌数据里回滚（该数据跨关卡存在）
			Card c = m_DataJoeyPlayer.GetSelfCardDictData(uniqueId);
			if (c != null && c.id == "1024")
			{
				c.SetAttack(Mathf.Max(0, c.currentAttack - bonus));
				continue;
			}

			// 兜底：如果当前 UI 仍持有该 CardData，也做一次回滚
			if (m_CardDict != null && m_CardDict.TryGetValue(uniqueId, out Card uiCard) && uiCard != null && uiCard.id == "1024")
			{
				uiCard.SetAttack(Mathf.Max(0, uiCard.currentAttack - bonus));
			}
		}

		s_SpiralShurikenLevelBonusByUniqueId.Clear();
	}

	private void AddSpiralShurikenLevelBonus(Card spiralCardData, int bonus)
	{
		if (spiralCardData == null) return;
		if (spiralCardData.id != "1024") return;
		if (bonus == 0) return;

		// 记录本关累计加成，用于关卡切换时回滚
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

	/// <summary>
	/// 检查手牌中是否有螺旋手里剑，如果有则将其移动到环境
	/// </summary>
	private void CheckAndMoveSpiralShurikenToEnv(UICardSimpleControl newlyAddedCard)
	{
		// 关卡切换时，确保回滚上关的螺旋手里剑临时加成
		ResetSpiralShurikenLevelBonusesIfNeeded();

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

		// 在真正移动前再做一次关卡回滚检查（避免异步跨关卡）
		ResetSpiralShurikenLevelBonusesIfNeeded();

		// 先从手牌中移除
		int cardTypeInt = (int)ECardType.attack;
		if (m_BagCardDict.TryGetValue(cardTypeInt, out List<UICardSimpleControl> cardList))
		{
			cardList.Remove(spiralShuriken);
		}

		// 关键：只有“螺旋手里剑自身效果导致其进入环境”时，才获得 +2 攻击（本关有效）
		AddSpiralShurikenLevelBonus(spiralShuriken.CardData, YSpiralShuriken.TRIGGER_ATTACK_BONUS);

		// 调用AddEnvCardFromBagAsync移动到环境
		await AddEnvCardFromBagAsync(spiralShuriken);
		spiralShuriken.Return();
	}
}


// Scripts/CardEffects/Effects/YMagicPotion.cs
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YMagicPotion : YCardEffect
{
	public int baseExtra;

	// 可用的技能卡ID列表
	private static readonly List<string> SkillCardIds = new List<string>
	{
		"4001", // 连续攻击
		"4006", // 连环闪电
		"4007", // 匕首飞来
		"4008", // 丢盔弃甲
		"4010", // 鲜血寄存
		"4011", // 宁死不屈
		"4012", // 浴血奋战
		"4013", // 魔力召唤
		"4014", // 烈焰火球
		"4015", // 冰霜魔法
	};

	public YMagicPotion(int baseExtra)
	{
		this.baseExtra = Mathf.Max(1, baseExtra);
		Id = ECardEffectId.MagicPotion;
	}

	public override float UseItem()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
		}

		if (SkillCardIds.Count == 0) return 0.3f;

		// 使用确定性种子以保证可重放
		DataJoeyPlayer dataJoeyPlayer = DataSystem.Instance.GetDataJoeyPlayer();
		int seed = dataJoeyPlayer.levelRandomSeed + dataJoeyPlayer.giftBoxUseCounter;
		dataJoeyPlayer.giftBoxUseCounter++;

		// 保存当前随机状态
		Random.State oldState = Random.state;
		Random.InitState(seed);

		// 随机选择技能卡并加入手牌
		for (int i = 0; i < baseExtra; i++)
		{
			int idx = Random.Range(0, SkillCardIds.Count);
			string selectedCardId = SkillCardIds[idx];

			Card newCard = DataSystem.Instance.CreateCard(selectedCardId);
			if (newCard != null)
			{
				YActionSystem.Instance.DispatchAction(EActionId.AddCardToBagFromSelect, newCard);
			}
		}

		// 恢复随机状态
		Random.state = oldState;

		return 0.3f;
	}

	public override float OnRemoveCard()
	{
		return 0f;
	}
}


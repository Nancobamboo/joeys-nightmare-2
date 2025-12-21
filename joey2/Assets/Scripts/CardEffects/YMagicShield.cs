// Scripts/CardEffects/Effects/YMagicShield.cs
// 魔法盾效果：玩家每使用一张技能牌，当前手牌顶部的防御牌防御力增加baseExtra
// 只对手牌顶部的这张牌生效，使用后或游戏重新开始不保留额外防御力
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YMagicShield : YDefaultEffect
{
	public int baseExtra;

	public YMagicShield(int baseExtra)
	{
		Id = ECardEffectId.MagicShield;
		this.baseExtra = baseExtra;
	}

	public override float UseDefence(bool isOverflow = false)
	{
		return base.UseDefence(isOverflow);
	}
}

public partial class UIGamePhaseControl
{
	/// <summary>
	/// 技能牌使用后，检查手牌顶部防御牌是否有魔法盾效果，有则增加临时防御力
	/// </summary>
	void MagicShieldOnSkillUsed()
	{
		UICardSimpleControl topDefenceCard = GetLastBagCard(ECardType.defence);
		if (topDefenceCard == null || topDefenceCard.CardEffect == null)
		{
			return;
		}

		// 检查是否是魔法盾效果
		if (topDefenceCard.CardEffect.Id != ECardEffectId.MagicShield)
		{
			return;
		}

		YMagicShield magicShield = topDefenceCard.CardEffect as YMagicShield;
		if (magicShield == null)
		{
			return;
		}

		Debug.Log($"YMagicShield: Skill card used, adding {magicShield.baseExtra} defence to top shield");
		// 使用 AddEffectValue 添加临时防御力加成，使用后自动清除
		topDefenceCard.AddEffectValue(EEffectType.Defence, magicShield.baseExtra);
		topDefenceCard.RefreshCard();

		// 播放增益特效
		if (topDefenceCard.CacheTrans != null)
		{
			JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, topDefenceCard.CacheTrans, 1f);
		}
	}
}


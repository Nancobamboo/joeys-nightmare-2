// Scripts/CardEffects/Effects/YMagicSword.cs
// 魔法剑效果：玩家每使用一张技能牌，当前手牌顶部的武器牌攻击力增加baseExtra
// 只对手牌顶部的这张牌生效，使用后或游戏重新开始不保留额外攻击力
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YMagicSword : YDefaultEffect
{
	public int baseExtra;

	public YMagicSword(int baseExtra)
	{
		Id = ECardEffectId.MagicSword;
		this.baseExtra = baseExtra;
	}

	public override float OnDealDamage()
	{
		return base.OnDealDamage();
	}
}

public partial class UIGamePhaseControl
{
	/// <summary>
	/// 技能牌使用后，检查手牌顶部武器牌是否有魔法剑效果，有则增加临时攻击力
	/// </summary>
	void MagicSwordOnSkillUsed()
	{
		UICardSimpleControl topWeaponCard = GetLastBagCard(ECardType.attack);
		if (topWeaponCard == null || topWeaponCard.CardEffect == null)
		{
			return;
		}

		// 检查是否是魔法剑效果
		if (topWeaponCard.CardEffect.Id != ECardEffectId.MagicSword)
		{
			return;
		}

		YMagicSword magicSword = topWeaponCard.CardEffect as YMagicSword;
		if (magicSword == null)
		{
			return;
		}

		Debug.Log($"YMagicSword: Skill card used, adding {magicSword.baseExtra} attack to top weapon");
		// 使用 AddEffectValue 添加临时攻击力加成，使用后自动清除
		topWeaponCard.AddEffectValue(EEffectType.Damage, magicSword.baseExtra);
		topWeaponCard.RefreshCard();

		// 播放增益特效
		if (topWeaponCard.CacheTrans != null)
		{
			JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, topWeaponCard.CacheTrans, 1f);
		}
	}
}
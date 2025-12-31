// Scripts/CardEffects/Effects/YThrowWeaponToStack_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YHolyLight_UseSkill : YCardEffect
{
	public YHolyLight_UseSkill()
	{
		Id = ECardEffectId.HolyLight_UseSkill;
	}
	public override float UseSkill()
	{
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
            
            // 调用消灭skeleton monster的逻辑
            YActionSystem.Instance.DispatchAction(EActionId.KillSkeletonMonster, CardControl);
            
            return 0.3f;
        }
		return base.UseSkill();
	}

}

public partial class UIGamePhaseControl
{
	// 静态配置skeleton类型的monster card id
	private static readonly HashSet<string> SkeletonMonsterIds = new HashSet<string> { "5003", "5004", "5011" };

	void KillSkeletonMonster(object[] paraArray)
	{
		UICardSimpleControl cardControl = (UICardSimpleControl)paraArray[0];
		KillSkeletonMonster(cardControl);
	}

	async void KillSkeletonMonster(UICardSimpleControl cardControl)
	{
		if (cardControl == null || m_EnvCardDict == null || m_EnvCardDict.Count == 0)
		{
			return;
		}

		// 收集所有最外层的skeleton monster
		List<(UICardSimpleControl card, int envIndex)> skeletonMonsters = new List<(UICardSimpleControl, int)>();

		// 遍历所有列（0-4）
		for (int envIndex = 0; envIndex < m_EnvPanels.Count; envIndex++)
		{
			UICardSimpleControl lastCard = GetLastEnvCard(envIndex);
			if (lastCard != null && 
			    lastCard.CardType == ECardType.monster && 
			    SkeletonMonsterIds.Contains(lastCard.CardData.id))
			{
				skeletonMonsters.Add((lastCard, envIndex));
			}
		}

		// 如果有skeleton monster，随机消灭一个
		if (skeletonMonsters.Count > 0)
		{
			int randomIndex = Random.Range(0, skeletonMonsters.Count);
			var target = skeletonMonsters[randomIndex];
			
			// 参考AttackRandomEnemy的实现，使用目标monster的token
			int currentHealth = target.card.CardData.currentHealth;
			await DealDamageToEnvCard(target.card, currentHealth, target.envIndex, EEffectType.Damage, default);
		}
	}
}

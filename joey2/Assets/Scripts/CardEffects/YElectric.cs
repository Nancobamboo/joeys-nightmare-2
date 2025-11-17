// Scripts/CardEffects/Effects/YElectric.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YElectric : YCardEffect
{
	public int baseExtra;

	public YElectric(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.Electric;
	}

	public override float UseSkill()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
			return maxDelayTime > 0f ? maxDelayTime : base.UseSkill();
		}
		return base.UseSkill();
	}

	public override float OnRemoveCard()
	{
		int damage = GetEffectValue(EEffectType.Damage);
		YActionSystem.Instance.DispatchAction(EActionId.TakeAllEnemyDamage, damage);
		return 0f;
	}

	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.Damage)
		{
			return baseExtra;
		}
		return base.GetEffectValue(effectType);
	}
}

public partial class UIGamePhaseControl
{
	public async UniTask TakeAllEnemyDamage(int damage)
	{
		if (damage <= 0)
		{
			return;
		}

		List<int> enemyIndices = new List<int>();
		foreach (KeyValuePair<int, List<UICardSimpleControl>> kvp in m_EnvCardDict)
		{
			if (kvp.Value != null && kvp.Value.Count > 0)
			{
				UICardSimpleControl lastCard = kvp.Value[kvp.Value.Count - 1];
				if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardType == ECardType.monster)
				{
					enemyIndices.Add(kvp.Key);
				}
			}
		}

		foreach (int envIndex in enemyIndices)
		{
			UICardSimpleControl enemyCardControl = GetLastEnvCard(envIndex);
			if (enemyCardControl != null)
			{
				await DealDamageToEnvCard(enemyCardControl, damage, envIndex, EEffectType.Electric);
			}
		}
	}
}


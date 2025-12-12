// Scripts/CardEffects/Effects/YElectric.cs
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
		JoeyGameControl.Instance.QueueAction(EActionId.TakeAllEnemyDamage, baseExtra);
		return 0f;
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
				CancellationToken token = GetOrCreateCardToken(enemyCardControl);
				await DealDamageToEnvCard(enemyCardControl, damage, envIndex, EEffectType.Electric, token);
				RemoveCardCts(enemyCardControl);
			}
		}
	}
}


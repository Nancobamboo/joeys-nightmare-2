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

		Debug.Log($"Electric: TakeAllEnemyDamage called with damage = {damage}");

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

		Debug.Log($"Electric: Found {enemyIndices.Count} enemies to damage");

		foreach (int envIndex in enemyIndices)
		{
			UICardSimpleControl enemyCardControl = GetLastEnvCard(envIndex);
			if (enemyCardControl != null)
			{
				Debug.Log($"Electric: Damaging {enemyCardControl.CardData.cardName} at envIndex {envIndex}, HP before: {enemyCardControl.CardData.currentHealth}");
				
				CancellationToken token = GetOrCreateCardToken(enemyCardControl);
				// DealDamageToEnvCard 返回 true 表示敌人死亡，内部已调用 RemoveCardCts
				bool enemyKilled = await DealDamageToEnvCard(enemyCardControl, damage, envIndex, EEffectType.Electric, token);
				
				// 只有敌人存活时才需要清理 CTS 和应用易伤效果
				if (!enemyKilled)
				{
					if (enemyCardControl != null && enemyCardControl.gameObject.activeSelf && 
					    enemyCardControl.CardData.currentHealth > 0)
					{
						Debug.Log($"Electric: {enemyCardControl.CardData.cardName} survived with HP: {enemyCardControl.CardData.currentHealth}, applying vulnerable");
						ApplyElectricVulnerable(enemyCardControl);
					}
					RemoveCardCts(enemyCardControl);
				}
				else
				{
					Debug.Log($"Electric: {enemyCardControl.CardData?.cardName ?? "null"} died or destroyed");
				}
			}
		}
	}

	private void ApplyElectricVulnerable(UICardSimpleControl enemyCard)
	{
		if (enemyCard != null && enemyCard.CardType == ECardType.monster)
		{
			// Add vulnerable debuff for 1 turn (set to 2 because it decreases at end of current turn)
			// Turn flow: Apply buff (value=2) -> End of turn (-1, value=1) -> Next turn (effect active) -> End of next turn (-1, value=0)
			int vulnerableDuration = 2;
			int currentVulnerable = enemyCard.GetBuffValue(EBuffType.Vulnerable);
			if (currentVulnerable < vulnerableDuration)
			{
				enemyCard.AddBuff(EBuffType.Vulnerable, vulnerableDuration);
				Debug.Log($"Electric: Applied Vulnerable debuff to {enemyCard.CardData.cardName} for 1 turn (value={vulnerableDuration})");
			}
			else
			{
				Debug.Log($"Electric: {enemyCard.CardData.cardName} already has Vulnerable debuff for {currentVulnerable} turns");
			}
			
			// Vulnerable VFX is handled centrally in UICardSimpleControl.AddBuff(EBuffType.Vulnerable)
		}
	}
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YApeWine : YCardEffect
{
    public YApeWine()
    {
        Id = ECardEffectId.ApeWine;
    }

    public override float UseItem()
    {
		YActionSystem.Instance.DispatchAction(EActionId.DoubleLastWeaponAttack, CardControl);
		return 0f;
    }
}

public partial class UIGamePhaseControl
{
	public void DoubleLastWeaponAttack(UICardSimpleControl cardControl)
	{
		if (cardControl == null)
		{
			return;
		}
		UICardSimpleControl weaponCard = GetLastBagCard(ECardType.attack);
		if (weaponCard == null)
		{
			return;
		}
		int damage = weaponCard.CardData.attack + weaponCard.CardEffect.GetEffectValue(EEffectType.Damage);
		Debug.Log("DoubleLastWeaponAttack attack: " + weaponCard.CardData.attack);
		Debug.Log("DoubleLastWeaponAttack effect damage: " + weaponCard.CardEffect.GetEffectValue(EEffectType.Damage));
		AddEffectValueToBagCard(ECardType.attack, EEffectType.Damage, damage);
	}
}


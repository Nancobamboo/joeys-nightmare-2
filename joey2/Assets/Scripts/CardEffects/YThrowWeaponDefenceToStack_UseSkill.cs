// Scripts/CardEffects/Effects/YThrowWeaponToStack_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YThrowWeaponDefenceToStack_UseSkill : YCardEffect
{
    public YThrowWeaponDefenceToStack_UseSkill()
    {
        Id = ECardEffectId.ThrowWeaponDefenceToStack_UseSkill;
    }


    public override float OnRemoveCard()
    {
        YActionSystem.Instance.DispatchAction(EActionId.ThrowWeaponDefenceToEnv, CardControl);
        return 0f;
    }
}

public partial class UIGamePhaseControl
{
    public void ThrowWeaponDefenceToEnv(UICardSimpleControl cardControl)
    {

		UICardSimpleControl weaponCard = GetLastBagCard(ECardType.attack);
        // TODO judge whether the weapon card is fist
        if (weaponCard != null)
        {
            AddEnvCardFromBag(weaponCard);
            weaponCard.Return();
		}
		

        UICardSimpleControl defenceCard = GetLastBagCard(ECardType.defence);
        // TODO judge whether the weapon card is fist
        if (defenceCard != null)
        {
            AddEnvCardFromBag(defenceCard);
            defenceCard.Return();
        }
    }
}
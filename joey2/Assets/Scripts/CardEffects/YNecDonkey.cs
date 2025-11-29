using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YNecDonkey : YDefaultEffect
{

    bool m_IsCalled = false;
    public YNecDonkey()
    {
        Id = ECardEffectId.NecDonkey;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            Card cardData = CardControl.CardData;
            if (cardData.currentHealth > 0 && cardData.currentHealth <= cardData.health / 2 && !m_IsCalled)
            {                
                // TODO due to the env index is fixed, set env size = 5
                int envIndex = CardControl.EnvIndex;
                int leftEnvIndex = Mathf.Clamp(envIndex - 1, 0, 4);
                int rightEnvIndex = Mathf.Clamp(envIndex + 1, 0, 4);
                YActionSystem.Instance.DispatchAction(EActionId.AddCardToSpecifiedEnv, CardControl, "5011", leftEnvIndex);
                YActionSystem.Instance.DispatchAction(EActionId.AddCardToSpecifiedEnv, CardControl, "5011", rightEnvIndex);
                m_IsCalled = true;
            }
        }
        return base.OnTakeDamage(effectType);
    }

}
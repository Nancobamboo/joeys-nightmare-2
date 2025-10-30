using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HPManager : MonoBehaviour
{
    public Text heartText;
    public Text attackText;
    public Text defenceText;

    
    // Start is called before the first frame update
    void OnEnable()
    {
        GameEvents.OnHPChanged += OnHPChanged;
        GameEvents.OnAttackChanged += OnAttackChanged;
        GameEvents.OnDefenceChanged += OnDefenceChanged;
    }
    void OnDisable()
    {
        GameEvents.OnHPChanged -= OnHPChanged;
        GameEvents.OnAttackChanged -= OnAttackChanged;
        GameEvents.OnDefenceChanged -= OnDefenceChanged;
    }

    void OnHPChanged(int hp)
    {
        heartText.text = PData.Instance.playerHealth.ToString();
        if (PData.Instance.playerHealth <= 0)
        {
            Debug.Log("PlayerLost");
            PhaseManager.Instance.SetGamePhase(GamePhase.battleEnd);
        }
    }

    void OnAttackChanged(int attack)
    {
        if (attackText != null)
        {
            attackText.text = PData.Instance.playerAttack.ToString();
        }
    }

    void OnDefenceChanged(int defence)
    {
        if (defenceText != null)
        {
            defenceText.text = PData.Instance.playerDefence.ToString();
        }
    }
}

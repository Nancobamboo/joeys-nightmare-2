using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HPManager : MonoBehaviour
{
    public Text heartText;

    
    // Start is called before the first frame update
    void OnEnable()
    {
        GameEvents.OnHPChanged += OnHPChanged;
    }
    void OnDisable()
    {
        GameEvents.OnHPChanged -= OnHPChanged;
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
}

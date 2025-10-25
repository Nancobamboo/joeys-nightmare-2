using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    playerAction,enemyAction
}

public class BattleManager : MonoBehaviour
{
    public GamePhase gamePhase = GamePhase.playerAction;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

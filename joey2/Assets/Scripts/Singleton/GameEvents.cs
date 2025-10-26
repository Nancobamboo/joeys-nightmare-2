using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class GameEvents
{
    public static event Action<GamePhase, GamePhase> OnGamePhaseChanged;
    public static void RaiseGamePhaseChanged(GamePhase oldGamePhase, GamePhase newGamePhase)
    {
        OnGamePhaseChanged?.Invoke(oldGamePhase, newGamePhase);
    }
    // 你也可以改成 Action<string, CardState> 只传 id 和状态
    public static event Action<GameObject> OnCardClicked;
    
    public static void RaiseCardClicked(GameObject cardGameObject)
    {
        OnCardClicked?.Invoke(cardGameObject);
    }
}
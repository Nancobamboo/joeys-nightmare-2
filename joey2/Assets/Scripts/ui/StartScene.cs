using UnityEngine;

public class StartScene : MonoBehaviour
{
    /// <summary>
    /// Load Battle scene - called from button OnClick event
    /// </summary>
    public void LoadBattleScene()
    {
        Debug.Log("StartScene: LoadBattleScene called!");
        
        // Reset level to 1 when starting new game from menu
        PData.Instance.currentLevel = 1;
        
        SceneLoader.Instance.LoadScene("Battle");
    }
}

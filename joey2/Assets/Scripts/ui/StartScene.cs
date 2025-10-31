using UnityEngine;

public class StartScene : MonoBehaviour
{
    /// <summary>
    /// Load Battle scene - called from button OnClick event
    /// </summary>
    public void LoadBattleScene()
    {
        Debug.Log("StartScene: LoadBattleScene called!");
        SceneLoader.Instance.LoadScene("Battle");
    }
}

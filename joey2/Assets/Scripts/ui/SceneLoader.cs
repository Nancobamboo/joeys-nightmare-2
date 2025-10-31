using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoSingleton<SceneLoader>
{
    /// <summary>
    /// Load scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneLoader: Scene name is empty!");
            return;
        }

        // Check if scene exists in build settings
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity");
        if (sceneIndex == -1)
        {
            Debug.LogError($"SceneLoader: Scene '{sceneName}' not found in build settings!");
            Debug.LogError("SceneLoader: Make sure the scene is added to Build Settings (File > Build Settings)");
            return;
        }

        Debug.Log($"SceneLoader: Loading scene '{sceneName}' (index: {sceneIndex})");
        SceneManager.LoadScene(sceneName);
    }
}


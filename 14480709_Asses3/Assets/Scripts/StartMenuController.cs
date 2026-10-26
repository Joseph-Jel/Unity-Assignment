using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private string level1SceneName;
    [SerializeField] private string level2SceneName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadLevel1()
    {
        if (!string.IsNullOrWhiteSpace(level1SceneName))
        {
            SceneManager.LoadScene(level1SceneName);
        }    
    }

    // Update is called once per frame
    public void LoadLevel2()
    {
        if (!string.IsNullOrWhiteSpace(level2SceneName))
        {
            SceneManager.LoadScene(level2SceneName);
        }
    }
}

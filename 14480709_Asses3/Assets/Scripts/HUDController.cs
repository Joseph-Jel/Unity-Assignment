using UnityEngine;
using UnityEngine.SceneManagement;
public class HUDController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ExitToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
}

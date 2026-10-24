using UnityEngine;
using System.Collections;
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource introMusic;
    [SerializeField] private AudioSource normalGhostMusic;

    
    private void Start()
    {
        StartCoroutine(PlayIntroThenNormalMusic());
    }

    private IEnumerator PlayIntroThenNormalMusic()
    {
        if (introMusic == null || normalGhostMusic == null)
        {
            Debug.LogError("AudioManager: AudioSource reference is missing");
            yield break;
        }

        introMusic.loop = false;
        normalGhostMusic.loop = true;

        introMusic.Play();

        float waitTime = 3f;

        if (introMusic.clip != null)
        {
            waitTime = Mathf.Min(3f, introMusic.clip.length);
        }

        yield return new WaitForSeconds(waitTime);

        introMusic.Stop();
        normalGhostMusic.Play();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

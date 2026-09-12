using UnityEngine;
using System.Collections;
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource introMusic;
    [SerializeField] private AudioSource normalGhostMusic;

    
    void Start()
    {
        StartCoroutine(PlayIntroThenNormalMusic());
    }

    private IEnumerator PlayIntroThenNormalMusic()
    {
        introMusic.Play();

        float startTime = Time.time;

        while (introMusic.isPlaying && Time.time - startTime < 3f)
        {
            yield return null;
        }

        introMusic.Stop();
        normalGhostMusic.Play();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

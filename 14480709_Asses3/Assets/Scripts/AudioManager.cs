using UnityEngine;
using System.Collections;
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip normalGhostMusic;
    [SerializeField] private AudioClip scaredGhostMusic;
    [SerializeField] private AudioClip deadGhostMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip wallCollisionSound;
    [SerializeField] private AudioClip cherryEatSound;
    [SerializeField] private AudioClip ghostEatSound;
    [SerializeField] private AudioClip pacStudentDeathSound;

    public void PlayIntroMusic()
    {
        PlayMusic(introMusic, false);
    }

    public void PlayNormalMusic()
    {
        PlayMusic(normalGhostMusic, true);
    }

    public void PlayScaredMusic()
    {
        PlayMusic(scaredGhostMusic, true);
    }

    public void PlayDeadMusic()
    {
        PlayMusic(deadGhostMusic, true);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    private void PlayMusic(AudioClip clip, bool loop)
    {
        if (clip == null || musicSource == null)
        {
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return; 
        }

        musicSource.clip = clip;
        musicSource.loop = loop;

        musicSource.Play();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayWallCollision()
    {
        PlaySFX(wallCollisionSound);
    }

    public void PlayCherryEat()
    {
        PlaySFX(cherryEatSound);
    }

    public void PlayGhostEat()
    {
        PlaySFX(ghostEatSound);
    }

    public void PlayPacStudentDeath()
    {
        PlaySFX(pacStudentDeathSound);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

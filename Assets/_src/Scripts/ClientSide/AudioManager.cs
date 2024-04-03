using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource music, sfx1, sfx2, oneshot;
    public List<AudioClip> musicList;
    public List<AudioClip> sfxList;
    public Toggle musicToggle, sfxToggle, musicToggleMobile, sfxToggleMobile;
    public Slider musicSlider, sfxSlider, musicSliderMobile, sfxSliderMobile;
    public float musicVolume, sfxVolume;

    public List<Button> allButtons;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        musicVolume = 0.5f;
        sfxVolume = 0.5f;
        VolumeMusic(musicVolume);
        VolumeSFX(sfxVolume);
        PlaySFX(0);
        StopResumeSFX(true);
        musicToggle.onValueChanged.AddListener(x => MusicMute(x));
        musicSlider.onValueChanged.AddListener(x => VolumeMusic(x));
        sfxToggle.onValueChanged.AddListener(x => SfxMute(x));
        sfxSlider.onValueChanged.AddListener(x => VolumeSFX(x));
        musicToggleMobile.onValueChanged.AddListener(x => MusicMute(x));
        musicSliderMobile.onValueChanged.AddListener(x => VolumeMusic(x));
        sfxToggleMobile.onValueChanged.AddListener(x => SfxMute(x));
        sfxSliderMobile.onValueChanged.AddListener(x => VolumeSFX(x));
        allButtons.ForEach(x => x.onClick.AddListener(() => PlayOneShot(0)));
    }

    public void PlayMusic()
    {
        music.Play();
    }

    public void PlaySFX(int sfx)
    {
        switch (sfx)
        {
            case 0:
                sfx1.Play();
                break;
            case 1:
                sfx2.Play();
                break;
        }
    }
    public void MusicMute(bool mute)
    {
        if (!mute) { music.mute = true; }
        else { music.mute = false; }
    }

    public void SfxMute(bool mute)
    {
        if (!mute) { sfx1.mute = true; sfx2.mute = true; oneshot.mute = true; }
        else { sfx1.mute = false; sfx2.mute = false; oneshot.mute = false; }
    }

    public void StopResumeMusic(bool pause)
    {
        if (pause)
        {
            music.Pause();
        }
        else
        {
            music.UnPause();
        }
    }

    public void StopResumeSFX(bool pause)
    {
        if (pause)
        {
            sfx1.Pause();
            sfx2.Pause();
        }
        else
        {
            sfx1.UnPause();
            sfx2.UnPause();
        }
    }

    public void VolumeMusic(float volume) 
    {
        music.volume = volume; 
        musicSlider.value = volume; 
        musicSliderMobile.value = volume; 
        volume = volume / 2;
        musicVolume = volume; 
    }

    public void VolumeSFX(float volume)
    {
        sfxVolume = volume; 
        sfxSlider.value = volume; 
        sfxSliderMobile.value = volume; 
        volume = volume / 2;
        sfx1.volume = volume; 
        sfx2.volume = volume; 
        oneshot.volume = volume;
    }

    public void SetMusic(int music) { this.music.clip = musicList[music]; }

    public void SetSFX1(int sfx) { sfx1.clip = sfxList[sfx]; }

    public void SetSFX2(int sfx) { sfx2.clip = sfxList[sfx]; }

    public void PlayOneShot(int sfx) { oneshot.PlayOneShot(sfxList[sfx]); }
}

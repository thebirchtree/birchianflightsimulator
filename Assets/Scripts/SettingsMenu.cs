using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void SetsfxhiVolume(float volume)
    {
        audioMixer.SetFloat("sfxVol", volume);
    }
    public void SetmusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVol", volume);
    }
    public void SetJetVolume(float volume)
    {
        audioMixer.SetFloat("jetVol", volume);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}

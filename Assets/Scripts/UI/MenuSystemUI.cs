using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSystem : MonoBehaviour
{
    public GameObject menuPanel;
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool menuActive = false;

    void Start()
    {
        menuPanel.SetActive(false);
        menuActive = false;

        // initialize slider callbacks
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);

        // optional: pull saved prefs
        musicSlider.value = PlayerPrefs.GetFloat("musicVol", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVol", 0.5f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            menuPanel.SetActive(menuActive);
            menuActive = !menuActive;
        }
    }

    public void QuitGame()
    {
        // persist settings
        PlayerPrefs.SetFloat("musicVol", musicSlider.value);
        PlayerPrefs.SetFloat("sfxVol", sfxSlider.value);
        PlayerPrefs.Save();

        SaveSystem.Instance.Save();
        Application.Quit();
    }
}


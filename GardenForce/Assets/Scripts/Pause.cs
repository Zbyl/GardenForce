using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public Button firstButton;
    public Toggle kidsModeToggle;
    public GameObject kidsModeIndicator;
    GameObject menuPanel;
    void Awake()
    {
        menuPanel = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        kidsModeToggle.SetIsOnWithoutNotify(Map.kidsMode);
    }

    void Update()
    {
        if (Input.GetButtonDown("Escape"))
        {
            TriggerPause();
        }
        kidsModeIndicator.SetActive(Map.kidsMode);
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void TriggerPause()
    {
        if (Time.timeScale > 0)
        {
            menuPanel.SetActive(true);
            Map.instance.StopCoroutine("Tick");
            Time.timeScale = 0;
            firstButton.OnSelect(null);
            firstButton.Select();
        }
        else
        {
            menuPanel.SetActive(false);
            Map.instance.StartCoroutine("Tick");
            Time.timeScale = 1;
        }
    }
    public void Quit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
    public void SetKidsMode(bool enable)
    {
        Debug.Log("Kids mode: " + enable);
        Map.kidsMode = enable;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public Button firstButton;
    GameObject menuPanel;
    void Awake()
    {
        menuPanel = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (Input.GetButtonDown("Escape"))
        {
            TriggerPause();
        }
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
        Application.Quit();
    }
}

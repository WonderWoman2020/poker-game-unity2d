using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net.NetworkInformation;
//using System.Diagnostics;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button playButton;

    // Start is called before the first frame update
    void Start()
    {
        //SceneManager.LoadScene("MainMenu");
    }

    public void OnExitButton()
    {
        Application.Quit();
    }
    public void OnSettingsButton()
    {
        Debug.Log("Settings");
    }

    public void OnPlayButton()
    {
        if(MyGameManager.Instance.MainPlayer == null)
            SceneManager.LoadScene("LoginMenu");
        else
            SceneManager.LoadScene("PlayMenu");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

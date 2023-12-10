using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSettingsMenu : MonoBehaviour
{

    [SerializeField] private Button soundButton;
    private bool isMuted = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSoundButton()
    {
        if(isMuted == false)
        {
            isMuted = true;
            AudioListener.pause = true;
            soundButton.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = "on";
        }
        else
        {
            isMuted = false;
            AudioListener.pause = false;
            soundButton.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = "off";
        }
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

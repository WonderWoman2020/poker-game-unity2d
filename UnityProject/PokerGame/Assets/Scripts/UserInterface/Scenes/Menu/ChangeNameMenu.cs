using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using UnityEngine.SceneManagement;

//using System.Net.Sockets;

//using PokerGameClasses;
//using pGrServer;


public class ChangeNameMenu : MonoBehaviour
{

    [SerializeField] private Button changeNameButton;
    [SerializeField] private Button backButton;

    public GameObject PopupWindow;

    private string newName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangeNameButton()
    {
        Debug.Log("Change");
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }


    public void ReadNewName(string newName)
    {
        if (newName.Length == 0)
        {
            this.newName = null;
            return;
        }

        this.newName = newName;
        Debug.Log(this.newName);
    }


    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }

}

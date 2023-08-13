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
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField newNameField;

    public GameObject PopupWindow;

    private string newName;

    // Start is called before the first frame update
    void Start()
    {
        this.passwordField.contentType = TMP_InputField.ContentType.Password;
        this.passwordField.asteriskChar = '*';
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangeNameButton()
    {
        Debug.Log(newNameField.text); //tresc pola name
        Debug.Log(passwordField.text); //tresc pola haslo

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using UnityEngine.SceneManagement;

//using System.Net.Sockets;

//using PokerGameClasses;
//using pGrServer;

public class DeleteAccount : MonoBehaviour
{

    [SerializeField] private Button deleteAccountButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField passwordField;

    public GameObject PopupWindow;

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

    public void OnDeleteAccountButton()
    {
        // TODO zapytanie o usuniêcie tu wrzuciæ
        // jeœli powiedzie siê usuwanie, to pokazaæ popup informacyjny, ¿e siê uda³o usun¹æ
        Debug.Log(passwordField.text); //tresc pola haslo
        if(passwordField.text == "haslo")
        {
            //Usun konto
            SceneManager.LoadScene("MainMenu");
        }

    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}

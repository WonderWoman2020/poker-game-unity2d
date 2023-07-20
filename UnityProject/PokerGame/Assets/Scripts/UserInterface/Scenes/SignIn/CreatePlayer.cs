using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Text.RegularExpressions;

using PokerGameClasses;

// Ekran do rejestracji nowego u�ytkownika
public class CreatePlayer : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backToMenuButton;

    [SerializeField] private TMP_InputField newPasswordField;
    [SerializeField] private TMP_InputField confirmPasswordField;

    // informacje o b��dach, komunikaty dla gracza
    public GameObject PopupWindow;

    // dane z formularza
    private string playerNick;
    private string password1;
    private string password2;
    private string login;

    // Start is called before the first frame update
    void Start()
    {
        this.playerNick = null;
        this.password1 = null;
        this.password2 = null;
        this.login = null;

        this.newPasswordField.contentType = TMP_InputField.ContentType.Password;
        this.newPasswordField.asteriskChar = '*';

        this.confirmPasswordField.contentType = TMP_InputField.ContentType.Password;
        this.confirmPasswordField.asteriskChar = '*';
    }

    // Update is called once per frame
    void Update()
    {
        // wci�ni�cie enter robi to samo co wci�ni�cie przycisku 'Create'
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (this.playerNick != null && this.password1 != null && this.password2 != null && this.login != null)
                this.OnCreateButton();
        }
    }

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnCreateButton()
    {
        Debug.Log("Create button was clicked.");
        if (this.login != null)
        {
            if (this.playerNick != null)
            {
                if (this.password1 == this.password2 && this.password1!=null)
                {
                    Debug.Log("Parameters are good.");
                    StartCoroutine(SendNewUser());
                }
                else
                    ShowWrongInputPopup("Passwords are different.");
            }
            else
                ShowWrongInputPopup("Add your nick.");
        }
        else
            ShowWrongInputPopup("Add your login");
    }

    // rejestracja nowego gracza w bazie (mamy to tylko w wersji z API webowym od Unity)
    // TODO przepisa� kiedy� na API C#, �eby m�c korzysta� z tego te� w konsolowym kliencie do test�w?
    // TODO doda� kiedy� do osobnej klasy
    IEnumerator SendNewUser()
    {
        var request = new UnityWebRequest("https://3rh988512b.execute-api.eu-central-1.amazonaws.com/default/addAccount", "POST");

        string str = "{\"login\":\"" + this.login + "\",\"nick\":\"" + this.playerNick + "\",\"password\":\"" + this.password1 + "\"}";

        byte[] body = System.Text.Encoding.ASCII.GetBytes(str);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log(request.downloadHandler.text);
        var responseCode = Regex.Match(request.downloadHandler.text, @"\d+").Value;
        if (responseCode == "401") // login exist
        {
            ShowWrongInputPopup("Login exists.");
        }
        else if(responseCode == "205") // ok - przejd� do ekranu logowania
        {
            SceneManager.LoadScene("Login");
        }
        else //failed
        {
            ShowWrongInputPopup("Server error.");
        }
    }

    public void ShowWrongInputPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void ReadPlayerNick(string nick)
    {
        if (nick.Length == 0)
        {
            this.playerNick = null;
            return;
        }

        this.playerNick = nick;
        Debug.Log(this.playerNick);
    }

    public void ReadLogin(string login)
    {
        if (login.Length == 0)
        {
            this.login = null;
            return;
        }

        this.login = login;
        Debug.Log(this.login);
    }
    public void ReadPassword1(string password)
    {
        if (password.Length == 0)
        {
            this.password1 = null;
            return;
        }

        this.password1 = password;
        Debug.Log(this.password1);
    }
    public void ReadPassword2(string password)
    {
        if (password.Length == 0)
        {
            this.password2 = null;
            return;
        }

        this.password2 = password;
        Debug.Log(this.password2);
    }
}

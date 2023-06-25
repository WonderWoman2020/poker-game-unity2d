using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using UnityEngine.SceneManagement;

//using System.Net.Sockets;

//using PokerGameClasses;
//using pGrServer;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Button changePasswordButton;
    [SerializeField] private Button changeNameButton;
    [SerializeField] private Button deleteAccountButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button backButton;

    public GameObject PopupWindow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangePasswordButton()
    {
        SceneManager.LoadScene("ChangePasswordMenu");
    }

    public void OnChangeNameButton()
    {
        SceneManager.LoadScene("ChangeNameMenu");
    }

    public void OnDeleteAccountButton()
    {
        // TODO sprawdzaæ, czy gracz jest zalogowany i nie wpuszczaæ go tu jeœli nie jest (+ Popup z odpowiednim info czemu nie móg³ wejœæ)
        SceneManager.LoadScene("DeleteAccount");
    }

    public void OnLogoutButton()
    {

    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}

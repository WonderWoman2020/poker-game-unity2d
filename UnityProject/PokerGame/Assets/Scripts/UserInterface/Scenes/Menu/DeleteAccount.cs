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

    public GameObject PopupWindow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDeleteAccountButton()
    {
        // TODO zapytanie o usuni�cie tu wrzuci�
        // je�li powiedzie si� usuwanie, to pokaza� popup informacyjny, �e si� uda�o usun��
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

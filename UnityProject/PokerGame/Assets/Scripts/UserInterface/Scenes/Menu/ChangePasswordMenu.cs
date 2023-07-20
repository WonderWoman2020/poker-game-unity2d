using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using UnityEngine.SceneManagement;

//using System.Net.Sockets;

//using PokerGameClasses;
//using pGrServer;



public class ChangePasswordMenu : MonoBehaviour
{

    [SerializeField] private Button changePasswordButton;
    [SerializeField] private Button backButton;

    [SerializeField] private TMP_InputField currentPasswordField;
    [SerializeField] private TMP_InputField newPasswordField;
    [SerializeField] private TMP_InputField confirmPasswordField;

    public GameObject PopupWindow;

    private string currentPassword;
    private string newPassword;
    private string confirmPassword;

    // Start is called before the first frame update
    void Start()
    {
        this.currentPasswordField.contentType = TMP_InputField.ContentType.Password;
        this.currentPasswordField.asteriskChar = '*';

        this.newPasswordField.contentType = TMP_InputField.ContentType.Password;
        this.newPasswordField.asteriskChar = '*';

        this.confirmPasswordField.contentType = TMP_InputField.ContentType.Password;
        this.confirmPasswordField.asteriskChar = '*';
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangePasswordButton()
    {
        Debug.Log("Change");
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    public void ReadCurrentPassword(string currentPswd)
    {
        if (currentPswd.Length == 0)
        {
            this.currentPassword = null;
            return;
        }

        this.currentPassword = currentPswd;
        Debug.Log(this.currentPassword);
    }

    public void ReadNewPassword(string newPswd)
    {
        if (newPswd.Length == 0)
        {
            this.newPassword = null;
            return;
        }

        this.newPassword = newPswd;
        Debug.Log(this.newPassword);
    }

    public void ReadConfirmPassword(string confirmPswd)
    {
        if (confirmPswd.Length == 0)
        {
            this.confirmPassword = null;
            return;
        }

        this.confirmPassword = confirmPswd;
        Debug.Log(this.confirmPassword);
    }

    void ShowPopup(string text)
    {
        var popup = Instantiate(PopupWindow, transform.position, Quaternion.identity, transform);
        popup.GetComponent<TextMeshProUGUI>().text = text;
    }
}

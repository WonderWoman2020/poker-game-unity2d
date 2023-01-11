using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class EndOfHand : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitToMenuButton;
    [SerializeField] private Button kickBotButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnContinueButton()
    {
        SceneManager.LoadScene("Table");
    }
    public void OnExitToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void OnKickBotButton()
    {
        SceneManager.LoadScene("Table");
    }
}


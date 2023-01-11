using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class JoinTable : MonoBehaviour
{
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backToMenuButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnJoinButton()
    {
        SceneManager.LoadScene("Table");
    }
    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }
}

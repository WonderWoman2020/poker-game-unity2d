using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickAudio : MonoBehaviour
{

    public AudioSource buttonSound;
    // Start is called before the first frame update
    public void PlaySoundEffect()
    {
        buttonSound.Play();
        DontDestroyOnLoad(buttonSound);
    }

}

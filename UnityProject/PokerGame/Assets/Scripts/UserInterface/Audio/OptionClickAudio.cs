using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionClickAudio : MonoBehaviour
{
    public AudioSource optionSound;
    // Start is called before the first frame update
    public void PlaySoundEffect()
    {
        optionSound.Play();
        DontDestroyOnLoad(optionSound);
    }
}

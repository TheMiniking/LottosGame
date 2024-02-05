using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimRTP : MonoBehaviour
{
    [SerializeField] Animator rtpAnim;
    [SerializeField] Toggle rtpTogle;
    public void PlayRTP()
    {
        rtpAnim.Play(rtpTogle.isOn ? "Open": "Close");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject fire;

    void Start()
    {
        Walking(false);
        Crash(false);
    }

    public void Walking(bool v)
    {
        Debug.Log("Walking");
        anim.SetBool("walking", v);
    }
    public void Crash(bool v)
    {
        Debug.Log("Crash");
        anim.SetBool("crash", v);
        anim.SetBool("walking", false);
        //explosion.SetActive(v);
        //fire.SetActive(v);
    }
}

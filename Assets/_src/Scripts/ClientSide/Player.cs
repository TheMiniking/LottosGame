using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject fire;

    void Start()
    {
    }

    public void Walking(bool v)
    {
        // Debug.Log("Animation:"+( v? "Walking": "Lost"));
        anim.Play(v ? "Walking" : "Lost");
        //anim.SetBool("walking", v);
    }
    public void Crash(bool v)
    {
        Debug.Log("Crash");
        anim.Play("Lost");
        //explosion.SetActive(v);
        //fire.SetActive(v);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
    [SerializeField]public GameScreen screen;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Colidiu"); 
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine(screen.Open(screen.boxT[0].currentBox));
        }
    }
}

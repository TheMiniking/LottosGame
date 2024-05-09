using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxControl : MonoBehaviour
{
    [SerializeField] float velocity = 1f;
    [SerializeField] Material parallaxMaterial;

    private void Update()
    {
        var p = parallaxMaterial.GetFloat("_RealTimeVelocity");
        parallaxMaterial.SetFloat("_RealTimeVelocity",GameManager.Instance.fundoOnMove? p + velocity * Time.deltaTime : p);
    }
}

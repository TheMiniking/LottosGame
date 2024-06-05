using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class ParallaxControl : MonoBehaviour
{

    [SerializeField] bool useRawImage = false;
    [SerializeField] float velocity = 1f;
    [SerializeField] Material parallaxMaterial;
    [SerializeField] RawImage image;
    [SerializeField] Tween tween;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        useRawImage = image != null;
    }

    void Start()
    {
        if(useRawImage) PlayAnim();
    }

    private void FixedUpdate()
    {
        if (useRawImage) 
        {
            var stop = GameManager.Instance.fundoOnMove ;
            if (!stop)
            {
                tween.Stop();
            }
            else
            {
                if(!tween.isAlive)
                    PlayAnim();
            }
        }
        else
        {
            var p = parallaxMaterial.GetFloat("_RealTimeVelocity");
            parallaxMaterial.SetFloat("_RealTimeVelocity",GameManager.Instance.fundoOnMove? p + (velocity*GameManager.Instance.paralaxVelocity)*Time.deltaTime: p);
        }
    }

    void PlayAnim()
    {
       tween = Tween.Custom(image, image.uvRect.x, image.uvRect.x + 1, (velocity * GameManager.Instance.paralaxVelocity), (t, x) => image.uvRect = new Rect(x, image.uvRect.y, image.uvRect.width, image.uvRect.height), Ease.Linear,cycleMode: CycleMode.Restart);
    }
}

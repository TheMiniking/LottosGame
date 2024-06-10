using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MovingBox : MonoBehaviour
{
    [SerializeField] RectTransform thisBox;
    [SerializeField] float initPosition;
    [SerializeField] float loopPosition;
    [SerializeField] float duration;
    [SerializeField] float durationLoop;
    [SerializeField] public float boxDistance;
    [SerializeField] GameObject boxIndicador,boxIndicador2,paraquedas;
    [SerializeField] TMP_Text indicador,indicador2;
    [Range(0, 1f), SerializeField] float margin =0.02f;
    [Range(0, 1f), SerializeField] float offScreamDistance =1f;
    [SerializeField] float screenLimit;
    [SerializeField] float maxDistance;
    [SerializeField] Slider slider;
    [SerializeField] Tween tween;
    [SerializeField] Animator popUp;
    [SerializeField] public TMP_Text popUpText;

    private void OnEnable()
    {
        GoToOrigin();
        boxDistance = thisBox.anchoredPosition.x;
        screenLimit = (Screen.width - (margin * Screen.width) + (Screen.width / 3)) / 2;
        GoToCenter();
    }

    private void OnDisable()
    {
        boxIndicador.SetActive(false);
    }

    private void Update()
    {
        boxDistance = thisBox.anchoredPosition.x;
        indicador.text = $"{boxDistance/100:00.0} M";
        indicador2.text = $"{boxDistance/100:00.0} M";
        if (boxDistance < initPosition)
        {
            if(boxDistance > screenLimit)
            { 
                boxIndicador.SetActive(true);
                boxIndicador2.SetActive(false);
                paraquedas.SetActive(true);
                float sliderValue = ( boxDistance - screenLimit) / (maxDistance - screenLimit);
                slider.value = sliderValue;
            }
            else
            {
                paraquedas.SetActive(false);
                boxIndicador.SetActive(false);
                boxIndicador2.SetActive(true);
            }
        }
        if(boxDistance == 0)
        {
            popUp.Play("PopUp");
            GoToLoop();
        }
    }
    public void GoToCenter()
    {
        tween = Tween.UIAnchoredPositionX(thisBox, endValue: 0, duration: duration,Ease.Linear);
        boxIndicador.SetActive(true);
    }

    public void GoToOrigin()
    {
       tween =Tween.UIAnchoredPositionX(thisBox, endValue: initPosition, duration: 0);
    }
    public void GoToLoop()
    {
        tween = Tween.UIAnchoredPositionX(thisBox, endValue: loopPosition, duration: 0);
        boxDistance = loopPosition;
        tween = Tween.UIAnchoredPositionX(thisBox, endValue: 0, duration: durationLoop, Ease.Linear);
        boxIndicador.SetActive(true);
    }

    public void Stop()
    {
        tween.Stop();
    }
}

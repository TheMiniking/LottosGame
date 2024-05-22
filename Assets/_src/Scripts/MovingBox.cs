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
    [SerializeField] float duration;
    [SerializeField] public float boxDistance;
    [SerializeField] GameObject boxIndicador,boxIndicador2,paraquedas;
    [SerializeField] TMP_Text indicador,indicador2;
    [Range(0, 1f), SerializeField] float margin =0.02f;
    [Range(0, 1f), SerializeField] float offScreamDistance =1f;
    [SerializeField] float screenLimit;
    [SerializeField] float maxDistance;
    [SerializeField] Slider slider;


    private void OnEnable()
    {
        GoToOrigin();
        boxDistance = thisBox.anchoredPosition.x;
        screenLimit = (Screen.width - (margin * Screen.width) + (Screen.width / 3)) / 2;
        GoToCenter();
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
            GoToOrigin();
            thisBox.gameObject.SetActive(false);
        }
    }
    public void GoToCenter()
    {
        Tween.UIAnchoredPositionX(thisBox, endValue: 0, duration: duration,Ease.Linear);
        boxIndicador.SetActive(true);
    }

    public void GoToOrigin()
    {
        Tween.UIAnchoredPositionX(thisBox, endValue: initPosition, duration: 0);
    }

}

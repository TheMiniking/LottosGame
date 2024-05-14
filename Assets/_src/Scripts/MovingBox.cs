using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovingBox : MonoBehaviour
{
    [SerializeField] RectTransform thisBox;
    [SerializeField] float initPosition;
    [SerializeField] float duration;
    [SerializeField] public float boxDistance;
    [SerializeField] GameObject boxIndicador,boxIndicador2;
    [SerializeField] TMP_Text indicador,indicador2;
    [Range(0, 1f), SerializeField] float margin =0.02f;

    private void OnEnable()
    {
        GoToOrigin();
        GoToCenter();
    }

    private void Update()
    {
        boxDistance = thisBox.anchoredPosition.x;
        indicador.text = $"{boxDistance/100:00.0} M";
        indicador2.text = $"{boxDistance/100:00.0} M";
        if (boxDistance < initPosition)
        {
            if(boxDistance > (Screen.width - (margin * Screen.width))/ 2)
            { 
                boxIndicador.SetActive(true);
                boxIndicador2.SetActive(false);
            }
            else
            {
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

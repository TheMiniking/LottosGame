using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingParalaxObjects : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector3 initialPosition;
    [SerializeField] float finalPosition;
    [SerializeField] List<GameObject> movingObjectsVariants;
    [SerializeField] GameObject specialFX;
    [SerializeField] Tween tween;
    [SerializeField] bool onMoviment;

    private void Start()
    {
        var r = Random.Range(0, movingObjectsVariants.Count);
        movingObjectsVariants.ForEach(x => x.GetComponent<CanvasGroup>().alpha= movingObjectsVariants[r] == x ? 1 : 0);
        
    }
    private void Update()
    {
        GetComponent<RectTransform>().position = GameManager.Instance.fundoOnMove ? new Vector3(GetComponent<RectTransform>().position.x - ((speed * GameManager.Instance.paralaxVelocity)* Time.deltaTime), GetComponent<RectTransform>().position.y, GetComponent<RectTransform>().position.z) : GetComponent<RectTransform>().position;
        if (GetComponent<RectTransform>().position.x <= finalPosition) ResetPosition();
        //if (GameManager.Instance.fundoOnMove && !onMoviment)
        //{
        //    onMoviment = true;
        //    tween = Tween.UIAnchoredPositionX(GetComponent<RectTransform>(), endValue: -Screen.width, duration: speed)
        //        .OnComplete(() => ResetPosition());
        //}
        //else if (!GameManager.Instance.fundoOnMove && onMoviment)
        //{
        //    onMoviment = false;
        //    tween.Stop();
        //}

    }

    void ResetPosition()
    {
        this.transform.position = initialPosition;
        var r = Random.Range(0, movingObjectsVariants.Count);
        movingObjectsVariants.ForEach(x => x.GetComponent<CanvasGroup>().alpha = movingObjectsVariants[r] == x ? 1 : 0);
        if (specialFX == null) return;
        if (r != 0) specialFX.SetActive(Random.Range(0, 5)==0);
        //tween = Tween.UIAnchoredPositionX(GetComponent<RectTransform>(), endValue: -Screen.width, duration: speed)
        //    .OnComplete(() => ResetPosition());

    }
}

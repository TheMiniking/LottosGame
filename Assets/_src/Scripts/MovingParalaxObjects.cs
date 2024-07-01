using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MovingParalaxObjects : MonoBehaviour
{
    [SerializeField] float speedy;
    [SerializeField] Vector3 initialPosition;
    [SerializeField] float finalPosition;
    [SerializeField] List<GameObject> movingObjectsVariants;
    [SerializeField] GameObject specialFX;
    [SerializeField] Tween tween;
    [SerializeField] bool onMoviment;

    [SerializeField] RectTransform thisTransform;
    [SerializeField] float duration;
    [SerializeField] float adicionalDistance;
    [SerializeField] Vector3 endValue;

    private void Start()
    {
        var r = Random.Range(0, movingObjectsVariants.Count);
        thisTransform = GetComponent<RectTransform>();
        movingObjectsVariants.ForEach(x => x.GetComponent<CanvasGroup>().alpha= movingObjectsVariants[r] == x ? 1 : 0);
        duration = 5;
        initialPosition.x = Screen.width + adicionalDistance;
        endValue = new Vector3(-(Screen.width + adicionalDistance), initialPosition.y, 0);
    }
    private void Update()
    {
        if (GameManager.Instance.fundoOnMove && !onMoviment)
        {
            onMoviment = true;
            tween = Tween.LocalPositionAtSpeed(thisTransform, endValue: endValue, speedy, ease: Ease.Linear)
                    .OnComplete(target: this, target => {ResetPosition();});
        }
        else if (!GameManager.Instance.fundoOnMove && onMoviment)
        {
            onMoviment = false;
            tween.Stop();
        }
    }

    void ResetPosition()
    {
        this.transform.localPosition = initialPosition;
        var r = Random.Range(0, movingObjectsVariants.Count);
        movingObjectsVariants.ForEach(x => x.GetComponent<CanvasGroup>().alpha = movingObjectsVariants[r] == x ? 1 : 0);
        if (r != 0 && specialFX != null) specialFX.SetActive(Random.Range(0, 5) == 0);
        tween = Tween.LocalPositionAtSpeed(thisTransform, endValue: endValue, speedy, ease: Ease.Linear)
                .OnComplete(target: this, target => ResetPosition());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingParalaxObjects : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector3 initialPosition;
    [SerializeField] float finalPosition;
    [SerializeField] List<GameObject> movingObjectsVariants;

    private void Start()
    {
        var r = Random.Range(0, movingObjectsVariants.Count);
        movingObjectsVariants.ForEach(x => x.SetActive(movingObjectsVariants[r] == x));
    }
    private void Update()
    {
        GetComponent<RectTransform>().position = GameManager.Instance.fundoOnMove? new Vector3(GetComponent<RectTransform>().position.x - (speed * Time.deltaTime), GetComponent<RectTransform>().position.y, GetComponent<RectTransform>().position.z) : GetComponent<RectTransform>().position;
        if (GetComponent<RectTransform>().position.x <= finalPosition) ResetPosition();
    }

    void ResetPosition()
    {
        this.transform.position = initialPosition;
        var r = Random.Range(0, movingObjectsVariants.Count);
        movingObjectsVariants.ForEach(x => x.SetActive(movingObjectsVariants[r] == x));
    }
}

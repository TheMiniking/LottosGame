using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ElementMove : MonoBehaviour
{
    [SerializeField] List<RectTransform> sky;
    [SerializeField] List<RectTransform> bg;
    [SerializeField] List<RectTransform> road;
    [SerializeField] Transform content;
    List<RectTransform> activesSky=new List<RectTransform>();
    List<RectTransform> activesBg = new List<RectTransform>();
    List<RectTransform> activesRoad = new List<RectTransform>();
    [SerializeField] float layer1Speed;
    [SerializeField] float layer2Speed;
    [SerializeField] float layer3Speed;
    [SerializeField] float resetPosition;
    [SerializeField] float initPosition;
    void Start()
    {
        var s = Instantiate(sky[Random.Range(0, sky.Count)], content);
        var s2 = Instantiate(sky[Random.Range(0, sky.Count)], content);
        var b = Instantiate(bg[Random.Range(0, bg.Count)], content);
        var b2 = Instantiate(bg[Random.Range(0, bg.Count)], content);
        var r = Instantiate(road[Random.Range(0, road.Count)], content);
        var r2 = Instantiate(road[Random.Range(0, road.Count)], content);


        Debug.Log("b.anchoredPosition.y " + b.anchoredPosition.y);

        s.anchoredPosition = new Vector2(0, s.anchoredPosition.y);
        b.anchoredPosition = new Vector2(0, s.anchoredPosition.y);
        r.anchoredPosition = new Vector2(0, s.anchoredPosition.y);


        activesSky.Add(s);
        activesBg.Add(b);
        activesRoad.Add(r);


        s2.anchoredPosition = new Vector2(-740, s2.anchoredPosition.y);
        b2.anchoredPosition = new Vector2(-740, s2.anchoredPosition.y);
        r2.anchoredPosition = new Vector2(-740, s2.anchoredPosition.y);

        activesSky.Add(s2);
        activesBg.Add(b2);
        activesRoad.Add(r2);
    }

    void Update()
    {
        for (int i = 0; i < activesSky.Count; i++)
        {
            float newPosition = activesSky[i].anchoredPosition.x - layer1Speed * Time.deltaTime;

            var pos = new Vector2(newPosition, activesSky[i].anchoredPosition.y);

            if (pos.x < resetPosition)
            {
                pos.x = initPosition;
            }
            else if (pos.x <= -740 * 2)
            {
                Destroy(activesSky[i]);
                activesSky[i] = Instantiate(sky[Random.Range(0, sky.Count)], content);
            }
            activesSky[i].anchoredPosition = pos;
        }
        for (int i = 0; i < activesBg.Count; i++)
        {
            float newPosition = activesBg[i].anchoredPosition.x - layer2Speed * Time.deltaTime;

            var pos = new Vector3(newPosition, activesBg[i].anchoredPosition.y);

            if (pos.x < resetPosition)
            {
                pos.x = initPosition;
            }
            else if (pos.x <= -740 * 2)
            {
                int z = activesBg[i].transform.GetSiblingIndex();
                Destroy(activesBg[i]);
                activesBg[i] = Instantiate(bg[Random.Range(0, bg.Count)], content);
                activesBg[i].transform.SetSiblingIndex(z);
            }
            activesBg[i].anchoredPosition = pos;
        }
        for (int i = 0; i < activesRoad.Count; i++)
        {
            float newPosition = activesRoad[i].anchoredPosition.x - layer3Speed * Time.deltaTime;

            var pos = new Vector3(newPosition, activesRoad[i].anchoredPosition.y);

            if (pos.x < resetPosition)
            {
                pos.x = initPosition;
            }
            else if (pos.x <= -740 * 2)
            {
                Destroy(activesRoad[i]);
                activesRoad[i] = Instantiate(road[Random.Range(0, road.Count)], content);
            }
            activesRoad[i].anchoredPosition = pos;
        }
    }
}

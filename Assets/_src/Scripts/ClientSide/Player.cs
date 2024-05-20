using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject fire;
    [SerializeField] int tankNum;
    [SerializeField] bool lastMovingStatus = false;

    void Start()
    {

    }

    private void Update()
    {
        if(GameManager.Instance.fundoOnMove != lastMovingStatus)
        {
            lastMovingStatus = GameManager.Instance.fundoOnMove;
            Walking(lastMovingStatus);
        }
    }

    public void Walking(bool v)
    {
        anim.Play(v ? "Walking" : "Lost");
    }
    public void Crash(bool v)
    {
        Debug.Log("Crash");
        anim.Play("Lost");
    }
}

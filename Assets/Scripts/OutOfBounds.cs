using KartGame.Track;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : KartRepositionTrigger
{
    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("player out of bounds");
        if (collision.gameObject == (movable as GameObject))
        {
            Debug.Log("player out of bounds");
            trackManager.ReplaceMovable(m_Movable);
        }
    }


    public void OnTouchGround(RaycastHit hit)
    {
        if (hit.transform.gameObject.layer == (LayerMask)9)
        {
            trackManager.ReplaceMovable(m_Movable);
        }

    }
}

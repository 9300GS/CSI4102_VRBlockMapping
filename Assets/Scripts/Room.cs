using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    
    [SerializeField] public List<ResetPoint> connectedResetPoints;
    [HideInInspector] public Vector3 position;

    public GameObject border_current;
    public GameObject border_next;

    void Awake()
    {
        if(connectedResetPoints.Count == 0)
            Debug.Log("Error : Room (" + this.gameObject.name + ") is missing Reset Point");

        position = transform.position;
    }

    public void SetAllResetPointsActive(bool enable)
    {
        foreach(ResetPoint r in connectedResetPoints)
        {
            r.gameObject.SetActive(enable);
        }
    }

    public void MarkAsCurrent(bool mark)
    {
        border_current.SetActive(mark);
    }

    public void MarkAsNext(bool mark)
    {
        border_next.SetActive(mark);
    }

    
}

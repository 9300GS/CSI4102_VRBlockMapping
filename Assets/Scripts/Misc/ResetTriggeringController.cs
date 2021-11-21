using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Redirection;
using Valve.VR;

public class ResetTriggeringController : MonoBehaviour
{
    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");

    RedirectionManager rm;
    public Room currentRoom;
    public Room nextRoom;
    public ResetPoint closestResetPoint;


    [HideInInspector]
    List<float> distancesFromResets;

    [HideInInspector]
    public Vector3 triggeredResetPosition;

    [HideInInspector]
    public int minIndex;

    void Start()
    {
        rm = GameObject.Find("Redirected User").GetComponent<RedirectionManager>();
    }

    public void UpdateRTC()
    {
        distancesFromResets = new List<float>();
        currentRoom = rm.currentRoom;

        for(int i = 0; i < currentRoom.connectedResetPoints.Count; i++)
        {
            float distance = (currentRoom.connectedResetPoints[i].transform.position - rm.currPos).magnitude;
            distancesFromResets.Add(distance);
        }
        minIndex = distancesFromResets.IndexOf(distancesFromResets.Min());
        
        closestResetPoint = currentRoom.connectedResetPoints[minIndex];
        nextRoom = closestResetPoint.GetNextRoom(currentRoom);

        bool watchingRP = false;
        Vector3 closestRelativePosition = closestResetPoint.transform.position - currentRoom.transform.position;
        Vector3 direction = rm.currDir;
        if(Mathf.Abs(closestRelativePosition.x) > Mathf.Abs(closestRelativePosition.z))
        {
            if(closestRelativePosition.x * direction.x > 0) watchingRP = true;
        }
        else
        {
            if(closestRelativePosition.z * direction.z > 0) watchingRP = true;
        }

        if(distancesFromResets.Min() < 0.2f && !rm.inReset && watchingRP)
        {
            GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = true;
            nextRoom.MarkAsNext(true);
            

            if(grabPinchAction.GetStateDown(SteamVR_Input_Sources.Any))
            {
                GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = false;
                rm.setControllerTriggered();
                triggeredResetPosition = rm.currPos;
            }
        }
        else
        {
            GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = false;
            nextRoom.MarkAsNext(false);
        }
        
    }

    public void ChangetoNextRoom(Room nextRoom_)
    {
        rm.currentRoom.MarkAsCurrent(false);
        nextRoom_.MarkAsCurrent(true);
        nextRoom_.MarkAsNext(false);

        rm.currentRoom = nextRoom_;
        Debug.Log("Change to "+ rm.currentRoom.gameObject.name);
    }

}

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


    [HideInInspector]
    List<float> distancesFromResets;

    [HideInInspector]
    public Vector3 triggeredResetPosition;

    [HideInInspector]
    public int minIndex;

    [HideInInspector]
    public float dirFlipper;

    void Start()
    {
        rm = GameObject.Find("Redirected User").GetComponent<RedirectionManager>();
        dirFlipper = 1f;
    }

    // Update is called once per frame
    public void UpdateRTC()
    {
        distancesFromResets = new List<float>();
        currentRoom = rm.currentRoom;

        for(int i = 0; i < currentRoom.connectedResetPoints.Count; i++)
        {
            float distance = (currentRoom.connectedResetPoints[i].position - rm.currPosReal).magnitude;
            distancesFromResets.Add(distance);
        }
        minIndex = distancesFromResets.IndexOf(distancesFromResets.Min());

        nextRoom = (currentRoom.connectedResetPoints[minIndex]).GetNextRoom(currentRoom);

        bool watchingRP = false;
        Vector3 closestRelativePosition = currentRoom.connectedResetPoints[minIndex].position - currentRoom.position;
        Vector3 direction = rm.currDirReal * dirFlipper;
        if(Mathf.Abs(closestRelativePosition.x) > Mathf.Abs(closestRelativePosition.z))
        {
            if(closestRelativePosition.x * direction.x > 0) watchingRP = true;
        }
        else
        {
            if(closestRelativePosition.z * direction.z > 0) watchingRP = true;
        }

        if(distancesFromResets.Min() < 0.3 && !rm.inReset && watchingRP)
        {
            GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = true;
            nextRoom.MarkAsNext(true);
            

            if(grabPinchAction.GetStateDown(SteamVR_Input_Sources.Any))
            {
                GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = false;
                rm.setControllerTriggered();
                triggeredResetPosition = rm.currPosReal;
            }
        }
        else
        {
            GameObject.Find("TurnReadySign").GetComponent<Canvas>().enabled = false;
            nextRoom.MarkAsNext(false);
        }
        
    }

    public void ChangetoNextRoom()
    {
        rm.currentRoom.MarkAsCurrent(false);
        nextRoom.MarkAsCurrent(true);
        nextRoom.MarkAsNext(false);

        rm.currentRoom = nextRoom;
        Debug.Log("Change to "+ currentRoom.gameObject.name);

        dirFlipper *= -1f;
    }

}

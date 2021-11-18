using UnityEngine;
using System.Collections;
using Redirection;

public class RFL_Resetter : Resetter {

    float overallInjectedRotation;
    
    Transform instanceHUD;

    Vector3 virtualCenter;

    ResetTriggeringController rtc;

    public override bool IsResetRequired()
    {
        return !isUserFacingAwayFromWall();
    }

    public override void InitializeReset()
    {
        overallInjectedRotation = 0;

        virtualCenter = redirectionManager.trackedSpace.position; //current Plane 위치
        rtc = GameObject.Find("Redirected User").GetComponent<ResetTriggeringController>();
        SetHUD();
    }

    public override void ApplyResetting()
    {
        if (Mathf.Abs(overallInjectedRotation) < 180)
        {
            float remainingRotation = redirectionManager.deltaDir > 0 ? 180 - overallInjectedRotation : -180 - overallInjectedRotation; // The idea is that we're gonna keep going in this direction till we reach objective
            
            if (Mathf.Abs(remainingRotation) < Mathf.Abs(redirectionManager.deltaDir))
            {
                InjectRotation(remainingRotation);
                overallInjectedRotation += remainingRotation;

                Room currentRoom = rtc.currentRoom;
                Room nextRoom = rtc.nextRoom;
                ResetPoint rp = currentRoom.connectedResetPoints[rtc.minIndex];
                
                Vector3 expectedVector = nextRoom.transform.position - currentRoom.transform.position;
                Vector3 movedVector = redirectionManager.trackedSpace.position - virtualCenter;
                Vector3 flippedVector = (rp.transform.position - currentRoom.transform.position) * 2;

                GameObject.Find("Terrain").gameObject.transform.position -= expectedVector - movedVector;   // Resetting Error Fix
                GameObject.Find("Terrain").gameObject.transform.position -= expectedVector - flippedVector; // Reset Point Position Error Fix

                redirectionManager.OnResetEnd();
            }
            else
            {
                InjectRotation(redirectionManager.deltaDir);
                overallInjectedRotation += redirectionManager.deltaDir;
            }
        }
    }

    public override void FinalizeReset()
    {
        GameObject.Find("TurnAroundSign").GetComponent<Canvas>().enabled = false;
        rtc.ChangetoNextRoom();
    }

    public void SetHUD()
    {
        GameObject.Find("TurnAroundSign").GetComponent<Canvas>().enabled = true;
    }
    
}

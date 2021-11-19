using UnityEngine;
using System.Collections;
using Redirection;

public class RFL_Resetter : Resetter {

    float overallInjectedRotation;
    
    Transform instanceHUD;

    Vector3 virtualCenter;
    Vector3 userError;
    Vector3 resetPointError;

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

        userError = 2 * (redirectionManager.currPos - rtc.closestResetPoint.transform.position);
        resetPointError = 2 * (rtc.closestResetPoint.transform.position - virtualCenter) + virtualCenter
                            - rtc.nextRoom.transform.position;

        SetHUD();
    }

    public override void ApplyResetting()
    {
        if (Mathf.Abs(overallInjectedRotation) < 180)
        {
            float remainingRotation = redirectionManager.deltaDir > 0 ? 180 - overallInjectedRotation : -180 - overallInjectedRotation; // The idea is that we're gonna keep going in this direction till we reach objective
            
            if (Mathf.Abs(remainingRotation) >= Mathf.Abs(redirectionManager.deltaDir))
            {
                InjectRotation(redirectionManager.deltaDir);
                float multiplier = 1f;
                if(overallInjectedRotation != 0) multiplier = overallInjectedRotation / Mathf.Abs(overallInjectedRotation);
                GameObject.Find("Terrain").gameObject.transform.position
                    += multiplier * (redirectionManager.deltaDir / 180f) * (userError + resetPointError);

                overallInjectedRotation += redirectionManager.deltaDir;
            }
            else
            {
                InjectRotation(remainingRotation);
                overallInjectedRotation += remainingRotation;
                
                virtualCenter = redirectionManager.trackedSpace.position;

                GameObject.Find("Terrain").gameObject.transform.position
                    += virtualCenter - rtc.nextRoom.transform.position;
                
                redirectionManager.OnResetEnd();
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

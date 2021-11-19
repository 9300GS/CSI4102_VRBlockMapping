using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redirection;

public class RedirectionManager : MonoBehaviour {

    public enum MovementController { Keyboard, AutoPilot, Tracker };

    [Tooltip("How user movement is controlled.")]
    public MovementController MOVEMENT_CONTROLLER;

    [Tooltip("The game object that is being physically tracked (probably user's head)")]
    public Transform headTransform;
    
    [HideInInspector] public Transform body;
    [HideInInspector] public Transform duplicatedBody;
    [HideInInspector] public Transform trackedSpace;

    [HideInInspector] public Resetter resetter;
    [HideInInspector] public ResetTrigger resetTrigger;
    [HideInInspector] public SimulationManager simulationManager;
    [HideInInspector] public HeadFollower bodyHeadFollower;
    [HideInInspector] public ResetTriggeringController rtc;
    public BlockLoader blockLoader;

    [HideInInspector] public Vector3 currPos, currPosReal, prevPos, prevPosReal;
    [HideInInspector] public Vector3 currDir, currDirReal, prevDir, prevDirReal;
    [HideInInspector] public Vector3 deltaPos;
    [HideInInspector] public float deltaDir;
    [HideInInspector] public Transform targetWaypoint;


    [HideInInspector] public bool inReset = false;
    
    [HideInInspector] public bool tilingMode = false;

    [HideInInspector] public bool controllerTriggered = false;


    public Room currentRoom;


    void Awake()
    {
        if(!blockLoader.LoadInfo()) Debug.Log("Failed Loading");
        blockLoader.LoadBlocks();

        currentRoom = blockLoader.startingRoom;
        currentRoom.MarkAsCurrent(true);

        body = transform.Find("Body");
        duplicatedBody = GameObject.Find("DuplicatedBody").transform;
        trackedSpace = transform.Find("Tracked Space");

        GetSimulationManager();
        SetReferenceForSimulationManager();
        simulationManager.Initialize();

        rtc = GetComponent<ResetTriggeringController>();

        GetResetter();
        GetResetTrigger();

        GetBodyHeadFollower();
        SetReferenceForResetter();
        SetReferenceForResetTrigger();
        SetBodyReferenceForResetTrigger();

        SetReferenceForBodyHeadFollower();

        // The rule is to have RedirectionManager call all "Awake"-like functions that rely on RedirectionManager as an "Initialize" call.
        resetTrigger.Initialize();
        // Resetter needs ResetTrigger to be initialized before initializing itself
        if (resetter != null)
            resetter.Initialize();
        else
        {
            MOVEMENT_CONTROLLER = MovementController.Tracker;
        }
    }

	void Start () {
        UpdatePreviousUserState();
	}

    void FixedUpdate()
    {
        UpdateCurrentUserState();
        CalculateStateChanges();

        rtc.UpdateRTC();

        // BACK UP IN CASE UNITY TRIGGERS FAILED TO COMMUNICATE RESET (Can happen in high speed simulations)
        //if (resetter != null && !inReset && resetter.IsUserOutOfBounds() && !tilingMode)
        if (resetter != null && !inReset && false && !tilingMode)
        {
            Debug.LogWarning("Reset Aid Helped!");
            OnResetTrigger();
        }
        else if (resetter != null && !inReset && controllerTriggered && tilingMode)
        {
            //Debug.Log(controllerTriggered);
            this.controllerTriggered = false;
            Debug.LogWarning("Reset Aid Helped!");
            OnResetTrigger();
        }

        if (inReset)
        {
            if (resetter != null)
            {
                resetter.ApplyResetting();
            }
        }

        UpdatePreviousUserState();

        UpdateBodyPose();
        UpdateDuplicatedBodyPose();
    }

    void UpdateDuplicatedBodyPose()
    {
        duplicatedBody.position = currPosReal + new Vector3(-200f, 0f, 200f);
        duplicatedBody.rotation = Quaternion.LookRotation(Utilities.FlattenedDir3D(currDirReal.normalized), Vector3.up);
    }

    void UpdateBodyPose()
    {
        body.position = Utilities.FlattenedPos3D(headTransform.position);
        body.rotation = Quaternion.LookRotation(Utilities.FlattenedDir3D(headTransform.forward), Vector3.up);
    }

    void SetReferenceForResetter()
    {
        if (resetter != null)
            resetter.redirectionManager = this;
    }

    void SetReferenceForResetTrigger()
    {
        if (resetTrigger != null)
            resetTrigger.redirectionManager = this;
    }

    void SetBodyReferenceForResetTrigger()
    {
        if (resetTrigger != null && body != null)
        {
            // NOTE: This requires that getBody gets called before this
            resetTrigger.bodyCollider = body.GetComponentInChildren<CapsuleCollider>();
            // Debug.Log("resetTrigger.bodyCollider.radius: "+resetTrigger.bodyCollider.radius);
        }
    }

    void SetReferenceForSimulationManager()
    {
        if (simulationManager != null)
        {
            simulationManager.redirectionManager = this;
        }
    }

    void SetReferenceForBodyHeadFollower()
    {
        if (bodyHeadFollower != null)
        {
            bodyHeadFollower.redirectionManager = this;
        }
    }

    void GetResetter()
    {
        resetter = this.gameObject.GetComponent<Resetter>();
        if (resetter == null)
            this.gameObject.AddComponent<RFL_Resetter>();
        resetter = this.gameObject.GetComponent<Resetter>();
    }

    void GetResetTrigger()
    {
        resetTrigger = this.gameObject.GetComponentInChildren<ResetTrigger>();
    }

    void GetSimulationManager()
    {
        simulationManager = this.gameObject.GetComponent<SimulationManager>();
    }

    void GetBodyHeadFollower()
    {
        bodyHeadFollower = body.GetComponent<HeadFollower>();
    }

    void GetTargetWaypoint()
    {
        targetWaypoint = transform.Find("Target Waypoint").gameObject.transform;
    }

    void UpdateCurrentUserState()
    {
        currPos = Utilities.FlattenedPos3D(headTransform.position);
        currPosReal = Utilities.GetRelativePosition(currPos, this.transform);
        currDir = Utilities.FlattenedDir3D(headTransform.forward);
        currDirReal = Utilities.FlattenedDir3D(Utilities.GetRelativeDirection(currDir, this.transform));
    }

    void UpdatePreviousUserState()
    {
        prevPos = Utilities.FlattenedPos3D(headTransform.position);
        prevPosReal = Utilities.GetRelativePosition(prevPos, this.transform);
        prevDir = Utilities.FlattenedDir3D(headTransform.forward);
        prevDirReal = Utilities.FlattenedDir3D(Utilities.GetRelativeDirection(prevDir, this.transform));
    }

    void CalculateStateChanges()
    {
        deltaPos = currPos - prevPos;
        deltaDir = Utilities.GetSignedAngle(prevDir, currDir);
    }

    public void OnResetTrigger()
    {
        if (inReset)
            return;
        //print("NOT IN RESET");
        //print("Is Resetter Null? " + (resetter == null));
        //Debug.Log("resetter.IsResetRequired(): "+resetter.IsResetRequired());
        if (resetter != null && resetter.IsResetRequired() && !tilingMode)
        {
            //print("RESET WAS REQUIRED");
            resetter.InitializeReset();
            inReset = true;

        }
        else if(tilingMode) // 조개줍기 등의 시나리오 필요, 사운드 필요, Drawer 보이기 안보이기 필요
        {
            resetter.InitializeReset();
            inReset = true;
        }
    }

    public void OnResetEnd()
    {
        resetter.FinalizeReset();
        inReset = false;
    }

    public void RemoveResetter()
    {
        this.resetter = this.gameObject.GetComponent<Resetter>();
        if (this.resetter != null)
            Destroy(resetter);
        resetter = null;
    }

    public void UpdateResetter(System.Type resetterType)
    {
        RemoveResetter();
        if (resetterType != null)
        {
            this.resetter = (Resetter) this.gameObject.AddComponent(resetterType);
            //this.resetter = this.gameObject.GetComponent<Resetter>();
            SetReferenceForResetter();
            if (this.resetter != null)
                this.resetter.Initialize();
        }
    }

    public void UpdateTrackedSpaceDimensions(float x, float z)
    {
        trackedSpace.localScale = new Vector3(x, 1, z);
        resetTrigger.Initialize();
        if (this.resetter != null)
            this.resetter.Initialize();
    }

    public void setControllerTriggered()
    {
        this.controllerTriggered = true;
    }
}

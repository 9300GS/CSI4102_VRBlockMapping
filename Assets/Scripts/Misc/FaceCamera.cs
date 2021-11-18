using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [HideInInspector] private Transform vrCamera;

    void Start()
    {
        vrCamera = GameObject.Find("VRCamera").transform;
    }

    void Update()
    {
        transform.LookAt(new Vector3(vrCamera.position.x, transform.position.y, vrCamera.position.z), Vector3.up);
    }
}

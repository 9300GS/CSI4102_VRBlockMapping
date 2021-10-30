using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [HideInInspector] private Transform camera;

    void Start()
    {
        camera = GameObject.Find("VRCamera").transform;
    }

    void Update()
    {
        transform.LookAt(new Vector3(camera.position.x, transform.position.y, camera.position.z), Vector3.up);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Door"))
        {
            Animator m_Animator = other.gameObject.GetComponent<Animator>();
            m_Animator.Play("Opening");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Door"))
        {
            Animator m_Animator = other.gameObject.GetComponent<Animator>();
            m_Animator.Play("Closing");
        }
    }
}

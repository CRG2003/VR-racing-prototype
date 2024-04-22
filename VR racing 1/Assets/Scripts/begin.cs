using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class begin : MonoBehaviour
{
    public bool started = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            started = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            started = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    public bool passed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Car")) {
            passed = true;
        }
    }
}

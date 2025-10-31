using System.Collections.Generic;
using UnityEngine;

public class GrabController : MonoBehaviour
{
    // public GameObject touchedObj;

    // public List<GameObject> touchedObjects = new List<GameObject>();
    public GameObject touchedObject;


    void OnTriggerEnter(Collider other)
    {
        touchedObject = other.gameObject;
        // touchedObjects.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        touchedObject = null;
        // touchedObjects.Remove(other.gameObject);
    }
}

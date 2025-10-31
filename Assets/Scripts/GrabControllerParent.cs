using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabControllerParent : MonoBehaviour
{
    public GameObject left;
    public GameObject right;
    public GameObject grabbedObj;
    public bool Grabbed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (left == null || right == null)
        {
            Debug.Log("One side is missing");
        }
        Grabbed = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Left side object is: " + left.GetComponent<GrabController>().touchedObject);
        Debug.Log("Right side object is: " + right.GetComponent<GrabController>().touchedObject);

        // if (Grabbed) return;

        if (grabbedObj == null)
        {
            var leftController = left.GetComponent<GrabController>();
            var rightController = right.GetComponent<GrabController>();

            // Debug.Log("Checking if both tools are touching something");
            if (leftController.touchedObject != null && rightController.touchedObject != null)
            {
                // Debug.Log("Checking if both sides are touching the same thing");
                if (leftController.touchedObject == rightController.touchedObject)
                {
                    // Debug.Log("Both are touching the same thing, Grab");
                    grabbedObj = leftController.touchedObject;
                    Grab();
                }
            }

        }
        else
        {
            var leftController = left.GetComponent<GrabController>();
            var rightController = right.GetComponent<GrabController>();

            if (leftController.touchedObject != null && rightController.touchedObject != null)
            {
                // Debug.Log("Checking if both sides are NOT touching the same thing");
                if (leftController.touchedObject != rightController.touchedObject)
                {
                    Debug.Log("Release");
                    Release();
                    grabbedObj = null;
                }
            }
        }
        
        if (Grabbed)
        {
            if (left.GetComponent<GrabController>().touchedObject != right.GetComponent<GrabController>().touchedObject)
            {
                left.GetComponent<GrabController>().touchedObject = grabbedObj;
                right.GetComponent<GrabController>().touchedObject = grabbedObj;
            }
        }
    }

    void Grab()
    {
        Grabbed = true;
        grabbedObj.transform.SetParent(left.transform);

        left.GetComponent<GrabController>().touchedObject = grabbedObj;
        right.GetComponent<GrabController>().touchedObject = grabbedObj;

        // var col = grabbedObj.GetComponent<Collider>();
        // if (col != null)
        // {
        //     Physics.IgnoreCollision(left.GetComponent<Collider>(), col, true);
        //     Physics.IgnoreCollision(right.GetComponent<Collider>(), col, true);
        // }

    }

    void Release()
    {
        // var col = grabbedObj.GetComponent<Collider>();
        // if (col != null)
        // {
        //     Physics.IgnoreCollision(left.GetComponent<Collider>(), col, false);
        //     Physics.IgnoreCollision(right.GetComponent<Collider>(), col, false);
        // }

        grabbedObj.transform.SetParent(null);
        Grabbed = false;
    }
}

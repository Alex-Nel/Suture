using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabComponent : MonoBehaviour
{
    public List<GameObject> touchingObjects = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        touchingObjects = touchingObjects.Distinct<GameObject>().ToList<GameObject>();

        if (touchingObjects.Count >= 2)
        {
            transform.SetParent(touchingObjects[0].transform, true);
        }
        else
        {
            transform.SetParent(null);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Tool" || other.gameObject.tag == "OtherGrabber")
        {
            Debug.Log("Collided with: " + other.gameObject);
            touchingObjects.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Tool" || other.gameObject.tag == "OtherGrabber")
        {
            Debug.Log("Object exited: " + other.gameObject);
            touchingObjects.Remove(other.gameObject);
            // touchingObjects.RemoveAll(other.gameObject);
        }
    }



    //
    // Currrently Unused
    //
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject);
        // if (collision.gameObject.tag == "HandMarker")
        //     touchingObjects.Add(collision.gameObject);

        touchingObjects.Add(collision.gameObject);
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("Object exited: " + collision.gameObject);
        // if (collision.gameObject.tag == "HandMarker")
        //     touchingObjects.Remove(collision.gameObject);

        touchingObjects.Remove(collision.gameObject);
    }

}

using UnityEngine;

public class RopeGrabber : MonoBehaviour
{
    private Transform grabbedRopeEnd = null;
    private Rigidbody grabbedRigidbody = null;

    void OnTriggerEnter(Collider other)
    {
        if (grabbedRopeEnd == null && other.CompareTag("RopeEnd"))
        {
            GrabRope(other.transform);
        }
    }

    void Update()
    {
        // Example release: press space key or define your own trigger
        if (grabbedRopeEnd != null && Input.GetKeyDown(KeyCode.Space))
        {
            ReleaseRope();
        }
    }

    void GrabRope(Transform ropeEnd)
    {
        grabbedRopeEnd = ropeEnd;
        grabbedRigidbody = ropeEnd.GetComponent<Rigidbody>();

        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.isKinematic = true; // Disable physics while held
        }

        ropeEnd.SetParent(transform); // Attach to hand
        Debug.Log("Rope grabbed: " + ropeEnd.name);
    }

    void ReleaseRope()
    {
        if (grabbedRopeEnd == null) return;

        grabbedRopeEnd.SetParent(null); // Detach from hand

        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.isKinematic = false; // Re-enable physics
        }

        Debug.Log("Rope released: " + grabbedRopeEnd.name);

        // Clear references
        grabbedRopeEnd = null;
        grabbedRigidbody = null;
    }
}

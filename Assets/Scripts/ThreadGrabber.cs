using UnityEngine;

public class ThreadGrabber : MonoBehaviour
{

    public Rigidbody grabbedThreadBody = null;
    public Transform grabbedThreadPoint = null;

    public bool ThreadIsGrabbed = false;

    public Vector3 GrabLocation;

    public bool TriggerActive = false;
    public float TriggerTimer = 0f;

    void Start()
    {
        grabbedThreadBody = null;
        grabbedThreadPoint = null;

        GrabLocation = new Vector3(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && TriggerActive == true)
        {
            ReleaseThread();
            TriggerActive = false;
            gameObject.GetComponent<Collider>().isTrigger = false;
            TriggerTimer = 0f;
        }
        

        if (ThreadIsGrabbed == true)
        {
            grabbedThreadPoint.transform.position = transform.position;
        }


        if (TriggerActive == false)
        {
            TriggerTimer += Time.deltaTime;

            if (TriggerTimer >= 1f)
            {
                gameObject.GetComponent<Collider>().isTrigger = true;
                TriggerActive = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (grabbedThreadPoint == null && other.CompareTag("Thread"))
            GrabThread(other.transform);
    }

    void GrabThread(Transform threadPoint)
    {
        grabbedThreadPoint = threadPoint;
        grabbedThreadBody = grabbedThreadPoint.GetComponent<Rigidbody>();

        if (grabbedThreadBody != null)
            grabbedThreadBody.isKinematic = true;

        threadPoint.SetParent(transform);
        ThreadIsGrabbed = true;

        Debug.Log("Thread grabbed " + threadPoint.name);
    }

    void ReleaseThread()
    {
        if (ThreadIsGrabbed == false || grabbedThreadPoint == null)
            return;

        grabbedThreadPoint.SetParent(null);

        if (grabbedThreadBody != null)
            grabbedThreadBody.isKinematic = false;

        ThreadIsGrabbed = false;

        Debug.Log("Thread grabbed " + grabbedThreadPoint.name);

        grabbedThreadPoint = null;
        grabbedThreadBody = null;
    }
}

using UnityEngine;

public class AttachThread : MonoBehaviour
{

    public GameObject threadPoint;

    public GameObject needlePoint;

    public Vector3 originalPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (threadPoint == null || needlePoint == null)
            Debug.Log("One of two transforms aren't set");

        originalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = originalPosition;

        Vector3 needleWorldPoint = needlePoint.transform.position;

        threadPoint.transform.position = needleWorldPoint;
    }
}

using UnityEngine;

public class messanger : MonoBehaviour
{

    public Collider col;

    public bool EnteredMesh;

    public bool ExitedMesh;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        col = GetComponent<Collider>();
        EnteredMesh = false;
        ExitedMesh = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        EnteredMesh = true;
        // ExitedMesh = false;
    }

    void OnTriggerExit(Collider other)
    {
        ExitedMesh = true;
        // EnteredMesh = false;
    }
}

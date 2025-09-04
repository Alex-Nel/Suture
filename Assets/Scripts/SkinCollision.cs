using UnityEngine;

public class SkinCollision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected");
        Debug.Log("Collision was at position " + collision.transform.position.x + collision.transform.position.y + collision.transform.position.z);

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.transform.position = collision.transform.position;
        obj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

//     private void OnTriggerEnter(Collider collision)
//     {
//         Debug.Log("Trigger collision detected");
//         Debug.Log("Collision was at position " + collision.transform.position.x + collision.transform.position.y + collision.transform.position.z);

//         GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         obj.transform.position = collision.transform.position;
//         obj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

//     }
}

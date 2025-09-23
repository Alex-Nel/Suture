using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class SutureTipController : MonoBehaviour
{
    [Header("Suturing Settings")]
    public string deformableTag = "Deformable";
    public Material lineMaterial;
    public float autoResetTime = 5f; // seconds to reset if no second point is selected

    private Vector3? firstHit = null;
    private Vector3? secondHit = null;
    private float sutureTimer = 0f;

    private void Start()
    {
        // Set up tip collider
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.01f;

        // Set up physics
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Optional: make tip visible
        // GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // sphere.transform.SetParent(transform);
        // sphere.transform.localPosition = Vector3.zero;
        // sphere.transform.localScale = Vector3.one * 0.01f;
        // Renderer rend = sphere.GetComponent<Renderer>();
        // rend.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Unlit/Color")) { color = Color.red };
        // Destroy(sphere.GetComponent<Collider>());
    }

    private void Update()
    {
        // Reset timer if only one point is selected
        if (firstHit != null && secondHit == null)
        {
            sutureTimer += Time.deltaTime;
            if (sutureTimer >= autoResetTime)
            {
                Debug.Log("⏱️ Suture timeout. Resetting...");
                ResetSuture();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger hit, checking tag");
        if (!other.CompareTag(deformableTag)) return;

        Vector3 hitPos = transform.position;

        if (firstHit == null)
        {
            firstHit = hitPos;
            sutureTimer = 0f;
            Debug.Log("✅ First suture point set.");
        }
        else if (secondHit == null)
        {
            secondHit = hitPos;
            Debug.Log("✅ Second suture point set.");

            // Create suture line
            LineRenderer lr = new GameObject("SutureLine").AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startWidth = lr.endWidth = 0.002f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.SetPosition(0, firstHit.Value);
            lr.SetPosition(1, secondHit.Value);

            // Trigger deformation on mesh
            SuturingMeshDeformerOptimized deformer = other.GetComponent<SuturingMeshDeformerOptimized>();
            // SuturingMeshDeformer deformer = other.GetComponent<SuturingMeshDeformer>();
            if (deformer != null)
            {
                deformer.ApplySuture(firstHit.Value, secondHit.Value);
            }

            ResetSuture();
        }
    }

    private void ResetSuture()
    {
        firstHit = null;
        secondHit = null;
        sutureTimer = 0f;
    }
}

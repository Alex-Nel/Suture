using System.IO;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SaveMeshAsTxt : MonoBehaviour
{
    public string fileName = "ModifiedTooth.txt";  // Change to .txt for easier access
    public AudioSource contactSound;               // Optional: assign in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ARTarget"))
        {
            Debug.Log("[SaveMeshAsTxt] Touch detected. Saving OBJ as TXT...");

            if (contactSound != null)
                contactSound.Play();

            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.mesh == null)
            {
                Debug.LogError("[SaveMeshAsTxt] No MeshFilter or Mesh found.");
                return;
            }

            // Save to Application.persistentDataPath
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            string objData = MeshToOBJ(mf.mesh);
            File.WriteAllText(filePath, objData);

            Debug.Log($"[SaveMeshAsTxt] Mesh saved as .txt at:\n{filePath}");
        }
    }

    private string MeshToOBJ(Mesh mesh)
    {
        StringBuilder sb = new StringBuilder();

        foreach (Vector3 v in mesh.vertices)
            sb.AppendLine($"v {v.x} {v.y} {v.z}");

        foreach (Vector3 n in mesh.normals)
            sb.AppendLine($"vn {n.x} {n.y} {n.z}");

        foreach (Vector2 uv in mesh.uv)
            sb.AppendLine($"vt {uv.x} {uv.y}");

        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int a = triangles[i] + 1;
            int b = triangles[i + 1] + 1;
            int c = triangles[i + 2] + 1;
            sb.AppendLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");
        }

        return sb.ToString();
    }
}

using UnityEngine;

[ExecuteInEditMode]
public class CameraAutoAlignment : MonoBehaviour
{
    [SerializeField] private Renderer groundMesh;
    private Camera cam;

    private void LateUpdate()
    {
        if (groundMesh == null) return;
        if (cam == null) cam = GetComponent<Camera>();

        Vector3 center = groundMesh.bounds.center;
        transform.position = new Vector3(center.x, transform.position.y, center.z);

        cam.orthographicSize = groundMesh.bounds.extents.z;

    }
}

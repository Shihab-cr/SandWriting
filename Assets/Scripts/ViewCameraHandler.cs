using UnityEditor.Rendering;
using UnityEngine;

public class ViewCameraHandler : MonoBehaviour
{
    [SerializeField] private float cameraY=30;
    private Camera cam;
    void Awake()
    {
        cam = GetComponent<Camera>();
        cameraY = transform.position.y;
    }
    public void MoveCameraFurther(float dist)
    {
        float fieldOfView = cam.fieldOfView;
        float rad = Mathf.Deg2Rad * fieldOfView / 2;
        cameraY = dist / Mathf.Tan(rad);
        transform.position = new Vector3(transform.position.x, cameraY, transform.position.z);
    }
    public void ReAlignCamera(Vector2 coords)
    {
        cam.transform.position=new Vector3(coords.x, cam.transform.position.y, coords.y);
    }
    
}

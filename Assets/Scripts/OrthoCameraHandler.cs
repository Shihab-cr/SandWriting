using UnityEngine;

public class OrthoCameraHandler : MonoBehaviour
{
    private Camera cam;
    void Awake()
    {
        cam = GetComponent<Camera>();
    }
    public void IncreaseSize(float planeWorldUnits)
    {
        cam.orthographicSize = planeWorldUnits / 2;
    }
    public void ReAlignCamera(Vector2 coords)
    {
        cam.transform.position = new Vector3(coords.x, cam.transform.position.y, coords.y);
    }
}

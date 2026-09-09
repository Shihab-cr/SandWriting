using UnityEngine;
using UnityEngine.InputSystem;

public class BrushControl : MonoBehaviour
{
    private Camera mainCamera;
    private Collider brushCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        brushCollider = GetComponent<Collider>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            MoveBrush();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            TurnOffBrushEffect();
        }
        else if (Input.GetMouseButton(0))
        {
            MoveBrush();
        }


        /*
        if(Mouse.current.leftButton.isPressed)
        {
            MoveBrush();
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            MoveBrush();

        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            //TurnOffBrushEffect();
        }*/
    }

    void MoveBrush()
    {
        if(mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Or MainCamera tag is missing.");
            return;
        }
        TurnOnBrushEffect();
        
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.Log("Mouse Position: " + Mouse.current.position.ReadValue());
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
        if(Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Ground"))){
            transform.position = hit.point;
        }


    }

    void TurnOnBrushEffect()
    {
        if(brushCollider != null)
        {
            brushCollider.enabled = true;
        }
    }
    void TurnOffBrushEffect()
    {
        if (brushCollider != null)
        {
            brushCollider.enabled = false;
        }
    }
}

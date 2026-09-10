using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BrushControl : MonoBehaviour
{
    private Camera mainCamera;
    private Collider brushCollider;
    [SerializeField] private ParticleSystem brushTrail;
    [SerializeField] private ParticleSystem dustTrail;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        brushCollider = GetComponent<Collider>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                MoveBrush();
                simpleClick();

            }
            else if (Input.GetMouseButtonUp(0))
            {
                TurnOffBrushEffect();
            }
            else if (Input.GetMouseButton(0))
            {
                MoveBrush();
            }
        }
        
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
       // Debug.Log("Mouse Position: " + Mouse.current.position.ReadValue());
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
        if(Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Ground"))){
            transform.position = hit.point;
            //Debug.Log("Brush and ray difference: " + (hit.point - transform.position));
        }

        
    }

    void TurnOnBrushEffect()
    {
        if(brushCollider != null)
        {
            brushCollider.enabled = true;
        }
        if(brushTrail != null) {
            if(!brushTrail.isPlaying)
            {
                brushTrail.Play();
            }
        }

        
    }
    void TurnOffBrushEffect()
    {
        if (brushCollider != null)
        {
            brushCollider.enabled = false;
        }
        if(brushTrail != null)
        {
            brushTrail.Pause();
        }
    }
    void simpleClick()
    {
        if(brushTrail != null)
        {
            StartCoroutine(clickSequence());
        }
        if(dustTrail != null)
        {
            StartCoroutine(dustTrailSequence());
        }
    }



    private IEnumerator clickSequence()
    {
        var emission = brushTrail.emission;
        emission.rateOverTime = 15f;
        emission.rateOverDistance = 0;
        yield return new WaitForSeconds(0.1f);
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 5f;
    }

    private IEnumerator dustTrailSequence()
    {
        var emission = dustTrail.emission;
        
        emission.rateOverTime = 35f;
        emission.rateOverDistance = 0;
        yield return new WaitForSeconds(0.1f);
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 15f;
    }
   
}

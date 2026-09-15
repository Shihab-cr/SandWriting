using UnityEngine;

public class HandleTexture : MonoBehaviour
{
    Renderer planeRenderer;
    void Start()
    {
        planeRenderer = GetComponent<Renderer>();
        SetTextureColorToWhite();
    }
    
    private void SetTextureColorToWhite()
    {
        /*if (planeRenderer != null) {
            RenderTexture currRT = RenderTexture.active;
            RenderTexture rt = (RenderTexture)planeRenderer.material.GetTexture("_BaseMap");
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.white);
            RenderTexture.active = currRT;
        }*/
    }
}

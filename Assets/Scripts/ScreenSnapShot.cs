using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class ScreenSnapShot : MonoBehaviour
{
    [SerializeField] private RenderTexture rt;
    private Texture2D snapShot;
    [SerializeField] private GridConversionLogic gridLogic;

    [SerializeField] private ParticleSystem brushTrail;
    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
       
        //TakeSnapShot();
        
    }

    public void TakeSnapShot()
    {
        if (brushTrail != null) { 
            if (brushTrail.particleCount <= 0)
            {
                return;
            }
        }


        RenderTexture currRenderTex = RenderTexture.active;
        RenderTexture.active = rt;

        //if(snapShot == null)
        //{
            snapShot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        //}
        snapShot.ReadPixels(new Rect(0, 0, snapShot.width, snapShot.height), 0, 0);
        snapShot.Apply();
        RenderTexture.active = currRenderTex;

        //viewPlaneRenderer.material.SetTexture("_BaseMap", snapShot);
        if (gridLogic != null) gridLogic.ApplyTexture(snapShot);
    }
}

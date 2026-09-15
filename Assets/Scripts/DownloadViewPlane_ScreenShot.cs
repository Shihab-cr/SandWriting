using System.Collections;
using UnityEngine;

public class DownloadViewPlane_ScreenShot : MonoBehaviour
{

    [SerializeField] GameObject ViewPlane_Canvas;
    [SerializeField] Camera ViewCamera;
    [SerializeField] RenderTexture outputRes;

    void Start()
    {
        ViewCamera.gameObject.SetActive(false);
    }
    public void DownloadScreenShot()
    {
        Debug.Log("ScreenShot Saved!");
        if (ViewPlane_Canvas != null) ViewPlane_Canvas.gameObject.SetActive(false);

        if (outputRes != null)
        {
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = outputRes;

            ViewCamera.Render();

            int width = outputRes.width;
            int height = outputRes.height;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Rect rect = new Rect(0, 0, width, height);
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();
            RenderTexture.active = currentRT;

            byte[] byteStream = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/CameraScreenShot.png", byteStream);


        }

        if (ViewPlane_Canvas != null) ViewPlane_Canvas.gameObject.SetActive(true);
    }
}

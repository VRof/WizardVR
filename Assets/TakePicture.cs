using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class TakePicture : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SavePicture(string path,Camera cam, GameObject tip) {
        cam.transform.LookAt(tip.transform);

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D Image = new Texture2D(128, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var bytes = Image.EncodeToJPG();
        Destroy(Image);

        //string filePath = Application.persistentDataPath + "/SavedRenderTexture.jpg";

        // Write to file
        File.WriteAllBytes(path, bytes);
        Debug.Log("RenderTexture saved as JPG to: " + path);

    }
}

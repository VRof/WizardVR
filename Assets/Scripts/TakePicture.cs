using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem.Android;


public class TakePicture : MonoBehaviour
{
    static int counter = 0;
    static byte[] lastPictureSaved; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SavePicture(Camera cam, GameObject tip) {
        cam.transform.LookAt(tip.transform);

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        byte[] bytes = Image.EncodeToJPG();
        lastPictureSaved = bytes;
        Destroy(Image);

        // Write to file
        string filePath = Path.Combine(Application.persistentDataPath, "SavedRenderTexture" + counter + ".jpg");
        counter++;
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("RenderTexture saved as JPG to: " + filePath);
    }
    public static byte[] GetLastPictureAsData() {
        return lastPictureSaved;
    }
}

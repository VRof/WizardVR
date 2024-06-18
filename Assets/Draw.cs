using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class Draw : MonoBehaviour
{
    [Header("Pen Properties")]
    [SerializeField] private GameObject tip;
    [SerializeField] private Material drawingMaterial;
    [SerializeField] private Material whiteMaterial;

    [Range(0.01f, 0.1f)]
    public float penWidth = 0.03f;
    [SerializeField][Range(0.01f, 0.5f)] private float DrawOffset = 0.1f;
    [SerializeField] private Camera camera;
    private TakePicture pictureSaver = new TakePicture();
    private LineRenderer currentDrawing;


    // Update is called once per frame
    void Update()
    {
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out bool rightJoystickButtonPressed) && rightJoystickButtonPressed)
        {
                draw();
        }
        else if (!rightJoystickButtonPressed)
        {
                if (currentDrawing != null)
                {
                currentDrawing.material = whiteMaterial;
                //currentDrawingList.Add(currentDrawing.gameObject);
                pictureSaver.SavePicture("C:\\Users\\madrat\\Desktop",camera, tip.gameObject);
                Destroy(currentDrawing.gameObject);
                currentDrawing = null;
                }

        }
        //if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.gripButton, out bool botton) && botton)
        //{
        //        if (currentDrawingList.Count != 0)
        //        {
        //            GameObject last = currentDrawingList[currentDrawingList.Count - 1];
        //            cam.transform.LookAt(last.transform);
                    
        //            foreach (GameObject obj in currentDrawingList)
        //            {
        //                Debug.Log(obj.transform.position);
        //                GameObject.Destroy(obj); // Destroy each object in the list
        //            }

        //           currentDrawingList.Clear(); // Clear the list after destroying all objects
        //        }
        //}
    }

    void draw()
    {

        if (currentDrawing == null)
        {
            currentDrawing = new GameObject().AddComponent<LineRenderer>();
            currentDrawing.material = drawingMaterial;
            currentDrawing.startColor = currentDrawing.endColor = Color.black;
            currentDrawing.startWidth = currentDrawing.endWidth = penWidth;
            currentDrawing.positionCount = 1;
            currentDrawing.numCornerVertices = 20;
            currentDrawing.numCapVertices = 20;
            currentDrawing.SetPosition(0, tip.transform.position);
            currentDrawing.gameObject.layer = LayerMask.NameToLayer("Projection"); // Change "YourLayerName" to the desired layer name
        }
        else
        {
            var currentPos = currentDrawing.GetPosition(currentDrawing.positionCount - 1);
            if (Vector3.Distance(currentPos, tip.transform.position) > DrawOffset)
            {
                currentDrawing.positionCount++;
                currentDrawing.SetPosition(currentDrawing.positionCount - 1, tip.transform.position);
            }
        }
    }
}

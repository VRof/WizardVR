using System;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Draw : MonoBehaviour
{
    [Header("Pen Properties")]
    [SerializeField] private GameObject tip;
    [SerializeField] private Material drawingMaterial;
    [SerializeField] private Material whiteMaterial;

    [Range(0.01f, 0.1f)]
    public float penWidth = 0.03f;
    [SerializeField][Range(0.01f, 0.5f)] private float DrawOffset = 0.1f;
    [SerializeField] private Camera cam;
    private LineRenderer currentDrawing = null;

    private void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).IsPressed(InputHelpers.Button.TriggerButton, out bool rightJoystickButtonPressed);
        if (rightJoystickButtonPressed)
        {
           draw();
        }
        else if (!rightJoystickButtonPressed)
        {
            if (currentDrawing != null)
            {
                currentDrawing.material = whiteMaterial;
                //currentDrawingList.Add(currentDrawing.gameObject);
                TakePicture.SavePicture(cam, tip.gameObject);
                pythonConnector.SetDataToSend("Predict");
                Destroy(currentDrawing.gameObject);
                currentDrawing = null;
            }
        }
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

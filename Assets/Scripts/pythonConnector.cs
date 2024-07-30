using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using Unity.VisualScripting;
using System.Collections;
using TMPro;


public class pythonConnector : MonoBehaviour
{
    // Start is called before the first frame update
    Thread mThread;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    string receivedMsg;
    bool rightJoystickButtonPressed;
    bool running;
    static byte[] myWriteBuffer;
    Process pythonProcess;
    public static string modelName;
    [SerializeField] int fadeTime;
    [SerializeField] TMPro.TMP_Text startLabel;
    public delegate void DataReceivedEventHandler(string data);
    public static event DataReceivedEventHandler OnDataReceived;

    private void Start()
    {
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
        CreatePythonProcess();

        StartCoroutine(PauseForSeconds(fadeTime));
    }
    private IEnumerator PauseForSeconds(float seconds)
    {
        Time.timeScale = 0;
        UnityEngine.Debug.Log("Time paused");

        yield return new WaitForSecondsRealtime(seconds);

        Time.timeScale = 1;
        UnityEngine.Debug.Log("Time resumed");

        if (startLabel != null)
            startLabel.enabled = false;
       // SetDataToSend(Encoding.UTF8.GetBytes(modelName));
        UnityEngine.Debug.Log(modelName + "sent");
    }
    private void CreatePythonProcess() {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "python";
        startInfo.Arguments = "\"" + Path.Combine(Application.dataPath, "Scripts", "UnityPython", "UnityPython.py") + "\"";
        startInfo.UseShellExecute = true;
        startInfo.CreateNoWindow = true;
        pythonProcess = Process.Start(startInfo);
        UnityEngine.Debug.Log("python called");
    }

    private void OnApplicationQuit()
    {
        pythonProcess.Kill();
    }

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIP);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while (running)
        {
            if (myWriteBuffer != null)
            { //if there is data to send
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python

                int bytesRead = 0;
                try
                {
                    bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
                }
                catch { }
                if (bytesRead > 0)
                {
                    string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string

                    if (dataReceived != null)
                    {
                        if(dataReceived != "Model loaded" || dataReceived != "Model Updated")
                        {
                            OnDataReceived?.Invoke(dataReceived);
                        }
                        else
                        {
                            UnityEngine.Debug.Log(dataReceived);
                        }
                    }
                }
                myWriteBuffer = null;
            }
        }
        listener.Stop();
    }

    public static void SetDataToSend(byte[] image) {
        myWriteBuffer = image; 
    }
}


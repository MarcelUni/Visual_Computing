using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client; 
    public int port = 5052;
    public bool startRecieving = true;
    public bool printToConsole = false;
    public string data;

    public InputManager im;


    public void Start()
    {

        receiveThread = new Thread(
            new ThreadStart(ReceiveData));

        receiveThread.IsBackground = true;
        receiveThread.Start();
    }


    // receive thread
    private void ReceiveData()
    {
        client = new UdpClient(port);

        while (startRecieving)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                data = Encoding.UTF8.GetString(dataByte);

                if (printToConsole) { print(data); }
            }
            catch (Exception)
            {
                //print(err.ToString());
            }
        }
    }

    private void Update()
    {
        SendData();
    }

    private void SendData()
    {
        switch (data)
        {
            case "Forward":
                im.MoveForward();
                break;
            case "Backward":
                im.MoveBackward();
                break;
            case "ForwardSneak":
                im.ForwardSneak();
                break;
            case "BackwardSneak":
                im.BackwardSneak();
                break;
            case "Interact":
                im.Interact();
                break;
            case "Stop":
                im.NoInput();
                break;

            default:
                im.NoInput();
                break;
        }
    }

}

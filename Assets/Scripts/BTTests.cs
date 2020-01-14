using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Management;
using Microsoft.Win32;
using System.Threading;

public class BTTests : MonoBehaviour
{
   static SerialPort outgoingPort;
   static SerialPort incomingPort;

   public UnityEngine.UI.Text distLabel;

   static string incomingPortName = "COM5";
   static string outgoingPortName = "COM6";

    Thread readThread = new Thread(ReceiveMessageFromBT);
 
    char[] strm = new char[15];
   
    void Start(){
       SendMessageOverBT("it works to send messages over BT");

    }

    void SendMessageOverBT(string message)
    {   
        if(outgoingPort == null)
        {
            outgoingPort = new SerialPort(outgoingPortName, 9600, Parity.None, 8, StopBits.One);
            outgoingPort.Open();
            print("outgoing port opened");
        }
        outgoingPort.WriteLine(message);
    }

   public static void ReceiveMessageFromBT()
    {   
        print("checking if the port is open");
        if(incomingPort == null)
        {
            print("opening port");
            incomingPort = new SerialPort(incomingPortName, 9600, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 500

            };
            incomingPort.Open();
            incomingPort.Handshake = Handshake.None;
            print("port opened");
   
        }
        try
            {
            print(incomingPort.IsOpen);
            print(incomingPort.ReadLine());
            incomingPort.BaseStream.Flush();
        }
            catch (TimeoutException) { print("Exception: timeout");}
    }


    void Update() {
        if (Input.GetKeyDown("space")) {
            SendMessageOverBT("this is the seconds message");
        }

        if (Input.GetKeyDown("return"))
            //ReceiveMessageFromBT();
            RecData();
    }

    void RecData()
    {
        if (incomingPort == null)
        {
            print("opening port");
            incomingPort = new SerialPort(incomingPortName, 9600, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 500
            };
            incomingPort.Open();
            incomingPort.Handshake = Handshake.None;
            incomingPort.DtrEnable = true;
            print("port opened");

        }
        if ((incomingPort != null) && (incomingPort.IsOpen))
        {
            byte tmp;
            string data = "";
            string avalues = "";
            tmp = (byte)incomingPort.ReadByte();
            while (tmp != 255)
            {
                data += ((char)tmp);
                tmp = (byte)incomingPort.ReadByte();
                print(tmp);
                distLabel.text = tmp.ToString();
                if ((tmp == '>') && (data.Length > 30))
                {
                    avalues = data;
                    print(data);
                    distLabel.text = data.ToString();
                    //parseValues(avalues);
                    // data = "";
                }
            }
        }
    }
}


using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Management;
using Microsoft.Win32;
public class BT_Classic : MonoBehaviour
{
    readonly string btDeviceName = @"\Device\BthModem"; // this is how bluetooth serial ports are named; 
    public string inboundPort;
    public string outboundPort;


    [Range(1, 15)]
    public int COM = 1;

    [SerializeField]
    string comPort;

    SerialPort sp;
    private string msg;

    char[] strm = new char[100];

    string[] msgSplit;
    string msgAcceleration;

    string testMessage = "0.18, 0.16, -0.98, 0.12, -2.14, -0.55, 1.01,";

    void Start()
    {
        getPortNames();

    }

    void Update()
    {

    } 

    void getPortNames()
    {
        inboundPort = "COM0";
        List<string> keys = null;
        string rootKeyName = null;
        //This is the registry location for Serial ports devices
        using (RegistryKey RootKey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM", false)) 
        {
            if (RootKey != null)
            {
                keys = new List<string>(RootKey.GetValueNames());
                rootKeyName = RootKey.Name;
            }
        }
        if (keys != null && !string.IsNullOrEmpty(rootKeyName))
        {
            foreach (string drv in keys)
            {
                string port = Registry.GetValue(rootKeyName, drv, string.Empty) as string;
                if (port != null && drv.StartsWith(btDeviceName))
                {   
                    //the inbound is (as far as i know) the larger of the 2 COM ports
                    if (Convert.ToInt16(port.Remove(0, 3)) > Convert.ToInt16(inboundPort.Remove(0, 3)))
                        inboundPort = port;
                    //print(inboundPort);
                }
            }
        }
        //then outbound port is one 1 smaller;
        outboundPort = "COM"+ (Convert.ToInt16(inboundPort.Remove(0, 3))-1);
        OpenPort(inboundPort);
    }

    void OpenPort(string port)
    {
        try
        {
            sp = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
            sp.ReadTimeout = 500;
            sp.Open();
            msg = sp.ReadLine();
            Debug.Log(msg);
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.Log(msg);
                //TotalAcceleration(msg);
            }
            else
            {
                Debug.Log("Bluetooth sensor not found");
            }

        }
        catch (Exception ex)
        {
            sp.Close();
            Debug.Log("Could not open " + port + ", system error: " + ex);
        }
    }
    void TotalAcceleration(string m)
    {
        msgAcceleration = m.Substring(6, 20);
        msgSplit = msgAcceleration.Split(',');
        for (int i = 0; i < msgSplit.Length - 1; i++)
        {
            Debug.Log(msgSplit[i]); //each split
        }
    }
    
    //private void OnApplicationQuit()
    //{
    //    sp.Close();
    //}
}

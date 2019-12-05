using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Management;

public class Bluetooth : MonoBehaviour
{

    [Range(1,15)]
    public int COM = 1;

    [SerializeField]
    string comPort;

    SerialPort sp;
    private string msg;
       
    char[] strm = new char[100];

    string[] msgSplit;
    string msgAcceleration;

    void Start()
    {
        comPort = "COM" + COM;
        GetSerialPorts();

        // if (COM >= 10)
        // {
        //     comPort = "\\\\.\\COM" + COM;
        //     sp = new SerialPort(comPort, 9600, Parity.None, 8, StopBits.One);
        // }
        // else
        // {
        //     sp = new SerialPort(comPort, 9600, Parity.None, 8, StopBits.One); //ports higher than COM9 need to follow the syntax "\\\\.\\COM13" instead of simply "COM13".
        // }
        //
        // sp.ReadTimeout = 500;
        // sp.Open();

        //msg = sp.ReadLine();
        //Debug.Log(msg);
    }


    void Update()
    {
           
    } 



    void GetSerialPorts() {

        string[] ports = SerialPort.GetPortNames();

        foreach (string port in ports)
        {
            OpenPort(port);
            Debug.Log(port);
        }

    }

    void OpenPort(string p)
    {
        try
        {
            sp = new SerialPort(p, 9600, Parity.None, 8, StopBits.One);
            sp.ReadTimeout = 500;
            sp.Open();
            msg = sp.ReadLine();
            Debug.Log(msg);
            if(!string.IsNullOrEmpty(msg))
            {
                Debug.Log(msg);
                //TotalAcceleration(msg);
            } else
            {
                Debug.Log("Bluetooth sensor not found");
            }
            
        }
        catch (Exception ex)
        {
            sp.Close();
            Debug.Log("Could not open " + p + ", system error: " + ex);
        }
    }


    void TotalAcceleration(string m)
    {
        msgAcceleration = m.Substring(6, 20);
        msgSplit = msgAcceleration.Split(',');
        for (int i = 0; i < msgSplit.Length - 1; i++)
        {
            Debug.Log(msgSplit[i]); //each split xyz acceleration
        }
    }


    private void OnApplicationQuit()
    {
        sp.Close();
    }
}

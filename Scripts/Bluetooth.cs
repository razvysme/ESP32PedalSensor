using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

public class Bluetooth : MonoBehaviour
{

    SerialPort sp;

    char[] strm = new char[15];

    void Start()
    {
        sp = new SerialPort("COM8", 9600, Parity.None, 8, StopBits.One); //ports higher than COM9 need to follow the syntax "\\\\.\\COM13" instead of simply "COM13".
        sp.Open();
        //sp.Write("2");
    }

    void Update()
    {
        //sp.Write("*");
        sp.Read(strm, 0, 15);

        if (strm[0] == '-')
        {
            for (int i = 0; i < strm.Length; i++)
            {
                Debug.Log(strm[i]);
                if (i == strm.Length - 1)
                {
                    Debug.Log(strm[i] + "\n");
                }
            }
        }
    }
}
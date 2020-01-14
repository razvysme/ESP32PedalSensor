using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArduinoBluetoothAPI;
using System;
using System.Runtime.InteropServices;

public class ScanSceneManager : MonoBehaviour
{

    // Use this for initialization
    BluetoothHelper bluetoothHelper;
    string deviceName;

    public Text text;

    public GameObject sphere;

    string received_message;

    void Start()
    {
        try
        {
            // Debug.Log(getNumber());
            BluetoothHelper.BLE = true;  //use Bluetooth Low Energy Technology
            bluetoothHelper = BluetoothHelper.GetInstance();
            bluetoothHelper.OnConnected += OnConnected;
            bluetoothHelper.OnConnectionFailed += OnConnectionFailed;
            bluetoothHelper.OnDataReceived += OnMessageReceived; //read the data
            bluetoothHelper.OnScanEnded += OnScanEnded;

            bluetoothHelper.setTerminatorBasedStream("\n");
            //bluetoothHelper.setLengthBasedStream();
            //bluetoothHelper.setFixedLengthBasedStream(3);

            // if(bluetoothHelper.isDevicePaired())
            // 	sphere.GetComponent<Renderer>().material.color = Color.blue;
            // else
            // 	sphere.GetComponent<Renderer>().material.color = Color.grey;
            // bluetoothHelper.ScanNearbyDevices();
            if (!bluetoothHelper.ScanNearbyDevices())
            {
                text.text = "cannot start scan";
                sphere.GetComponent<Renderer>().material.color = Color.black;

                // bluetoothHelper.setDeviceAddress("00:21:13:02:16:B1");
                bluetoothHelper.setDeviceName("HC-08");
                bluetoothHelper.Connect();
            }
            else
            {
                text.text = "start scan";
                // sphere.GetComponent<Renderer>().material.color = Color.green;
            }

        }
        catch (BluetoothHelper.BlueToothNotEnabledException ex)
        {
            sphere.GetComponent<Renderer>().material.color = Color.yellow;
            Debug.Log(ex.ToString());
            text.text = ex.Message;
        }
    }

    IEnumerator blinkSphere()
    {
        sphere.GetComponent<Renderer>().material.color = Color.cyan;
        yield return new WaitForSeconds(0.5f);
        sphere.GetComponent<Renderer>().material.color = Color.green;
    }

    //Asynchronous method to receive messages
    void OnMessageReceived()
    {
        StartCoroutine(blinkSphere());
        received_message = bluetoothHelper.Read();
        text.text = received_message;
        Debug.Log(received_message);
    }

    void OnScanEnded(LinkedList<BluetoothDevice> nearbyDevices)
    {
        text.text = "Found " + nearbyDevices.Count + " devices";
        if (nearbyDevices.Count == 0)
            return;


        foreach(BluetoothDevice device in nearbyDevices)
        {

        }
            
        text.text = nearbyDevices.First.Value.DeviceName;
        bluetoothHelper.setDeviceName(nearbyDevices.First.Value.DeviceName);
        // bluetoothHelper.setDeviceAddress("00:21:13:02:16:B1");
        bluetoothHelper.Connect();
        bluetoothHelper.isDevicePaired();
    }

    void Update()
    {
        //Debug.Log(bluetoothHelper.IsBluetoothEnabled());
        if (!bluetoothHelper.IsBluetoothEnabled())
        {
            bluetoothHelper.EnableBluetooth(true);
        }
    }

    void OnConnected()
    {
        sphere.GetComponent<Renderer>().material.color = Color.green;
        try
        {
            bluetoothHelper.StartListening();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

    }

    void OnConnectionFailed()
    {
        sphere.GetComponent<Renderer>().material.color = Color.red;
        Debug.Log("Connection Failed");
    }

    //Call this function to emulate message receiving from bluetooth while debugging on your PC.
    void OnGUI()
    {
        if (bluetoothHelper != null)
            bluetoothHelper.DrawGUI();
        else
            return;

        // if(!bluetoothHelper.isConnected())
        // if(GUI.Button(new Rect(Screen.width/2-Screen.width/10, Screen.height/10, Screen.width/5, Screen.height/10), "Connect"))
        // {
        // 	if(bluetoothHelper.isDeviceFound())
        // 		bluetoothHelper.Connect (); // tries to connect
        // 	else
        // 		sphere.GetComponent<Renderer>().material.color = Color.magenta;
        // }

        if (bluetoothHelper.isConnected())
            if (GUI.Button(new Rect(Screen.width / 2 - Screen.width / 10, Screen.height - 2 * Screen.height / 10, Screen.width / 5, Screen.height / 10), "Disconnect"))
            {
                bluetoothHelper.Disconnect();
                sphere.GetComponent<Renderer>().material.color = Color.blue;
            }

        if (bluetoothHelper.isConnected())
            if (GUI.Button(new Rect(Screen.width / 2 - Screen.width / 10, Screen.height / 10, Screen.width / 5, Screen.height / 10), "Send text"))
            {
                bluetoothHelper.SendData(new byte[] { 0, 1, 2, 3, 4 });
                //bluetoothHelper.SendData("This is a very long long long long text");
            }
    }

    void OnDestroy()
    {
        if (bluetoothHelper != null)
            bluetoothHelper.Disconnect();
    }
}

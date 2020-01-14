using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Management;
using Microsoft.Win32;
using ArduinoBluetoothAPI;
using UnityEngine.UI;
using TMPro;

/* TO DO
* ensure that comports above 10 have the form :  "\\\\.\\COM13"
 
*/
public class BT_Classic : MonoBehaviour
{
    //ESP bluetooth instance
    public string bluetoothName;
    private BluetoothHelper ESP32;
    
    //GUI Elemennts
    public Text errorText;
    public Text statusText;
    public TextMeshProUGUI nameInputField;
    public Button connectButton;
    public Image statusCircle;
    
    // IMU Data
    public Vector3 accelerometer;
    public Vector3 gyroscope;
    public float acceleration;

    //Temporary obsolete
    readonly string btDeviceName = @"\Device\BthModem"; // this is how bluetooth serial ports are named; 
    public string incomingPort;
    public string outgoingPort;
    SerialPort sp;
    private string msg;
    string[] msgSplit;
    string msgAcceleration;
    //string testMessage = "0.18,0.16,-0.98,0.12,-2.14,-0.55,1.01,";

    Color32 orange = new Color32 (255, 171, 0, 255);
    void Start()
    {
        //bluetoothName = "Pedal_Sensor_Sensor_1"; //bluetooth should be turned ON;
        try
        {
            ESP32 = BluetoothHelper.GetInstance(bluetoothName);
            ESP32.OnConnected += OnConnected;
            ESP32.OnConnectionFailed += OnConnectionFailed;
            ESP32.OnDataReceived += OnMessageReceived; //read the data
            ESP32.setTerminatorBasedStream("\n");
            if (ESP32.isDevicePaired()){
                //print("paired");
                statusCircle.GetComponent<Image>().color = orange;
                statusText.text = "Paired";
                ESP32.Connect();
            }
            else{
                print("notPaired");
                statusCircle.GetComponent<Image>().color = Color.red;
                statusText.text = "Disconnected, attempting to reconnect";
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            errorText.text = ex.Message; //BlueToothNotEnabledException   == bluetooth Not turned ON
                                         //BlueToothNotSupportedException == device doesn't support bluetooth
                                         //BlueToothNotReadyException     == the device name you chose is not paired with your android or you are not connected to the bluetooth device;
                                         //								    bluetoothHelper.Connect () returned false;
        }

    }
    void Update()
    {
        if (Input.GetKeyDown("up"))
        {
            SendTimeToSensor();
            ESP32.Connect();
        }

        if (!ESP32.isConnected())
        {
            print("trying to connect");
            ESP32.Connect();
        }

            
    }

    void OnConnected()
    {
        print("Connected");
        try
        {
            statusCircle.GetComponent<Image>().color = Color.green;
            statusText.text = "Connected";
            ESP32.StartListening();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

    }
    void OnMessageReceived()
    {
        //print(ESP32.Read());
        ParseMessage(ESP32.Read());
    }
    void OnConnectionFailed()
    {
        Debug.Log("Connection Failed");
    }

    void getPortNames()
    {
        outgoingPort = "COM0";
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
                    //the outgoing SHOULD BE the larger of the 2 COM ports
                    if (Convert.ToInt16(port.Remove(0, 3)) > Convert.ToInt16(outgoingPort.Remove(0, 3)))
                        outgoingPort = port;
                    //print(incomingPort);
                }
            }
        }
        //then incoming port is one 1 smaller;
        incomingPort = "COM"+ (Convert.ToInt16(outgoingPort.Remove(0, 3))-1);
        //OpenPort(incomingPort);
    }

    void OpenPort(string port)
    {
        if(outgoingPort != "COM0"){
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

    void ParseMessage(string message) {
        message = message.Substring(0, message.Length - 1);
        float[] valuesIMU = new float[7];
        for (int i = 0; i < 7; i++) {
            valuesIMU[i] = float.Parse(message.Substring(0, message.IndexOf(",")));
            message = message.Substring(message.IndexOf(",") + 1);
        }
        accelerometer.x = valuesIMU[0];
        accelerometer.y = valuesIMU[1];
        accelerometer.z = valuesIMU[2];
        gyroscope.x = valuesIMU[3];
        gyroscope.y = valuesIMU[4];
        gyroscope.z = valuesIMU[5];
        acceleration = valuesIMU[6];
    }

    void SendTimeToSensor(){
        string time = System.DateTime.Now.TimeOfDay.ToString();
        print(time.Substring(0,time.IndexOf(".")));
    }

    private void OnApplicationQuit()
    {
        ESP32.Disconnect();
    }

    //private void OnGUI()
    //{
    //    if (!ESP32.isConnected())
    //        if (connectButton)
    //        {
    //            if (ESP32.isDevicePaired())
    //                ESP32.Connect(); // tries to connect
    //            else
    //                statusCircle.GetComponent<Renderer>().material.color = Color.red;
    //        }
    //    if (ESP32.isConnected())
    //        if (GUI.Button(new Rect(Screen.width / 2 - Screen.width / 10, Screen.height - 2 * Screen.height / 10, Screen.width / 5, Screen.height / 10), "Disconnect"))
    //        {
    //            ESP32.Disconnect();
    //            statusCircle.GetComponent<Renderer>().material.color = Color.blue;
    //        }
    //}
}

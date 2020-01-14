using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArduinoBluetoothAPI;
using System;
using System.Text;

public class manager : MonoBehaviour {

	// Use this for initialization
	BluetoothHelper ESP32;
	public string deviceName = "Pedal_Sensor_RazMatz";

    public Vector3 accelerometer;
    public Vector3 gyroscope;
    public float acceleration;

    public Text text;

	public GameObject sphere;

	string received_message;

	void Start () {
		//deviceName = "Pedal_Sensor_RazMatz"; //bluetooth should be turned ON;
		try
		{	
			ESP32 = BluetoothHelper.GetInstance(deviceName);
			ESP32.OnConnected += OnConnected;
			ESP32.OnConnectionFailed += OnConnectionFailed;
			ESP32.OnDataReceived += OnMessageReceived; //read the data
			ESP32.setTerminatorBasedStream("\n"); //delimits received messages based on \n char
			if(ESP32.isDevicePaired())
				sphere.GetComponent<Renderer>().material.color = Color.blue;
			else
				sphere.GetComponent<Renderer>().material.color = Color.grey;
		}
		catch (Exception ex) 
		{
			sphere.GetComponent<Renderer>().material.color = Color.yellow;
			Debug.Log (ex.Message);
			text.text = ex.Message;
			//BlueToothNotEnabledException == bluetooth Not turned ON
			//BlueToothNotSupportedException == device doesn't support bluetooth
			//BlueToothNotReadyException == the device name you chose is not paired with your android or you are not connected to the bluetooth device;
			//								bluetoothHelper.Connect () returned false;
		}
	}

	IEnumerator blinkSphere()
	{
		sphere.GetComponent<Renderer>().material.color = Color.cyan;
		yield return new WaitForSeconds(0.5f);
		sphere.GetComponent<Renderer>().material.color = Color.green;
	}
	
	// Update is called once per frame
	void Update () {
		
		/*
		//Synchronous method to receive messages
		if(bluetoothHelper != null)
		if (bluetoothHelper.Available)
			received_message = bluetoothHelper.Read ();
		*/
	}

	//Asynchronous method to receive messages
	void OnMessageReceived()
	{
        print("new message arrived");
		StartCoroutine(blinkSphere());
        print(ESP32.Read());
        ParseMessage(ESP32.Read());
		// received_message = 
		// Debug.Log(received_message);
	}

	void OnConnected()
	{
		sphere.GetComponent<Renderer>().material.color = Color.green;
		try{
			ESP32.StartListening ();
		}catch(Exception ex){
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
		if(ESP32!=null)
			ESP32.DrawGUI();
		else 
		return;

		if(!ESP32.isConnected())
		if(GUI.Button(new Rect(Screen.width/2-Screen.width/10, Screen.height/10, Screen.width/5, Screen.height/10), "Connect"))
		{
			if(ESP32.isDevicePaired())
				ESP32.Connect (); // tries to connect
			else
				sphere.GetComponent<Renderer>().material.color = Color.magenta;
		}

		if(ESP32.isConnected())
		if(GUI.Button(new Rect(Screen.width/2-Screen.width/10, Screen.height - 2*Screen.height/10, Screen.width/5, Screen.height/10), "Disconnect"))
		{
			ESP32.Disconnect ();
			sphere.GetComponent<Renderer>().material.color = Color.blue;
		}

		if(ESP32.isConnected())
		if(GUI.Button(new Rect(Screen.width/2-Screen.width/10, Screen.height/10, Screen.width/5, Screen.height/10), "Send text"))
		{
			ESP32.SendData("test\n");
            // bluetoothHelper.SendData("This is a very long long long long text");

		}
	}
    void OnDestroy()
	{
		if(ESP32!=null)
		ESP32.Disconnect ();
	}

    void ParseMessage(string message)
    {
        print("parsin");
        float[] valuesIMU = new float[7];
        for (int i = 0; i < 7; i++)
        {
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
}

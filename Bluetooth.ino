#include <MPU9250_asukiaaa.h>
#include <WiFi.h>
#include <WiFiUDP.h>
#include <OSCMessage.h>
#include "BluetoothSerial.h"
//#include <Ethernet.h>

#ifdef _ESP32_HAL_I2C_H_
#define SDA_PIN 19 //default SDA for IMU
#define SCL_PIN 18 //default SCL for IMU
#endif

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

//define the IMU Sensor instance
MPU9250_asukiaaa mySensor(MPU9250_ADDRESS_AD0_HIGH); //version 1.5


//Networking

//WiFiUDP Udp; //create instance of an UDP over WIFI
BluetoothSerial SerialBT; //Object for Bluetooth

void setup() {
  Serial.begin(115200);

  //initalize the IMU I2C connection
  #ifdef _ESP32_HAL_I2C_H_
  // for esp32
  Wire.begin(19, 18); //sda, scl
  #else
  Wire.begin();
  #endif

  //wifi mode selection
  WiFi.mode(WIFI_OFF); //disable wifi. To enable use "WIFI_MODE_STA" as argument

  SerialBT.begin("ESP_Pedal_Sensor"); //Name of the BT Device
  Serial.println("Bluetooth Device is Ready to Pair");

  //Start IMU reading   
  mySensor.setWire(&Wire);  
  mySensor.beginAccel();
  mySensor.beginMag();
}

void loop(){
  if (Serial.available()) {
      SerialBT.write(Serial.read());
    }
    if (SerialBT.available()) {
      Serial.write(SerialBT.read());
    }
    delay(20);
}


/*To Do
 * - Sleep modes (radio off, deep sleep)
 * - Send IMU Data
 * - Read/Write from EPROM
 * - Change name dinamically (WIFi or Bluetooth)
 * - Log on EPROM 
 * - Connect BT to Unity
 */

#include <MPU9250_asukiaaa.h>
#include <WiFi.h>
#include <WiFiUDP.h>
#include <OSCMessage.h>
#include "BluetoothSerial.h"
#include <TimeLib.h>
//#include <Ethernet.h>

#ifdef _ESP32_HAL_I2C_H_
#define SDA_PIN 19 //default SDA for IMU
#define SCL_PIN 18 //default SCL for IMU
#endif

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

//define the IMU Sensor instance
MPU9250_asukiaaa imu(MPU9250_ADDRESS_AD0_HIGH); //version 1.5


//Networking
//WiFiUDP Udp; //create instance of an UDP over WIFI
BluetoothSerial SerialBT; //Object for Bluetooth

//Variables
int prevDisplay = 0; //untility variable for displaying time, will be obsolete
float accel[3]; // store accelerometer data, X Y Z
float gyro[3];  //store gyro data, X Y Z
String message = " "; //string to send over BT


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
  imu.setWire(&Wire);  
  imu.beginAccel();
  imu.beginGyro();
  imu.beginMag();

  //set current time
  setTime(11,25,30,12,11,2019);
}

void loop(){
  if (Serial.available()) {
      SerialBT.write(Serial.read());
    }
    if (SerialBT.available()) {
      Serial.write(SerialBT.read());
    }
    delay(20);

  //update the display only if the time has changed
  if ( now() != prevDisplay){
    prevDisplay = now();
    digitalClockDisplay();
    updateSensor();
  }
}

 // digital clock display of the time
void digitalClockDisplay() {
  //Serial.print(hour());
  message = hour();
  printDigits(minute());
  printDigits(second());
  //Serial.print(" ");
  message += " ";
 //Serial.print(day());
  message += day();
  //Serial.print(" ");
  message += "-";
  //Serial.print(month());
  message += month();
  //Serial.print(" ");
  message += "-";
  //Serial.print(year());
  message += year();
  //Serial.println();
}

 // utility function for digital clock display: prints preceding colon and leading 0
void printDigits(int digits) {
 //Serial.print(":");
 message += ':';
 if (digits < 10){
    //Serial.print('0');
    message +=  '0';
  }
 //Serial.print(digits);
 message+= digits;
}

//update and store accel data
void updateSensor(){
  //Read acceleration values into array  
  imu.accelUpdate();
  accel[0] = imu.accelX();
  accel[1] = imu.accelY();
  accel[2] = imu.accelZ();
  //Read gyro values into array
  imu.gyroUpdate();
  gyro[0] = imu.gyroX();
  gyro[1] = imu.gyroY();
  gyro[2] = imu.gyroZ();

  printSensorData();
}

//utility function to print sensor data
void printSensorData(){
  //  Serial.print("Accel X, Y, Z: ");
   message += ", Accel: ";
  for(int i=0; i<3; ++i){
    // Serial.print(accel[i]);
    // Serial.print(", ");
    message += accel[i];
    message += ", ";
  }
  // Serial.println();
  // Serial.print("Gyro X, Y, Z: ");
  message += "Gyro: ";
  for(int i=0; i<3; ++i){
    // Serial.print(gyro[i]);
    // Serial.print(", ");
    message += gyro[i];
    message += ", ";
  }
  // Serial.println();
  // Serial.print("Acceleration Sqrt: ");
  // Serial.println(imu.accelSqrt());
  message += "Accel Sqrt: ";
  message += imu.accelSqrt();
  Serial.println(message);
  SerialBT.println(message); //send message over BT
}

/*To Do
 * - Sleep modes (radio off, deep sleep)
 * - Connect BT to Unity tomorow
 */

#include <MPU9250_asukiaaa.h>
#include <WiFi.h>
#include <WiFiUDP.h>
#include <OSCMessage.h>
#include "BluetoothSerial.h"
#include <TimeLib.h>
#include <EEPROM.h>
//#include <Ethernet.h>

#ifdef _ESP32_HAL_I2C_H_
#define SDA_PIN 19 //default SDA for IMU
#define SCL_PIN 18 //default SCL for IMU
#endif

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif



//Networking
//WiFiUDP Udp; //create instance of an UDP over WIFI
BluetoothSerial SerialBT; //Object for Bluetooth

//Variables
int prevDisplay = 0; //untility variable for displaying time, will be obsolete
float accel[3]; //stores accelerometer data, X Y Z
float gyro[3];  //stores gyro data, X Y Z
String message = ""; //complete message to send over BT
String name = ""; //Name of the BT device, to be stored/retreived from EEPROM
String serialInput = ""; //store commands received over BT Serial
int ledState = LOW; // ledState used to set the LED
unsigned long previousMillis = 0; // will store last time LED was update

//define the IMU Sensor
MPU9250_asukiaaa imu(MPU9250_ADDRESS_AD0_HIGH); //version 1.5

void setup() {
  Serial.begin(115200);
  EEPROM.begin(512);
  //initalize the IMU I2C connection
  #ifdef _ESP32_HAL_I2C_H_
  // for esp32
  Wire.begin(19, 18); //sda, scl
  #else
  Wire.begin();
  #endif

  //wifi mode selection
  WiFi.mode(WIFI_OFF); //disable wifi. To enable use "WIFI_MODE_STA" as argument
  name = "Pedal_Sensor_" + readFromEEPROM(10);
  SerialBT.begin(name); //Name of the BT Device
  Serial.println("Bluetooth Device is Ready to Pair"); 

  //Start IMU reading   
  imu.setWire(&Wire);  
  imu.beginAccel();
  imu.beginGyro();
  //imu.beginMag(); //magnetometer not in use for now

  //set current time
  setTime(11,25,30,12,11,2019);
}

void loop(){
  if (Serial.available()) {
      SerialBT.write(Serial.read());
    }
    if (SerialBT.available()) {
      serialInput = SerialBT.readStringUntil('\n');
      Serial.println(serialInput);
      configureOverBT();
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
  message = hour();
  printDigits(minute());
  printDigits(second());
  message += " ";
  message += day();
  message += "-";
  message += month();
  message += "-";
  message += year();
}

 // utility function for digital clock display: prints preceding colon and leading 0
void printDigits(int digits) {
 message += ':';
 if (digits < 10){
    message +=  '0';
  }
 message += digits;
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
   message += ", Accel: ";
  for(int i=0; i<3; ++i){
    message += accel[i];
    message += ", ";
  }
  message += "Gyro: ";
  for(int i=0; i<3; ++i){
    message += gyro[i];
    message += ", ";
  }
  message += "Accel Sqrt: ";
  message += imu.accelSqrt();
  //Serial.println(message);
  //SerialBT.println(message); //send message over BT
  //Serial.println(name); 
}

//write a string to EEPROM
void writeToEEPROM(char add, String data){
  int _size = data.length();
  int i;
  for(i=0;i<_size;i++)
  {
    EEPROM.write(add+i,data[i]);
  }
  EEPROM.write(add+_size,'\0');   //Add termination null character for String Data
  EEPROM.commit();
}
//read from EEPROM
String readFromEEPROM(char add){
  int i;
  char data[100]; //Max 100 Bytes
  int len=0;
  unsigned char k;
  k=EEPROM.read(add);
  while(k != '\0' && len<500)   //Read until null character
  {    
    k=EEPROM.read(add+len);
    data[len]=k;
    len++;
  }
  data[len]='\0';
  return String(data);
}

//function that listens and executes commands from SerialBT
void configureOverBT(){
  //change the Bluetooth name
  if (serialInput.startsWith("n")){
    serialInput.remove(0,2);
    name = serialInput;
    SerialBT.println("name has been changed to: " + name);
    writeToEEPROM(10,name);
    SerialBT.println("Sensor will restart in 3 seconds.");
    delay(3000);
    ESP.restart();
  }
  //reboot the sensor
  else if (serialInput.startsWith("r")){
    SerialBT.println("Sensor will restart in 3 seconds.");
    delay(3000);
    ESP.restart();
  }
  //blink 
  else if (serialInput.startsWith("b")){
    blink(1000);
  }
  //output status
   else if (serialInput.startsWith("s")){
    SerialBT.println(readFromEEPROM(10));
  }
  else
  {
    SerialBT.println("Unknown command. Try using n for name, r for reboot ro b for blink");
  }
  
}
//blinking function to test the sensor
void blink(const long interval){
  SerialBT.println("Blinking for 10 seconds");
  unsigned long currentMillis = millis();
  if (currentMillis - previousMillis >= interval) {
    // save the last time you blinked the LED
    previousMillis = currentMillis;
    // if the LED is off turn it on and vice-versa:
    if (ledState == LOW) {
      ledState = HIGH;
    } else {
      ledState = LOW;
    }
    // set the LED with the ledState of the variable:
    digitalWrite(2, ledState);
  }
}

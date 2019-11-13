// -----------------------------------------------------------------------------
// Written by Razvan Paisa - 2019  
// -----------------------------------------------------------------------------

/*To Do
 * - Sleep modes (radio off, deep sleep)
 * - Connect BT to Unity tomorow
 * - BLE //sort of works
 * - Angular Velocity
 */

#include <MPU9250_asukiaaa.h>
#include "BluetoothSerial.h"
#include <EEPROM.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>
//#include <WiFi.h>
//#include <WiFiUDP.h>
//#include <TimeLib.h>
//#include <Ethernet.h>
//#include <OSCMessage.h>

#ifdef _ESP32_HAL_I2C_H_
#define SDA_PIN 19 //default SDA for IMU
#define SCL_PIN 18 //default SCL for IMU
#endif

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

// =============================================================================
// Bluetooth LE initial setup
// =============================================================================
//BLEServer* pServer = NULL; //BLE Server
BLECharacteristic* pCharacteristic; //BLE pCharacteristic
bool clientConnected = false; 

#define SERVICE_UUID           "6E400001-B5A3-F393-E0A9-E50E24DCCA9E"
#define CHARACTERISTIC_UUID_TX "6E400001-B5A3-F393-E0A9-E50E24DCCA9E"

class MyServerCallbacks: public BLEServerCallbacks{
 //todo when a client is connected
 void onConnect(BLEServer* pServer){
    clientConnected = true;
    //Serial.println("Device connected");
 };
 //todo when no client is connected
 void onDisconnect(BLEServer* pServer){
    clientConnected = false;
    //Serial.println("Client disconnected");
 }
};

// =============================================================================
// Networking choices
// =============================================================================
//WiFiUDP Udp; //create instance of an UDP over WIFI
BLEServer* pServer = NULL; //create BLE server instance
BluetoothSerial SerialBT; //Object for Bluetooth


// =============================================================================
// Global Variables
// =============================================================================
String name = ""; //Name of the BT device, to be stored/retreived from EEPROM
String serialInput = ""; //store commands received over BT Serial
int txValue = 0; // temporary to test BLE
int addr = 254;


//define the IMU Sensor
MPU9250_asukiaaa imu(MPU9250_ADDRESS_AD0_HIGH); //version 1.5

// =============================================================================
// Setup function
// =============================================================================
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

// =============================================================================
// Communication methods(BT, BLE)
// =============================================================================
  
  //wifi mode selection
  //WiFi.mode(WIFI_OFF); //disable wifi. To enable use "WIFI_MODE_STA" as argument
  
  name = "Pedal_Sensor_" + readFromEEPROM(10);
  
  if((bool)EEPROM.read(1)==false){
    Serial.println("Creating Bluetooth Classic Server");
    createBTServer();
  }
  else if((bool)EEPROM.read(1)==true){
    Serial.println("Creating BLE server");
    createBLEServer();
  }
  else{
    Serial.println("EEPROM memory unacessible. Cannot create any server");
  }
  
  //Start IMU reading   
  imu.setWire(&Wire);  
  imu.beginAccel();
  imu.beginGyro();
  //imu.beginMag(); magnetometer not in use for now

  //set current time
  //setTime(11,25,30,12,11,2019);
}
// =============================================================================
// Loop function
// =============================================================================
void loop(){
 // ============================================================================
 // Read and write to/from BT Serial
 // ============================================================================
  if (Serial.available()) {
      serialInput = Serial.readStringUntil('\n');
      Serial.println(serialInput);
      configureOverBT();
      SerialBT.write(Serial.read());
    }
    if (SerialBT.available()) {
      Serial.write(SerialBT.read());
    }

    //message = "test"+imu.accelSqrt();
    //message = "Accel: "+imu.accelX()+", " + imu.accelY()+", "+imu.accelZ()+", Accel Sqrt: "+imu.accelSqrt()+", Gyro: "+imu.gyroX()+", "+imu.gyroY() + ", "+ imu.gyroZ();
    //Serial.println(createMessage());
    delay(20);
// =============================================================================
// update characteristics of BLE service
// =============================================================================
  if (clientConnected) {
    char txString[8];
    dtostrf(txValue,1, 2, txString);
    pCharacteristic -> setValue(txString);
    pCharacteristic -> notify();
    Serial.println("Sent value is: " + createMessage());
    //delay(500); //txValue = random(-10, 20);
  }
}
// Update IMU sensors and Create the message to be sent
String createMessage(){
  imu.accelUpdate();
  imu.gyroUpdate();
  String message = "";
  message += "Accel: ";
  message += imu.accelX();
  message += ", ";
  message += imu.accelY();
  message += ", ";
  message += imu.accelZ();
  message += ", Accel Srt: ";
  message += imu.accelSqrt();
  message += ", Gyro: "; 
  message += imu.gyroX();
  message += ", ";
  message += imu.gyroY();
  message += ", ";
  message += imu.gyroZ();
  message += ", ";
  return(message);
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
//commands are: rename, restart, status, mode
void configureOverBT(){
  //change the Bluetooth name
  if (serialInput.startsWith("name ")){
    serialInput.remove(0,5);
    name = serialInput;
    writeToEEPROM(10,name);
    SerialBT.begin(name);
    Serial.println("name has been changed to: " + name);
    // SerialBT.println("Sensor will restart in 3 seconds.");
    // delay(3000);
    // ESP.restart();
  }
  //reboot the sensor
  else if (serialInput.startsWith("restart")){
    Serial.println("Sensor will restart in 1 seconds.");
    delay(1000);
    ESP.restart();
  }
  //output status
  else if (serialInput.startsWith("status")){
    Serial.println(createMessage() + ", BT name is: "+readFromEEPROM(10)+", BLE mode is: "+ (bool)EEPROM.read(1));
  }
  //select mode BLE or BT classic
  else if(serialInput.startsWith("mode")){
    serialInput.remove(0,5);
    if(serialInput.equalsIgnoreCase("BLE")){
      EEPROM.write(1,(byte)true);
      EEPROM.commit();
      Serial.println("BLE enabled, BT Classic disabled");
      //write to epprom
    }
    else if(serialInput.equalsIgnoreCase("BT")){
      EEPROM.write(1,(byte)false);
      EEPROM.commit();
      Serial.println("BT Classic enabled, BLE disabled");
    }
    else
    {
      Serial.println("Unknown mode, use BT for Bluetooth or BLE for Bluetooth LE");
    }
    
  }
  else{
    Serial.println("Unknown command. Try using name, restart, status, or mode");
  }
  
}

void createBTServer(){
  SerialBT.begin(name); //Name of the BT Device; comment this to disable BT if not in use
  Serial.println("Bluetooth Classic Ready"); 
}

void createBLEServer(){
  BLEDevice::init(name.c_str()); //needs to be converted from String to c string.
  pServer = BLEDevice::createServer();
  pServer -> setCallbacks(new MyServerCallbacks());
  //Create BLE Service
  BLEService *pService = pServer -> createService(SERVICE_UUID);
  //Create BLE Characteristic
   pCharacteristic = pService->createCharacteristic(
                      CHARACTERISTIC_UUID_TX,
                      BLECharacteristic::PROPERTY_NOTIFY 
                    );
//Create a BLE Descriptor
  pCharacteristic->addDescriptor(new BLE2902());
  //Start the service
  pService->start();
  //Start advertising
  pServer->getAdvertising() ->start();  
  Serial.println("BLE Ready");
}


// =============================================================================
// Obsolete functions
// =============================================================================

 // digital clock display of the time
// void digitalClockDisplay() {
//   message = hour();
//   printDigits(minute());
//   printDigits(second());
//   message += " ";
//   message += day();
//   message += "-";
//   message += month();
//   message += "-";
//   message += year();
// }

 // utility function for digital clock display: prints preceding colon and leading 0
// void printDigits(int digits) {
//  message += ':';
//  if (digits < 10){
//     message +=  '0';
//   }
//  message += digits;
// }

//update and store accel data
// void updateSensor(){
//   //Read acceleration values into array  
//   imu.accelUpdate();
//   accel[0] = imu.accelX();
//   accel[1] = imu.accelY();
//   accel[2] = imu.accelZ();
//   //Read gyro values into array
//   imu.gyroUpdate();
//   gyro[0] = imu.gyroX();
//   gyro[1] = imu.gyroY();
//   gyro[2] = imu.gyroZ();

//   printSensorData();
// }

//utility function to print sensor data
// String printSensorData(){
//    message += ", Accel: ";
//   for(int i=0; i<3; ++i){
//     message += accel[i];
//     message += ", ";
//   }
//   message += "Gyro: ";
//   for(int i=0; i<3; ++i){
//     message += gyro[i];
//     message += ", ";
//   }
//   message += "Accel Sqrt: ";
//   message += imu.accelSqrt();
  
  
//   return(message);
//   //Serial.println(message);
//   //SerialBT.println(message); //send message over BT
//   //Serial.println(name); 
// }


// //blinking function to test the sensor
// void blink(const long interval){
//   SerialBT.println("Blinking for 10 seconds");
//   unsigned long currentMillis = millis();
//   if (currentMillis - previousMillis >= interval) {
//     // save the last time you blinked the LED
//     previousMillis = currentMillis;
//     // if the LED is off turn it on and vice-versa:
//     if (ledState == LOW) {
//       ledState = HIGH;
//     } else {
//       ledState = LOW;
//     }
//     // set the LED with the ledState of the variable:
//     digitalWrite(2, ledState);
//   }
// }

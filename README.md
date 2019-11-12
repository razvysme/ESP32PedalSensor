# ESP32PedalSensor
A ESP32 WROOM TTGO AIO board used a sensor for biking data, build in Arduino IDE.

To setup Arduino IDE for usage with the ESP32, follow the link below:
https://randomnerdtutorials.com/installing-the-esp32-board-in-arduino-ide-windows-instructions/

This will(probably) require the removal of default WiFi library pre-installed by Arduino(on Windows it is located at: C:\Program Files (x86)\Arduino\libraries), because the Arduino-ESP32 library overwrites some functions.

Libraries required for compilation:

https://github.com/CNMAT/OSC - OSC library

https://github.com/espressif/arduino-esp32 - ESP32 library(includes wifi library)

https://github.com/asukiaaa/MPU9250_asukiaaa - MPU9250 library - IMU Data

https://github.com/PaulStoffregen/Time - Time library

I am using an Android app called "Serial Bluetooth Terminal" to test the functionality.
There are a few commands that can be sent over BT from the terminal to the device. ALl commands start with a single letter:
n - change the name of the BT Sensor
s - sends the current status(WOP)
b - blinks an LED on the sensor
r - reboot in 3 seconds

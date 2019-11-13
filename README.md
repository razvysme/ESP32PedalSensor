# ESP32PedalSensor
A ESP32 WROOM TTGO T9 board used a sensor for biking data, build in Arduino IDE.

To setup Arduino IDE for usage with the ESP32, follow the link below:

Win: https://randomnerdtutorials.com/installing-the-esp32-board-in-arduino-ide-windows-instructions/

Mac: https://randomnerdtutorials.com/installing-the-esp32-board-in-arduino-ide-mac-and-linux-instructions/

This will(probably) require the removal of default WiFi library pre-installed by Arduino(on Windows it is located at: C:\Program Files (x86)\Arduino\libraries), because the Arduino-ESP32 library overwrites some functions.

Libraries required for compilation:

https://github.com/CNMAT/OSC - OSC library

https://github.com/espressif/arduino-esp32 - ESP32 library(includes wifi library)

https://github.com/asukiaaa/MPU9250_asukiaaa - MPU9250 library - IMU Data

https://github.com/PaulStoffregen/Time - Time library

There are a few commands that can be sent over serial from the terminal to the sensor. A list of commands can be found below:

name - change the name of the BT Sensor

status - sends the current status

restart - reboot in 3 seconds

mode - change BLE to BT and vice-versa


# ESP32PedalSensor
A ESP32 WROOM TTGO AIO board used a sensor for biking data, build in Arduino IDE.

To setup Arduino IDE for usage with the ESP32, follow the link below:
https://randomnerdtutorials.com/installing-the-esp32-board-in-arduino-ide-windows-instructions/

This will(probably) require the removal of default WiFi library pre-installed by Arduino(on Windows it is located at: C:\Program Files (x86)\Arduino\libraries), because the ESPRESSIF one overwrites some functions.

Libraries required for compilation:
https://github.com/CNMAT/OSC - OSC library
https://github.com/espressif/arduino-esp32 - ESP32 library(includes wifi library)
https://github.com/asukiaaa/MPU9250_asukiaaa - MPU9250 library - IMU Data

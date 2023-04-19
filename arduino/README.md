The reader sketch must be opened using the Arduino IDE.
The following libraries need to be installed:

- `Adafruit_Unified_Sensor`
- `DHT_sensor_library`
- `ArduinoHttpClient`

To get the correct board, the following board manager URL needs to be added in the IDE's preferences:
`https://arduino.esp8266.com/stable/package_esp8266com_index.json`

Then, the board `esp8266` needs to be installed in the board manager.

After that, create the files `apiserver.h` and `wificreds.h` in `reader` and fill it with the following content (adjust the values as necessary):

`apiserver.h`:

```c++
#ifndef APISERVER_H
#define APISERVER_H

#ifdef USE_LOCAL_SERVER
const char *serverAddress = "LOCAL DEVELOPMENT SERVER IP";
const int serverPort = 5076;
#else
const char *serverAddress = "DEPLOYED SERVER NAME";
const int serverPort = 443;
#endif /* USE_LOCAL_SERVER */

#endif /* !APISERVER_H */

```

`wificreds.h`:

```c++
#ifndef WIFICREDS_H
#define WIFICREDS_H

const char *wifiSSID = "YOUR WIFI NAME";
const char *wifiPwd = "YOUR WIFI PASSWORD";

#endif /* !WIFICREDS_H */
```

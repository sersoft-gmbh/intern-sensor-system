#include <ctime>
#include <set>
#include <ESP8266WiFi.h>
#include <WiFiClientSecure.h>
#include <ArduinoHttpClient.h>
#include <IRremote.hpp>

#include "time.h"
#include "DHT.h"

#include "wificreds.h"
#include "certs.h"
#include "apiserver.h"

#define DHTPIN 4

#define BTNPIN 5

#define IRPIN 2

#define REDPIN 16
#define GREENPIN 14
#define BLUEPIN 12

#define READ_INTERVAL 1000

#define REQ_RES_LOGGING 0

const String locations[3] = {"Office Desk", "Living Room", "Bedroom"};
const int locationsCount = (sizeof(locations) / sizeof(String));

volatile int locationIndex = 0;
volatile bool locationIndexChanged = false;
 
enum Color { red, green, blue };

DHT dht(DHTPIN, DHT11);

X509List cert(cert_ISRG_Root_X1_CA);

WiFiClientSecure wifi;
HttpClient client = HttpClient(wifi, serverAddress, serverPort);

void connectWifi() {
  WiFi.begin(wifiSSID, wifiPwd);

  Serial.print("Connecting");
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }
  Serial.println();
  Serial.print("Connected, IP address: ");
  Serial.println(WiFi.localIP()); 
}

void setupTime() {
  const char *ntpServer = "pool.ntp.org";
  const long gmtOffset_sec = 0;
  const int daylightOffset_sec = 3600;
  configTime(gmtOffset_sec, daylightOffset_sec, ntpServer);

  time_t now = time(nullptr);
  while (now < 8 * 3600 * 2) {
    delay(500);
    Serial.print(".");
    now = time(nullptr);
  }
  Serial.println("");
  struct tm timeinfo;
  gmtime_r(&now, &timeinfo);
  Serial.print("Current time: ");
  Serial.print(asctime(&timeinfo));
}

String httpErrorMessage(int status, bool isHTTPStatus) {
  switch (status) {
    case HTTP_SUCCESS: return "";
    case HTTP_ERROR_CONNECTION_FAILED: return "Connection failed!";
    case HTTP_ERROR_TIMED_OUT: return "Connection timed out!";
    case HTTP_ERROR_INVALID_RESPONSE: return "Invalid response!";
    case HTTP_ERROR_API: return "HTTP Client API error!";
    default: return isHTTPStatus ? "" : "Unknown error";
  }
}

void initLED(int pin) {
  pinMode(pin, OUTPUT);
  digitalWrite(pin, LOW);
}

void setLEDColor(std::set<enum Color> colors) {
  digitalWrite(REDPIN, colors.find(red) != colors.end() ? HIGH : LOW);
  digitalWrite(GREENPIN, colors.find(green) != colors.end() ? HIGH : LOW);
  digitalWrite(BLUEPIN, colors.find(blue) != colors.end() ? HIGH : LOW);
}

void changeLocationIndexBy(int diff) {
  if (diff == 0) return;
  changeLocationIndexTo(locationIndex + diff);
}

void changeLocationIndexTo(int newIndex) {
  while (newIndex < 0 || newIndex >= locationsCount) {
    if (newIndex >= locationsCount) {
      newIndex -= locationsCount;
    } else {
      newIndex += locationsCount;
    }
  }
  if (newIndex == locationIndex) return;
  Serial.print("New Index: ");
  Serial.println(newIndex);
  locationIndex = newIndex;
  locationIndexChanged = true;
}

double mticks() {
    struct timeval tv;
    gettimeofday(&tv, 0);
    return (double) tv.tv_usec / 1000 + tv.tv_sec * 1000;
}

volatile double lastButtonMticks = mticks();
IRAM_ATTR void buttonPressed() {
  auto currentMticks = mticks();
  auto difference = currentMticks - lastButtonMticks;
  lastButtonMticks = currentMticks;
  if (difference <= 500) return;
  Serial.println("Switching to next location...");
  changeLocationIndexBy(1);
}

volatile double lastIrButtonMticks = mticks();
IRAM_ATTR void irButtonPressed() {
  if (!IrReceiver.decode()) return;
  auto data = IrReceiver.decodedIRData;
  IrReceiver.resume();

  auto currentMticks = mticks();
  auto difference = currentMticks - lastIrButtonMticks;
  lastIrButtonMticks = currentMticks;
  if (difference <= 500) return;

  Serial.println(data.protocol);
  switch (data.command) {
    // A
    case 69: break;
    // Arrow Up
    case 70: 
      Serial.println("Switching to next location...");
      changeLocationIndexBy(1);
      break;
    // B
    case 71: break;
    // Arrow Left
    case 68: 
      Serial.println("Switching to previous location...");
      changeLocationIndexBy(-1);
      break;
    // X
    case 64: break;
    // Arrow Right
    case 67: 
      Serial.println("Switching to next location...");
      changeLocationIndexBy(1);
      break;
    // 0
    case 7: break;
    // Arrow Down
    case 21:
      Serial.println("Switching to previous location...");
      changeLocationIndexBy(-1);
      break;
    // C
    case 9: break;
    // 1
    case 22:
      Serial.println("Switching to first location...");
      changeLocationIndexTo(0);
      break;
    // 2
    case 25: 
      Serial.println("Switching to second location...");
      changeLocationIndexTo(1);
      break;
    // 3
    case 13: 
      Serial.println("Switching to third location...");
      changeLocationIndexTo(2);
      break;
    // 4
    case 12: break;
    // 5
    case 24: break;
    // 6
    case 94: break;
    // 7
    case 8: break;
    // 8
    case 28: break;
    // 9
    case 90: break;
    // Invalid
    default: break;
  }
}

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  dht.begin();
  IrReceiver.begin(IRPIN);

  pinMode(BTNPIN, INPUT_PULLUP);
  initLED(REDPIN);
  initLED(GREENPIN);
  initLED(BLUEPIN);
  setLEDColor({blue});

  attachInterrupt(digitalPinToInterrupt(BTNPIN), buttonPressed, FALLING);
  IrReceiver.registerReceiveCompleteCallback(irButtonPressed);

  connectWifi();
  setupTime();

  cert.append(cert_DigiCert_Global_Root_CA);
  wifi.setTrustAnchors(&cert);
  setLEDColor({green, blue});
}

void loop() {
  // put your main code here, to run repeatedly:
  delay(READ_INTERVAL);

  setLEDColor({red, blue});

  auto temp = dht.readTemperature();
  auto humidity = dht.readHumidity();

  if (isnan(temp) || isnan(humidity)) {
    setLEDColor({red});
    Serial.println("Failed to read!");
    return;
  }

  time_t now;
  time(&now);
  char curDate[sizeof "yyyy-MM-ddThh:mm:ssZ"];
  strftime(curDate, sizeof curDate, "%FT%TZ", gmtime(&now));

  if (locationIndexChanged) {
    locationIndexChanged = false;
    setLEDColor({red, green, blue});
    delay(500);
    setLEDColor({});
    delay(500);
    setLEDColor({red, green, blue});
    delay(500);
    setLEDColor({red, blue});
  }

  String body = "{";
  body += "\"date\":\"" + String(curDate) + "\"";
  body += ",";
  body += "\"location\":\"" + locations[locationIndex] + "\"";
  body += ",";
  body += "\"temperatureCelsius\":" + String(temp, 10);
  body += ",";
  body += "\"humidityPercent\":" + String(humidity / 100, 10);
  body += "}";

#if REQ_RES_LOGGING
  Serial.print("Request: "); 
  Serial.println(body);
#endif

  client.beginRequest();
  auto errorMessage = httpErrorMessage(client.put("/measurements"), false);
  if (!errorMessage.isEmpty()) {
    setLEDColor({red});
    Serial.println(errorMessage);
    return;
  }
  client.sendHeader("Authorization", "Bearer " + String(serverToken));
  client.sendHeader("Content-Type", "application/json");
  client.sendHeader("Content-Length", body.length());
  client.beginBody();
  client.print(body);
  client.endRequest();

  auto statusCode = client.responseStatusCode();
#if REQ_RES_LOGGING
  Serial.print("Status code: ");
  Serial.println(statusCode);
#endif
  errorMessage = httpErrorMessage(statusCode, true);
  if (!errorMessage.isEmpty()) {
    setLEDColor({red});
    Serial.println(errorMessage);
    return;
  }

  if (statusCode < 200 || statusCode >= 300) {
    setLEDColor({red}); // red and green looks more green than yellow
  } else {
    setLEDColor({green});
  }

#if REQ_RES_LOGGING
  auto response = client.responseBody();
  Serial.print("Response: ");
  Serial.println(response);
#endif
}

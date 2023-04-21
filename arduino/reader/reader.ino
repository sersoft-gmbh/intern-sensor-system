#include <ctime>
#include <set>
#include <ESP8266WiFi.h>
#include <WiFiClientSecure.h>
#include <ArduinoHttpClient.h>

#include "time.h"
#include "DHT.h"

#include "wificreds.h"
#include "certs.h"
#include "apiserver.h"

#define DHTPIN 4

#define BTNPIN 5

#define REDPIN 16
#define GREENPIN 14
#define BLUEPIN 12

#define READ_INTERVAL 1000

const String locations[3] = {"Office Desk", "Living Room", "Bedroom"};
volatile int locationIndex = 0;

enum Color { red, green, blue };

DHT dht(DHTPIN, DHT11);
WiFiClientSecure wifi;
HttpClient client = HttpClient(wifi, serverAddress, serverPort);

X509List cert(cert_DigiCert_Global_Root_CA);

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

double mticks() {
    struct timeval tv;
    gettimeofday(&tv, 0);
    return (double) tv.tv_usec / 1000 + tv.tv_sec * 1000;
}

volatile bool locationChanged = false;
volatile double lastButtonMticks = mticks();
IRAM_ATTR void buttonPressed() {
  auto currentMticks = mticks();
  auto difference = currentMticks - lastButtonMticks;
  lastButtonMticks = currentMticks;
  if (difference <= 500) return;
  const int locationsEndIndex = (sizeof(locations) / sizeof(String)) - 1;
  Serial.println("Switching Location...");
  if (locationIndex == locationsEndIndex) {
    locationIndex = 0;
  } else {
    locationIndex++;
  }
  locationChanged = true;
}

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  dht.begin();

  pinMode(BTNPIN, INPUT_PULLUP);
  initLED(REDPIN);
  initLED(GREENPIN);
  initLED(BLUEPIN);
  setLEDColor({blue});

  attachInterrupt(digitalPinToInterrupt(BTNPIN), buttonPressed, FALLING);

  connectWifi();
  setupTime();

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

  if (locationChanged) {
    setLEDColor({red, green, blue});
    delay(500);
    setLEDColor({});
    delay(500);
    setLEDColor({red, green, blue});
    delay(500);
    setLEDColor({red, blue});
    locationChanged = false;
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

  Serial.print("Request: "); 
  Serial.println(body);

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
  Serial.print("Status code: ");
  Serial.println(statusCode);
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

  auto response = client.responseBody();
  Serial.print("Response: ");
  Serial.println(response);
}

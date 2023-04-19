#include <ctime>
#include <ESP8266WiFi.h>
#include <WiFiClientSecure.h>
#include <ArduinoHttpClient.h>

#include "time.h"
#include "DHT.h"

#include "wificreds.h"
#include "certs.h"
#include "apiserver.h"

#define DHTPIN 4

#define READ_INTERVAL 1000

const String endpoint = "/measurements";
const String contentType = "application/json";
const String location = "Office Desk";

const char *ntpServer = "pool.ntp.org";
const long gmtOffset_sec = 0;
const int daylightOffset_sec = 3600;

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

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  dht.begin();

  connectWifi();
  setupTime();

  wifi.setTrustAnchors(&cert);
}

void loop() {
  // put your main code here, to run repeatedly:
  delay(READ_INTERVAL);

  auto temp = dht.readTemperature();
  auto humidity = dht.readHumidity();

  if (isnan(temp) || isnan(humidity)) {
    Serial.println("Failed to read!");
    return;
  }

  time_t now;
  time(&now);
  char curDate[sizeof "yyyy-MM-ddThh:mm:ssZ"];
  strftime(curDate, sizeof curDate, "%FT%TZ", gmtime(&now));

  String body = "{";
  body += "\"date\":\"" + String(curDate) + "\"";
  body += ",";
  body += "\"location\":\"" + location + "\"";
  body += ",";
  body += "\"temperatureCelsius\":" + String(temp, 10);
  body += ",";
  body += "\"humidityPercent\":" + String(humidity / 100, 10);
  body += "}";

  Serial.print("Request: "); 
  Serial.println(body);

  switch (client.put(endpoint, contentType, body)) {
    case HTTP_SUCCESS: break;
    case HTTP_ERROR_CONNECTION_FAILED: Serial.println("Connection failed!"); return;
    case HTTP_ERROR_TIMED_OUT: Serial.println("Connection timed out!"); return;
    case HTTP_ERROR_INVALID_RESPONSE: Serial.println("Invalid response!"); return;
    case HTTP_ERROR_API: Serial.println("HTTP Client API error!"); return;
    default: Serial.println("Unknown error"); return;
  }

  auto statusCode = client.responseStatusCode();
  Serial.print("Status code: ");
  Serial.println(statusCode);

  auto response = client.responseBody();
  Serial.print("Response: ");
  Serial.println(response);
}

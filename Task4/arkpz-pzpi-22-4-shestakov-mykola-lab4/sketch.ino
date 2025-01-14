#include <WiFi.h>
#include <HTTPClient.h>
#include "DHT.h"

#define DHTPIN 15
#define DHTTYPE DHT22
#define BUTTON_TURN_ON_OFF_PIN 16
#define LED_PIN 17
#define WIFI_SSID "Wokwi-GUEST"
#define WIFI_PASSWORD ""

int deviceId = 0;
int waterLevel = 100;
int delayTime = 0;
bool isWorking = false;
bool isWatering = false;

String serverUrl = "https://40d0bvk1-7139.euw.devtunnels.ms/api/Device";
String logDeviceData = "/log-device";
String logWatering = "/log-watering";
String checkNeedsWatering = "/check-needs-watering";

DHT dht(DHTPIN, DHTTYPE);

void toggleWorkingState();
void handleSendDeviceData();
void handleWatering();

void setup() {
  Serial.begin(115200);

  dht.begin();

  pinMode(BUTTON_TURN_ON_OFF_PIN, INPUT_PULLUP);
  pinMode(LED_PIN, OUTPUT);

  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Connecting to WiFi...");
  }
  Serial.println("Connected to WiFi");

  Serial.println("Enter device ID:");
  while (Serial.available() == 0) {
      // Wait for user input
    }
    String input = Serial.readStringUntil('\n');
    input.trim();
    deviceId = input.toInt();

    if (deviceId > 0) {
      Serial.println("Device ID updated.");
    } else {
      Serial.println("Invalid Device ID entered. Please reset and try again.");
    }
}

void loop() {
  if (digitalRead(BUTTON_TURN_ON_OFF_PIN) == LOW) {
    Serial.println("Button Pressed! Changing device state...");
    toggleWorkingState();
    delayTime++;
    delay(100);
  }

  if (deviceId > 0 && isWorking && delayTime >= 100) {
    delayTime = 0;
    if (WiFi.status() == WL_CONNECTED) {
      handleSendDeviceData();
      handleWatering();
    } else {
      Serial.println("WiFi not connected");
    }
  }

  delayTime++;
  delay(100);
}

void handleSendDeviceData() {
  HTTPClient http;
  http.begin(serverUrl + logDeviceData);

  int temperature = dht.readTemperature();
  int humidity = dht.readHumidity();

  if (isnan(temperature) || isnan(humidity)) {
    Serial.println("Failed to read from DHT sensor!");
    return;
  }

  Serial.println("Temperature: " + String(temperature));
  Serial.println("Humidity: " + String(humidity));

  String jsonPayload = String("{\"deviceId\": ") +
                       String(deviceId) +
                       String(", \"temperature\": ") +
                       String(temperature) +
                       String(", \"moisture\": ") +
                       String(humidity) +
                       String(", \"waterLevel\": ") +
                       String(waterLevel) +
                       String("}");

  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(jsonPayload);

  if (httpResponseCode > 0) {
    Serial.println("Data sent successfully: " + String(httpResponseCode));
  } else {
    Serial.println("Error sending data: " + String(http.errorToString(httpResponseCode).c_str()));
  }

  http.end();
}

void handleWatering() {
  HTTPClient http;
  http.begin(serverUrl + checkNeedsWatering + "?deviceId=" + String(deviceId));

  int httpResponseCode = http.GET();
  if (httpResponseCode == 200) {
    String response = http.getString();
    bool ledState = response.equalsIgnoreCase("true");

    if (ledState) {
      digitalWrite(LED_PIN, HIGH);
      isWatering = true;
      Serial.println("Watering ON");
      if (waterLevel > 0) {
        waterLevel -= 10;
        Serial.println("Water level: " + String(waterLevel));
      }
    } else {
      digitalWrite(LED_PIN, LOW);
      Serial.println("Watering OFF");

      if (isWatering){
        http.begin(serverUrl + logWatering);

        String jsonPayload = String(deviceId);

        http.addHeader("Content-Type", "application/json");
        int httpResponseCode = http.POST(jsonPayload);

        if (httpResponseCode > 0) {
          Serial.println("Data sent successfully: " + String(httpResponseCode));
        } else {
          Serial.println("Error sending data: " + String(http.errorToString(httpResponseCode).c_str()));
        }
      }
      
      isWatering = false;
    }
  } else {
    Serial.println("Failed to get LED state: " + String(httpResponseCode));
  }

  http.end();
}

void toggleWorkingState() {
  if (deviceId <= 0) {
    Serial.println("Device is not registered.");
    Serial.println("Working state change canceled");
    return;
  }
  isWorking = !isWorking;

  Serial.print("Device working state changed to: ");
  Serial.println(isWorking ? "On" : "Off");

  if (!isWorking) {
    digitalWrite(LED_PIN, LOW);
    Serial.println("Watering OFF");

    if (isWatering){
      HTTPClient http;
      http.begin(serverUrl + logWatering);

      String jsonPayload = String(deviceId);

      http.addHeader("Content-Type", "application/json");
      int httpResponseCode = http.POST(jsonPayload);

      if (httpResponseCode > 0) {
        Serial.println("Data sent successfully: " + String(httpResponseCode));
      } else {
        Serial.println("Error sending data: " + String(http.errorToString(httpResponseCode).c_str()));
      }
    }
      
    isWatering = false;
  }
}

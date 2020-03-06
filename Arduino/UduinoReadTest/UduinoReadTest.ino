#include <Uduino.h>

Uduino uduino("dataSender");
int lastTime;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  lastTime = millis();
}

void loop() {
  // put your main code here, to run repeatedly:
  uduino.update();
  if (uduino.isConnected()){
    if (millis() - lastTime > 3000 ){
      Serial.println("tick");
      lastTime = millis();
    }
  }
}

#include<Uduino.h>
Uduino uduino("serialControl");

bool firstTime;

void setup()
{
  Serial.begin(9600);
  pinMode(13, OUTPUT);
  uduino.addCommand("wall_on", wallOn);
  uduino.addCommand("wall_off", wallOff);
}

void wallOn() {
  digitalWrite(13, HIGH);
  Serial.println("cmd_ok");  //command executed
}

void wallOff() {
  digitalWrite(13, LOW);
  Serial.println("cmd_ok");  //command executed
}

void loop()
{
  uduino.readSerial();
  delay(10);
  
  if (uduino.isConnected()) {
    if (!firstTime) 
    { 
      Serial.println("sys_rdy");  //command executed
      firstTime = true;
    }
  }
}

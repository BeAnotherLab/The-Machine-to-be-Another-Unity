#include<Uduino.h>
Uduino uduino("serialControl");

int relay1 = 11;
int relay2 = 12;
int timeOn = 25000;
int timeOff = 1000;

void setup()
{
  pinMode(relay1, OUTPUT);
  pinMode(relay2, OUTPUT);
  digitalWrite(relay1, HIGH);
  digitalWrite(relay2, HIGH);
  Serial.begin(9600);
  uduino.addCommand("wal_on", wallOn);  
  uduino.addCommand("wal_off", wallOff);

  uduino.addCommand("mir_on", mirOn);
  uduino.addCommand("mir_off", mirOff);
}

void wallOn() {
  Serial.println("cmd_ok");  //command executed
  digitalWrite(relay2, LOW);   // turn the LED on (HIGH is the voltage level)
  delay(timeOn);   
  digitalWrite(relay2, HIGH);    // turn the LED off by making the voltage LOW
}

void wallOff() {
  Serial.println("cmd_ok");  //command executed
  digitalWrite(relay1, LOW);   // turn the LED on (HIGH is the voltage level)
  delay(timeOn);
  digitalWrite(relay1, HIGH);    // turn the LED off by making the voltage LOW
}

void mirOn(){
  Serial.println("cmd_ok");  //command executed
}

void mirOff(){
  Serial.println("cmd_ok");  //command executed
}


void loop()
{
  uduino.readSerial();
  delay(10);
}

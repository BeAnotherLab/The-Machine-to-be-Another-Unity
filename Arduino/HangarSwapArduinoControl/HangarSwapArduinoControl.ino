#include <SoftwareSerial.h>   // We need this even if we're not using a SoftwareSerial object
                              // Due to the way the Arduino IDE compiles
#include <SerialCommand.h>

SerialCommand SCmd;   // The SerialCommand object

void setup() {
  Serial.begin(9600);
  while (!Serial);

  SCmd.addCommand("wal_on", curtainDownCommand);
  SCmd.addCommand("wal_off", curtainUpCommand);
  SCmd.addCommand("wal_stop", stopCurtain);  

  pinMode(12, OUTPUT);
  pinMode(13, OUTPUT);

  stopCurtain();
}

void loop () {
  if (Serial.available() > 0)  SCmd.readSerial();   // We don't do much, just process serial commands
}

void curtainDownCommand(){
  digitalWrite(12, LOW);   // turn the LED off
  digitalWrite(13, HIGH);   // turn the LED off
}

void curtainUpCommand(){
   digitalWrite(12, HIGH);   // turn the LED on
   digitalWrite(13, LOW);   // turn the LED on
}

void stopCurtain(){
   digitalWrite(12, HIGH);   // turn the LED on
   digitalWrite(13, HIGH);   // turn the LED on
}

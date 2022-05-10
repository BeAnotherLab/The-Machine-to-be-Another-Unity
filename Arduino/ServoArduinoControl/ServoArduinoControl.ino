#include<Uduino.h>
Uduino uduino("servoControl");
#include <Servo.h>

Servo yawServo, pitchServo;  // create servo objects

void setup() {
  Serial.begin(115200);
  while (!Serial);

  yawServo.attach(9);  
  pitchServo.attach(10);

  uduino.addCommand("p", pitchCommand);
  uduino.addCommand("y", yawCommand);  

  //initialize in center position
  pitchServo.write(90); 
  yawServo.write(90); 
}

void loop () {
  uduino.readSerial();  
}

void pitchCommand() {
  servoCommand(pitchServo);    
}

void yawCommand() {
  servoCommand(yawServo);  
}

void servoCommand(Servo servo)    
{  
  float value;
  char *arg = NULL;
  arg = uduino.next();
  if (arg != NULL) {
  value = atof(arg); 
  }
   servo.write((int) value);   
   Serial.println((int) value);
}

#include <SoftwareSerial.h>   // We need this even if we're not using a SoftwareSerial object
                              // Due to the way the Arduino IDE compiles
#include <SerialCommand.h>
#include <Servo.h>

Servo yawServo, pitchServo;  // create servo objects
SerialCommand SCmd;   // The SerialCommand object

void setup() {
  Serial.begin(57600);
  while (!Serial);

  yawServo.attach(5);  
  pitchServo.attach(6);

  SCmd.addCommand("Pitch", pitchCommand);
  SCmd.addCommand("Yaw", yawCommand);  

  //initialize in center position
  pitchServo.write(90); 
  yawServo.write(90); 
}

void loop () {
  if (Serial.available() > 0)  SCmd.readSerial();   // We don't do much, just process serial commands
}

void pitchCommand() {
  servoCommand(pitchServo);    
}

void yawCommand() {
  servoCommand(yawServo);  
}

void servoCommand(Servo servo)    
{
  int aNumber;  
  char *arg; 
  
  arg = SCmd.next(); //get first argument
  if (arg != NULL) 
  {
    aNumber=atoi(arg);    // Converts a char string to an integer    
    servo.write(aNumber);
  }   
}


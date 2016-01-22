#include <SoftwareSerial.h>   // We need this even if we're not using a SoftwareSerial object
                              // Due to the way the Arduino IDE compiles
#include <SerialCommand.h>
#include <Servo.h>

#define arduinoLED 13   // Arduino LED on board

Servo yawServo, pitchServo;  // create servo objects
SerialCommand SCmd;   // The demo SerialCommand object

void setup() {
  Serial.begin(57600);
  while (!Serial);

  yawServo.attach(5);  
  pitchServo.attach(6);

  SCmd.addCommand("Pitch",pitchCommand);  // Converts two arguments to integers and echos them back 
  SCmd.addCommand("Yaw",yawCommand);  // Converts two arguments to integers and echos them back   

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

  arg = SCmd.next(); 
  if (arg != NULL) 
  {
    aNumber=atoi(arg);    // Converts a char string to an integer
    Serial.print("First argument was: "); 
    Serial.println(aNumber); 
    servo.write(aNumber);
  } 
  else {
    Serial.println("No arguments"); 
  }

  arg = SCmd.next(); 
  if (arg != NULL) 
  {
    aNumber=atol(arg); 
    Serial.print("Second argument was: "); 
    Serial.println(aNumber);     
  } 
  else {
    Serial.println("No second argument"); 
  }

}


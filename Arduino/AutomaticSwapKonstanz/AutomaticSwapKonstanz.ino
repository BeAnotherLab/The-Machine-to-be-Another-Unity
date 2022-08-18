#include<Uduino.h>

Uduino uduino("serialControl");
const int eStop = 11;

const int pinENA = 6;
const int pinIN1 = 3;
const int pinIN2 = 4;

const int pinMotorA[3] = { pinENA, pinIN1, pinIN2 };

unsigned long actualTime;
unsigned long timeDown = 2000;

void setup() 
{
  Serial.begin(115200);
 
  pinMode(eStop, INPUT_PULLUP);
  pinMode(pinIN1, OUTPUT);
  pinMode(pinIN2, OUTPUT);
  pinMode(pinENA, OUTPUT);

  uduino.addCommand("wallOn", wallOnHandler);  
  uduino.addCommand("wallOff", wallOffHandler);   
  uduino.addCommand("init", initHandler);
}

bool movingDown;

void loop() 
{
  uduino.readSerial();
  delay(10);
}

void initHandler(){
  wallOff(true);
}

void wallOnHandler() 
{
  if (!movingDown) 
  {
    movingDown = true;
    Serial.println("cmd_ok");  //command executed
    moveForward(pinMotorA, 180); //move down at minimum power
    delay(7500); //bit more than 2 meters
    fullStop(pinMotorA); //stop motor  
    movingDown = false;
  }

}

void wallOffHandler()
{
  wallOff(false);
}

void wallOff(bool confirm) 
{
  Serial.println("cmd_ok");  //command executed
  
  while(digitalRead(eStop)==0)
  { //until we reach the top
    moveBackward(pinMotorA, 255); //move up at full speed
  }
  
  //move back down a little
  moveForward(pinMotorA, 180); //second parameter is speed (0-255)
  delay(400);
  fullStop(pinMotorA); //stop motor

  if (confirm)
  {
    Serial.println("sysReady");  //command executed. only send when doing homing 
  }  
}

void moveForward(const int pinMotor[3], int speed) //go down
{
   digitalWrite(pinMotor[1], HIGH);
   digitalWrite(pinMotor[2], LOW);
   analogWrite(pinMotor[0], speed);
}

void moveBackward(const int pinMotor[3], int speed) //go up
{
   digitalWrite(pinMotor[1], LOW);
   digitalWrite(pinMotor[2], HIGH);
   analogWrite(pinMotor[0], speed);
}

void fullStop(const int pinMotor[3])
{
   digitalWrite(pinMotor[1], LOW);
   digitalWrite(pinMotor[2], LOW);
   analogWrite(pinMotor[0], 0);
}

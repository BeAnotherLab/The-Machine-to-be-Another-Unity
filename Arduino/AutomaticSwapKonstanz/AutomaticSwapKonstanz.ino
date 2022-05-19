#include<Uduino.h>

Uduino uduino("serialControl");
String inputString = "";         // a String to hold incoming data
bool stringComplete = false;  // whether the string is complete

const int eStop = 11;
int eStopState;

const int pinENA = 6;
const int pinIN1 = 3;
const int pinIN2 = 4;

const int pinMotorA[3] = { pinENA, pinIN1, pinIN2 };

bool goingUp = false;
bool goingDown = false;

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

void loop() 
{
  uduino.readSerial();
  delay(10);
}

void doHoming(bool confirm)
{
  Serial.println("cmd_ok");  //command executed

  if (confirm)
  {
    Serial.println("sysReady");  //command executed. only send when doing homing 
  }  
}

void initHandler(){
  doHoming(true);
}

void wallOnHandler() 
{
  Serial.println("cmd_ok");  //command executed
  moveForward(pinMotorA, 180); //move down at minimum power
  delay(600); //()
  //Serial.println("cmd_ok");  //command executed
  fullStop(pinMotorA); //stop motor
}

void wallOffHandler()
{
  Serial.println("cmd_ok");  //command executed
  while(digitalRead(eStop)==0){ //until we reach the top
    moveBackward(pinMotorA, 255); //move up at full speed
  }
  //move back down a little
  moveForward(pinMotorA, 180); //second parameter is speed (0-255)
  delay(400);
  fullStop(pinMotorA); //stop motor
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

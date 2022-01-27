#include<Uduino.h>
Uduino uduino("serialControl");

//Pin definitions
int stopPin = 4;
int relayPin = 5;
int dir = 8;
int step = 9;

//Constants
int wallOn = 1500; //curtain bottom position
int wallOff = 40; //curtain top position
int stepsPerRev = 200; //number of steps to go to after homing
int delayTime = 750; //the time beween digital writes when moving the curtain

//State variables
int stopState; //course end sensor value
bool homing = false;
int steps = 0;

void setup() 
{
  Serial.begin(9600);
  
  pinMode(step, OUTPUT);
  pinMode(dir, OUTPUT);
  pinMode(relayPin, OUTPUT);
  pinMode(stopPin, INPUT_PULLUP);

  uduino.addCommand("wallTo", wallTo);  
  uduino.addCommand("wallOn", wallOnHandler);  
  uduino.addCommand("wallOff", wallOffHandler);  
  
  uduino.addCommand("init", doHoming);
}

void loop() 
{
  uduino.readSerial();
  delay(10);
}

void doHoming()
{
  Serial.println("cmd_ok");  //command executed
  
  delayTime = 1500;

  //Move up until reaching the top
  brakeOFF();
  homing=false;
  digitalWrite(dir,HIGH); //UP
  
  while (!homing)
  {
    stopState = digitalRead(stopPin);
    
    if (stopState==0)
    {
      homing=true;
    }
    
    else
    {
       moveMotor();
    }
  }

  //Go to initial position
  digitalWrite(dir,LOW); //DOWN

  for (unsigned long i=0; i < stepsPerRev; i++)
  {
    moveMotor();
  }

  brakeON();
  delayTime = 750;
  steps = 0;

  Serial.println("sysReady");  //command executed
}

void wallTo()
{
  Serial.println("cmd_ok");  //command executed
  
  int stepTo = -1;
  int parameters = uduino.getNumberOfParameters(); 
  
  if (parameters > 0)
  {
    stepTo = uduino.charToInt(uduino.getParameter(0)); 
  }

  if (stepTo==-1) return;
  
  int d;
  
  if (stepTo>steps)
  {
    digitalWrite(dir,LOW); //DOWN
    d = 1;
  }
  
  if (stepTo<steps)
  {
    digitalWrite(dir,HIGH); //UP
    d = 0;
  }

  int stepsToMove = abs(stepTo - steps);
  brakeOFF();
  
  for (int i=0;i<stepsToMove;i++)
  {
    if (steps>=11000)
    {
      Serial.println("steps_error");  //command error
      break;
    }
    
    moveMotor();
    
    if (d==0)
    {
      steps--;
    }
    
    if (d==1)
    {
      steps++;
    }
  }
  
  brakeON(); 
}

void wallOnHandler() 
{
  wallHandler(true);
}


void wallOffHandler()
{
  wallHandler(false);
}

void wallHandler(bool on){
  Serial.println("cmd_ok");  //command executed

  int stepTo;
  
  if (on) stepTo = wallOn;  
  else stepTo = wallOff;  
    
  if (stepTo==-1) return;
  int d;
  
  if (stepTo>steps)
  {
    digitalWrite(dir,LOW); //DOWN
    d = 1;
  }
  
  if (stepTo<steps)
  {
    digitalWrite(dir,HIGH); //UP
    d = 0;
  }

  int stepsToMove = abs(stepTo - steps);
  brakeOFF();
  
  for (int i=0; i<stepsToMove; i++)
  {
    if(steps>=11000)
    {
      Serial.println("steps_error");  //command error
      break;
    }
    
    moveMotor();
    
    if (d==0)
    {
      steps--;
    }
    
    if (d==1)
    {
      steps++;
    }
  }
  
  brakeON(); 
}

void moveMotor()
{
  digitalWrite(step,HIGH);
  delayMicroseconds(delayTime);
  digitalWrite(step,LOW);
  delayMicroseconds(delayTime);
} 

void brakeOFF()
{
  digitalWrite(relayPin,LOW);
  delay(100);
}

void brakeON()
{
  digitalWrite(relayPin,HIGH);
}

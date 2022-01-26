#include<Uduino.h>
Uduino uduino("serialControl");

//Define pins
int stopPin = 4;
int relayPin = 5;
int step = 9;
int dir = 8;


int stepsPerRev = 200;
int numTurns = 2;
int numTurnsHoming = 1;
int delayTime = 750; 

int stopState;
bool homing = false;
int steps = 0;


void setup() {
  Serial.begin(9600);
  
  pinMode(step, OUTPUT);
  pinMode(dir, OUTPUT);
  pinMode(relayPin, OUTPUT);
  pinMode(stopPin, INPUT_PULLUP);

  uduino.addCommand("wallTo", wallTo);  
  uduino.addCommand("init", doHoming);
}

void loop() {
  uduino.readSerial();
  delay(10);
}

void doHoming(){
  
  delayTime = 1500;

  //Look for curtain 0
  brakeOFF();
  homing=false;
  digitalWrite(dir,HIGH); //UP
  
  while(!homing) {
    stopState = digitalRead(stopPin);
    if(stopState == 0) 
    {
      homing = true;
    }
    else 
    {
       moveMotor();
    }
  }

  //go to zero
  digitalWrite(dir,LOW); //DOWN

  for(int i = 0; i<stepsPerRev*numTurnsHoming; i++)
  {
    moveMotor();
  }

  brakeON();
  delayTime = 750;
  steps = 0;

  Serial.println("sys_rdy");  //command executed
}

void wallTo(){

  Serial.println("cmd_ok");  //command executed
  
  int stepTo = -1;
  int parameters = uduino.getNumberOfParameters(); // returns 2
  
  if(parameters > 0) {
    stepTo = uduino.charToInt(uduino.getParameter(0)); 
  }

  if(stepTo == -1) return;
  
  int d;
  
  if(stepTo>steps)
  {
    digitalWrite(dir,LOW); //DOWN
    d = 1;
  }
  
  if(stepTo<steps)
  {
    digitalWrite(dir,HIGH); //UP
    d = 0;
  }

  int stepsToMove = abs(stepTo - steps);
  Serial.println(stepsToMove);
  brakeOFF();
  
  for(int i = 0; i<stepsToMove; i++)
  {
    if(steps>=11000)
    {
      Serial.println("steps_error");  //command error
      break;
    }
    
    moveMotor();

    if(d==0)
    {
      steps--;
    }
    else if(d==1)
    {
      steps++;
    }
  }

  brakeON(); 
} 

void moveMotor(){
  digitalWrite(step,HIGH);
  delayMicroseconds(delayTime);
  digitalWrite(step,LOW);
  delayMicroseconds(delayTime);
}

void brakeOFF(){
  digitalWrite(relayPin,LOW);
  delay(100);
}

void brakeON(){
  digitalWrite(relayPin,HIGH);
}

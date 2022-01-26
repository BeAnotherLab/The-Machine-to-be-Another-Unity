#include<Uduino.h>
Uduino uduino("serialControl");

int step = 9;
int dir = 8;
int stepsPerRev = 200;
int numTurns = 2;
int numTurnsHoming = 1;
int delayTime = 750; //us

int relayPin = 5;
int eStopPin = 4;
int eStopState;
bool homing = false;
int steps = 0;


void setup() {
  Serial.begin(9600);
  
  pinMode(step, OUTPUT);
  pinMode(dir, OUTPUT);
  pinMode(relayPin, OUTPUT);
  pinMode(eStopPin, INPUT_PULLUP);

  uduino.addCommand("wallTo", wallTo);  
  uduino.addCommand("homing", doHoming);

  delay(1000);
  //doHoming();
}

void loop() {
  
  /*wallTo(9000);
  Serial.println(steps);
  wallTo(1000);
  Serial.println(steps);
  doHoming();
  Serial.println(steps);*/
  uduino.readSerial();
  delay(10);
}

void doHoming(){

  Serial.println("cmd_ok");  //command executed
  
  delayTime = 1500;

  //look for estop
  brakeOFF();
  homing=false;
  digitalWrite(dir,HIGH); //UP
  while(!homing){
    eStopState = digitalRead(eStopPin);
    if(eStopState==0){
      homing=true;
    }else{
       moveMotor();
    }
  }

  //goto zero
  digitalWrite(dir,LOW); //DOWN

  for(unsigned long i=0;i<stepsPerRev*numTurnsHoming;i++){
    moveMotor();
  }

  brakeON();
  delayTime = 750;
  steps = 0;

  //Serial.println("homing_ok");  //command executed
}


void wallTo(){

  Serial.println("cmd_ok");  //command executed
  
  int stepTo = -1;
  int parameters = uduino.getNumberOfParameters(); // returns 2
  
  if(parameters > 0) {
    stepTo = uduino.charToInt(uduino.getParameter(0)); 
  }

  if(stepTo==-1) return;
  
  int d;
  
  if(stepTo>steps){
    digitalWrite(dir,LOW); //DOWN
    d = 1;
  }
  
  if(stepTo<steps){
    digitalWrite(dir,HIGH); //UP
    d = 0;
  }

  int stepsToMove = abs(stepTo - steps);
  Serial.println(stepsToMove);
  brakeOFF();
  
  for(int i=0;i<stepsToMove;i++){
    if(steps>=11000){
      Serial.println("steps_error");  //command error
      break;
    }
    moveMotor();
    if(d==0){
      steps--;
    }
    if(d==1){
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

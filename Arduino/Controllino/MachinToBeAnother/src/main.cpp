//*******************************
//  Titel:    MachineToBeAnother
//  Projekt:  Kopfwelten
//  Version:  1.0
//  Autor:    che
//  Datum:    12.03.2020
//********************************
#include <Technorama.h>

bool DEBUG=false;                  //global

void setup(){
  Serial.begin(9600);              //start serial communication
  Serial.println();

  pinMode(CONTROLLINO_D0, OUTPUT); //wall start
  pinMode(CONTROLLINO_D1, OUTPUT); //wall direction
  pinMode(CONTROLLINO_D2, OUTPUT); //mirror

  pinMode(CONTROLLINO_D6, OUTPUT); //status machinery drive
  pinMode(CONTROLLINO_D7, OUTPUT); //running

  pinMode(CONTROLLINO_A0, INPUT);  //endpoint ON (inverted)
  pinMode(CONTROLLINO_A1, INPUT);  //endpoint OFF (inverted)
  pinMode(CONTROLLINO_A2, INPUT);  //machinery drive fault (true=ok)
  pinMode(CONTROLLINO_A3, INPUT);  //machinery drive ready (true=ready)
}

void thread_1(){
    static unsigned long int warteSeit;

    if (millis()-warteSeit >= 5){ //5ms
      warteSeit=millis();

      //5ms thread
      machinerydrive();
    }
}

void thread_2(){
    static unsigned long int warteSeit;

    if (millis()-warteSeit >= 5){ //5ms
      warteSeit=millis();

      //5ms thread
      reset();
    }
}

void thread_3(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 10){ //10ms
    warteSeit=millis();

    //10ms thread
    mirror();
  }
}

void thread_4(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 20){ //20ms
    warteSeit=millis();

    //20ms thread
    wall();
  }
}

void thread_5(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 20){ //20ms
    warteSeit=millis();

    //20ms thread
    serial();
  }
}


void thread_6(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 500){ //500ms
    warteSeit=millis();

    //500ms thread
    blink();
  }
}

void loop()
{
  thread_1();
  thread_2();
  thread_3();
  thread_4();
  thread_5();
  thread_6();
}

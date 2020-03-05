//*******************************
//  Titel:    Machine to be Another
//  Projekt:  Kopfwelten
//  Version:  0.4
//  Autor:    che
//  Datum:    14.01.2020
//********************************

#include <Technorama.h>

bool DEBUG=false; //Global

void setup(){
  Serial.begin(9600);  // Start Serial Communication
  Serial.println();
  Serial.println("Controllino is ready");
  Serial.println("--------------------");

  pinMode(CONTROLLINO_D0, OUTPUT); //Motor Start
  pinMode(CONTROLLINO_D1, OUTPUT); //Motor Richtung
  pinMode(CONTROLLINO_D2, OUTPUT); //Mirror

  pinMode(CONTROLLINO_D6, OUTPUT); //Systemfreigabe
  pinMode(CONTROLLINO_D7, OUTPUT); //Status

  pinMode(CONTROLLINO_A0, INPUT);  //Endanschlag ON (invertiert)
  pinMode(CONTROLLINO_A1, INPUT);  //Endanschlag OFF (invertiert)
  pinMode(CONTROLLINO_A2, INPUT);  //Externer Resetschalter
  pinMode(CONTROLLINO_A3, INPUT);  //FU Störung
  pinMode(CONTROLLINO_A4, INPUT);  //FU ready (ACHTUNG! kein digital Input)

  attachInterrupt(digitalPinToInterrupt(CONTROLLINO_IN0), notstop, CHANGE); //Notstop IN0
  attachInterrupt(digitalPinToInterrupt(CONTROLLINO_IN1), notstop, CHANGE); //Notstop IN1
}

void thread_1(){
    static unsigned long int warteSeit;

    if (millis()-warteSeit >= 2){ //2ms
      warteSeit=millis();

      //10ms thread
      serial();
    }
}

void thread_2(){
    static unsigned long int warteSeit;

    if (millis()-warteSeit >= 5){ //5ms
      warteSeit=millis();

      //10ms thread
      mirror();
    }
}

void thread_3(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 10){ //10ms
    warteSeit=millis();

    //10ms thread
    wall();
  }
}

void thread_4(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 100){ //100ms
    warteSeit=millis();

    //100ms thread
    reset();
  }
}

void thread_5(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 300){ //300ms
    warteSeit=millis();

    //300ms thread
    fu();
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

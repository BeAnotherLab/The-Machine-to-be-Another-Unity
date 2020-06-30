#include <Technorama.h>

bool DEBUG=false; //global

void setup(){
  Serial.begin(9600); //start serial communication
  Serial.println();

  //INPUTS
  pinMode(IN_ENDPOINT_ON, INPUT);   //endpoint ON (inverted)
  pinMode(IN_ENDPOINT_OFF, INPUT);  //endpoint OFF (inverted)
  pinMode(IN_MD_FAULT, INPUT);      //machinery drive fault (true=ok)
  pinMode(IN_MD_READY, INPUT);      //machinery drive ready (true=ready)
  //OUTPUTS
  pinMode(OUT_WALL_START, OUTPUT);  //wall start
  pinMode(OUT_WALL_START, OUTPUT);  //wall direction
  pinMode(OUT_CURTAIN, OUTPUT);     //curtain
  //language INPUTS
  pinMode(IN_LANG_BUT_DE, INPUT);   //language button DE
  pinMode(IN_LANG_BUT_EN, INPUT);   //language button EN
  pinMode(IN_LANG_BUT_FR, INPUT);   //language button FR
  pinMode(IN_LANG_BUT_IT, INPUT);   //language button IT
  //language OUTPUTS
  pinMode(OUT_LANG_IND_DE, OUTPUT); //language indicator DE
  pinMode(OUT_LANG_IND_EN, OUTPUT); //language indicator EN
  pinMode(OUT_LANG_IND_FR, OUTPUT); //language indicator FR
  pinMode(OUT_LANG_IND_IT, OUTPUT); //language indicator IT
  //relay BLOCK B
  pinMode(REL_MIRROR, OUTPUT);      //mirror

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

  if (millis()-warteSeit >= 10){ //10ms
    warteSeit=millis();

    //10ms thread
    mirror();
  }
}

void thread_3(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 20){ //20ms
    warteSeit=millis();

    //20ms thread
    wall();
  }
}

void thread_4(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 20){ //20ms
    warteSeit=millis();

    //20ms thread
    serial();
  }
}

void thread_5(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 20){ //20ms
    warteSeit=millis();

    //20ms thread
    curtain();
  }
}

void thread_6(){
  static unsigned long int warteSeit;

  if (millis()-warteSeit >= 20){ //20ms
    warteSeit=millis();

    //20ms thread
    language();
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

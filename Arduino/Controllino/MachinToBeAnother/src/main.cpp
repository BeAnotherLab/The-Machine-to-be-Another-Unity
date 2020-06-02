#include <Technorama.h>

Adafruit_MCP23008 mcp;

bool DEBUG=false; //global

void setup(){
  Serial.begin(9600); //start serial communication
  Serial.println();

  pinMode(CONTROLLINO_D0, OUTPUT); //wall start
  pinMode(CONTROLLINO_D1, OUTPUT); //wall direction
  pinMode(CONTROLLINO_D2, OUTPUT); //mirror
  pinMode(CONTROLLINO_D3, OUTPUT); 
  pinMode(CONTROLLINO_D4, OUTPUT); //curtain
  pinMode(CONTROLLINO_D5, OUTPUT); 

  pinMode(CONTROLLINO_A0, INPUT);  //endpoint ON (inverted)
  pinMode(CONTROLLINO_A1, INPUT);  //endpoint OFF (inverted)
  pinMode(CONTROLLINO_A2, INPUT);  //machinery drive fault (true=ok)
  pinMode(CONTROLLINO_A3, INPUT);  //machinery drive ready (true=ready)

  mcp.begin();  //use default address 0

  mcp.pinMode(0, INPUT);  //language button DE
  mcp.pullUp(0,  HIGH);   //turn on a 100K pullup internally
  mcp.pinMode(1, INPUT);  //language button EN
  mcp.pullUp(1,  HIGH);   //turn on a 100K pullup internally
  mcp.pinMode(2, INPUT);  //language button FR
  mcp.pullUp(2,  HIGH);   //turn on a 100K pullup internally
  mcp.pinMode(3, INPUT);  //language button IT
  mcp.pullUp(3,  HIGH);   //turn on a 100K pullup internally

  mcp.pinMode(4, OUTPUT); //language LED DE
  mcp.pinMode(5, OUTPUT); //language LED EN
  mcp.pinMode(6, OUTPUT); //language LED FR
  mcp.pinMode(7, OUTPUT); //language LED IT
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

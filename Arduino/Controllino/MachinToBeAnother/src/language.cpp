#include <Technorama.h>

bool butAonce = false;
bool butBonce = false;
bool butConce = false;
bool butDonce = false;

byte Language = 0;      //global 0:DE 1:EN 2:FR 3:IT
byte LanguageSave = 0;

bool FirstRunLanguage = true;

void language() {
  if (FirstRunLanguage) {
    mcp.digitalWrite(4, HIGH); //set language DE
    mcp.digitalWrite(5, LOW);
    mcp.digitalWrite(6, LOW);
    mcp.digitalWrite(7, LOW);
    FirstRunLanguage = false;
  }

  if (!mcp.digitalRead(0)) { //language button DE pressed
    if (butAonce) {
    butAonce = false;
    Language = 0;
    }
  }
  else butAonce = true;

  if (!mcp.digitalRead(1)) { //language button EN pressed
    if (butBonce) {
    butBonce = false;
    Language = 1;
    }
  }
  else butBonce = true;

  if (!mcp.digitalRead(2)) { //language button FR pressed
    if (butConce) {
    butConce = false;
    Language = 2;
    }
  }
  else butConce = true;

  if (!mcp.digitalRead(3)) { //language button IT pressed
    if (butDonce) {
    butDonce = false;
    Language = 3;
    }
  }
  else butDonce = true;

  if (Language != LanguageSave) { //change language
    LanguageSave = Language;

    switch (Language) {
      case 0:
        Serial.println("lng_de");
        mcp.digitalWrite(4, HIGH);
        mcp.digitalWrite(5, LOW);
        mcp.digitalWrite(6, LOW);
        mcp.digitalWrite(7, LOW);
      break;

      case 1:
        Serial.println("lng_en");
        mcp.digitalWrite(4, LOW);
        mcp.digitalWrite(5, HIGH);
        mcp.digitalWrite(6, LOW);
        mcp.digitalWrite(7, LOW);
      break;

      case 2:
        Serial.println("lng_fr");
        mcp.digitalWrite(4, LOW);
        mcp.digitalWrite(5, LOW);
        mcp.digitalWrite(6, HIGH);
        mcp.digitalWrite(7, LOW);
      break;

      case 3:
        Serial.println("lng_it");
        mcp.digitalWrite(4, LOW);
        mcp.digitalWrite(5, LOW);
        mcp.digitalWrite(6, LOW);
        mcp.digitalWrite(7, HIGH);
      break;    
    }
  }
}
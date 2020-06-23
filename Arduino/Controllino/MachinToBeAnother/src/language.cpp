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
    digitalWrite(OUT_LANG_IND_DE, HIGH); //set language DE
    digitalWrite(OUT_LANG_IND_EN, LOW);
    digitalWrite(OUT_LANG_IND_FR, LOW);
    digitalWrite(OUT_LANG_IND_IT, LOW);
    FirstRunLanguage = false;
  }

  if (digitalRead(IN_LANG_BUT_DE)) { //language button DE pressed
    if (butAonce) {
    butAonce = false;
    Language = 0;
    }
  }
  else butAonce = true;

  if (digitalRead(IN_LANG_BUT_EN)) { //language button EN pressed
    if (butBonce) {
    butBonce = false;
    Language = 1;
    }
  }
  else butBonce = true;

  if (digitalRead(IN_LANG_BUT_FR)) { //language button FR pressed
    if (butConce) {
    butConce = false;
    Language = 2;
    }
  }
  else butConce = true;

  if (digitalRead(IN_LANG_BUT_IT)) { //language button IT pressed
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
        digitalWrite(OUT_LANG_IND_DE, HIGH);
        digitalWrite(OUT_LANG_IND_EN, LOW);
        digitalWrite(OUT_LANG_IND_FR, LOW);
        digitalWrite(OUT_LANG_IND_IT, LOW);
      break;

      case 1:
        Serial.println("lng_en");
        digitalWrite(OUT_LANG_IND_DE, LOW);
        digitalWrite(OUT_LANG_IND_EN, HIGH);
        digitalWrite(OUT_LANG_IND_FR, LOW);
        digitalWrite(OUT_LANG_IND_IT, LOW);
      break;

      case 2:
        Serial.println("lng_fr");
        digitalWrite(OUT_LANG_IND_DE, LOW);
        digitalWrite(OUT_LANG_IND_EN, LOW);
        digitalWrite(OUT_LANG_IND_FR, HIGH);
        digitalWrite(OUT_LANG_IND_IT, LOW);
      break;

      case 3:
        Serial.println("lng_it");
        digitalWrite(OUT_LANG_IND_DE, LOW);
        digitalWrite(OUT_LANG_IND_EN, LOW);
        digitalWrite(OUT_LANG_IND_FR, LOW);
        digitalWrite(OUT_LANG_IND_IT, HIGH);
      break;    
    }
  }
}
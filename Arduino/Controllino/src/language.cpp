#include <mtba.h>

// One-shot flags for each language button
bool butAonce = false;
bool butBonce = false;
bool butConce = false;
bool butDonce = false;

// Language selection: 0 = DE, 1 = EN, 2 = FR, 3 = IT
byte Language = 0;
byte LanguageSave = 0;

bool firstRunLanguage = true;

void language() {
  // Set default language (DE) on first run
  if (firstRunLanguage) {
    digitalWrite(OUT_LANG_IND_DE, HIGH);
    digitalWrite(OUT_LANG_IND_EN, LOW);
    digitalWrite(OUT_LANG_IND_FR, LOW);
    digitalWrite(OUT_LANG_IND_IT, LOW);
    firstRunLanguage = false;
  }

  // Check DE button
  if (digitalRead(IN_LANG_BUT_DE)) {
    if (butAonce) {
      butAonce = false;
      Language = 0;
    }
  } else butAonce = true;

  // Check EN button
  if (digitalRead(IN_LANG_BUT_EN)) {
    if (butBonce) {
      butBonce = false;
      Language = 1;
    }
  } else butBonce = true;

  // Check FR button
  if (digitalRead(IN_LANG_BUT_FR)) {
    if (butConce) {
      butConce = false;
      Language = 2;
    }
  } else butConce = true;

  // Check IT button
  if (digitalRead(IN_LANG_BUT_IT)) {
    if (butDonce) {
      butDonce = false;
      Language = 3;
    }
  } else butDonce = true;

  // Update language if changed
  if (Language != LanguageSave) {
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

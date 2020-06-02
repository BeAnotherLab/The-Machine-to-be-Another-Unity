#include <Technorama.h>

bool butAonce = false;
bool butBonce = false;
bool butConce = false;
bool butDonce = false;

byte state = 0;

void language() {

    if (!mcp.digitalRead(0)) {
      if (butAonce) {
      butAonce = false;
      Serial.println("lng_de");
      state = 0;
      }
    }
    else butAonce = true;

    if (!mcp.digitalRead(1)) {
      if (butBonce) {
      butBonce = false;
      Serial.println("lng_en");
      state = 1;
      }
    }
    else butBonce = true;

    if (!mcp.digitalRead(2)) {
      if (butConce) {
      butConce = false;
      Serial.println("lng_fr");
      state = 2;
      }
    }
    else butConce = true;

    if (!mcp.digitalRead(3)) {
      if (butDonce) {
      butDonce = false;
      Serial.println("but_it");
      state = 3;
      }
    }
    else butDonce = true;

    switch (state) {
    case 0:
      mcp.digitalWrite(4, HIGH);
      mcp.digitalWrite(5, LOW);
      mcp.digitalWrite(6, LOW);
      mcp.digitalWrite(7, LOW);
    break;

    case 1:
      mcp.digitalWrite(4, LOW);
      mcp.digitalWrite(5, HIGH);
      mcp.digitalWrite(6, LOW);
      mcp.digitalWrite(7, LOW);
    break;

    case 2:
      mcp.digitalWrite(4, LOW);
      mcp.digitalWrite(5, LOW);
      mcp.digitalWrite(6, HIGH);
      mcp.digitalWrite(7, LOW);
    break;

    case 3:
      mcp.digitalWrite(4, LOW);
      mcp.digitalWrite(5, LOW);
      mcp.digitalWrite(6, LOW);
      mcp.digitalWrite(7, HIGH);
    break;    
    }
}
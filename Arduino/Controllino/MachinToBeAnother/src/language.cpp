#include <Technorama.h>

#define BTN_DE 0            //value DE button
#define BTN_EN 163          //value EN button
#define BTN_FR 312          //value FR button
#define BTN_IT 454          //value IT button
#define BTN_NONE 731        //value no button
#define RANGE 10            //range for buttons

byte LngState = 0;          //global
bool LngChange = false;     //global
bool LngChangeOnce = true;
int  LngAnalogValue;        //variable for analog input

void language() {

    LngAnalogValue = analogRead(CONTROLLINO_A4);
    //Serial.println(LngAnalogValue);

    if (LngAnalogValue <= (BTN_NONE - RANGE)) { //a button is pressed
        if (LngChangeOnce) LngChange = true;    //send just once
        if ((LngAnalogValue <= (BTN_DE + RANGE)) && (LngAnalogValue >= BTN_DE))             LngState = 0;   //DE language
        if ((LngAnalogValue <= (BTN_EN + RANGE)) && (LngAnalogValue >= (BTN_EN - RANGE)))   LngState = 1;   //EN language
        if ((LngAnalogValue <= (BTN_FR + RANGE)) && (LngAnalogValue >= (BTN_FR - RANGE)))   LngState = 2;   //FR language
        if ((LngAnalogValue <= (BTN_IT + RANGE)) && (LngAnalogValue >= (BTN_IT - RANGE)))   LngState = 3;   //IT language
    }
    else {
        LngChangeOnce = true;   //no button is pressed
    }
    if (LngChange) {
        LngChange = false;
        LngChangeOnce = false;
        
        switch (LngState)       //send the language      
        {
        case 0:
            Serial.println("lng_de");
            break;
        case 1:
            Serial.println("lng_en");
            break;
        case 2:
            Serial.println("lng_fr");
            break;
        case 3:
            Serial.println("lng_it");
            break;
        default:
            break;
        }
    }
}
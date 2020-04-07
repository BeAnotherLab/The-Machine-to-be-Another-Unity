#include <Technorama.h>

#define BTN_DE 0            //value DE button
#define BTN_EN 165          //value EN button
#define BTN_FR 315          //value FR button
#define BTN_IT 458          //value IT button
#define BTN_NONE 737        //value no button
#define RANGE 50            //range for buttons

byte LngState = 0;          //global
bool LngChange = false;     //global
bool LngChangeOnce = true;
int  LngAnalogValue;        //variable for analog input

void language() {

    LngAnalogValue = analogRead(CONTROLLINO_A4);
    //Serial.println(LngAnalogValue);

    if (LngAnalogValue <= (BTN_NONE - RANGE)) { //a button is pressed
        if (LngChangeOnce) LngChange = true;    //send just once
        
        if ((LngAnalogValue <= (BTN_DE + RANGE)) && (LngAnalogValue >= BTN_DE))                 LngState = 0;   //DE language
        else if ((LngAnalogValue <= (BTN_EN + RANGE)) && (LngAnalogValue >= (BTN_EN - RANGE)))  LngState = 1;   //EN language
        else if ((LngAnalogValue <= (BTN_FR + RANGE)) && (LngAnalogValue >= (BTN_FR - RANGE)))  LngState = 2;   //FR language
        else if ((LngAnalogValue <= (BTN_IT + RANGE)) && (LngAnalogValue >= (BTN_IT - RANGE)))  LngState = 3;   //IT language
        else LngState = 4;
    }
    else {
        LngChangeOnce = true;   //no button is pressed
    }
    if (LngChange) {
        LngChange = false;
        LngChangeOnce = false;
        
        switch (LngState) {     //send the language      
        case 0:
            Serial.println("lng_de"); //DE language
            break;
        case 1:
            Serial.println("lng_en"); //EN language
            break;
        case 2:
            Serial.println("lng_fr"); //FR language
            break;
        case 3:
            Serial.println("lng_it"); //IT language
            break;
        default:
            //Serial.println("lng_not_read");
            //Serial.println(LngAnalogValue);
            break;
        }
    }
}
/******************************************************************************* 
TODO : Clarifier : get parameter ça ne peut pas etre plus grand que 20

Uduino - Uduino — Simple and robust Arduino-Unity communication
Uduino - Copyright (C) 2016-2017 Marc Teyssier  <marc.teys@gmail.com>
http://marcteyssier.com

ArduinoSerialCommand - An Arduino library to tokenize and parse commands received over
a serial port. - MIT Licence
Copyright (C) 2011-2013 Steven Cogswell  <steven.cogswell@gmail.com>
***********************************************************************************/
#ifndef Uduino_h
#define Uduino_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "Arduino.h"
#else
#include "WProgram.h"
#endif

//#include <avr/pgmspace.h>

#if (defined(AVR))
#include <avr/pgmspace.h>
#else
#include <pgmspace.h>
#endif
// If you want to use UDUINO with the hardware serial port only, and want to disable
// SoftwareSerial support, and thus don't have to use "#include <SoftwareSerial.h>" in your
// sketches, then uncomment this define for UDUINO_HARDWAREONLY, and comment out the 
// corresponding #undef line.  
//
// You don't have to use SoftwareSerial features if this is not defined, you can still only use 
// the Hardware serial port, just that this way lets you get out of having to include 
// the SoftwareSerial.h header. 
#define UDUINO_HARDWAREONLY 1
//#undef UDUINO_HARDWAREONLY

#ifdef UDUINO_HARDWAREONLY
//#warning "Warning: Building UDUINO without SoftwareSerial Support"
#endif

#ifndef UDUINO_HARDWAREONLY 
#include <SoftwareSerial.h>  
#endif

#include <string.h>

#define RECEIVE_MAX_BUFFER 126
#define SEND_MAX_BUFFER 256
#define MAX_COMMAND_NAME 16
#define MAX_COMMANDS 16


#define MAXDELIMETER 2

//#define UDUINO_DEBUG 0
//#undef UDUINO_DEBUG      // Comment for Debug Mode

class Uduino : public Print
{
  public:
    static Uduino * _instance; 
    static char *_identity; 

    Uduino(const char* identity);                             // Constructor
    Uduino(const char* identity, const char* customDelimitier );                             // Constructor
  //  Uduino(const char* identity, const char* separator);      // Constructor
    #ifndef UDUINO_HARDWAREONLY
    Uduino(SoftwareSerial &SoftSer,char* identity);  // Constructor for using SoftwareSerial objects
    #endif

    void clearBuffer();   // Sets the command buffer to all '\0' (nulls)
    char *next();         // returns pointer to next token found in command buffer (for getting arguments to commands)
    char *nextParameter();         // returns pointer to next Parameter token found in command buffer (for getting arguments to commands)
    char *getParameter(unsigned short index);         // returns pointer to secific pointer index found in buffer (for getting arguments to commands)
    int getNumberOfParameters();

   //Update and loop
    void update();    // Main entry point.  
    void update(char inputChar);    // Main entry point with input parametev
    void readSerial();    // Main entry point with input parametev
    void readSerial(char inputChar);    // Main entry point with input parametev
   
   //Commands
    void addCommand(const char *, void(*)());   // Add commands to processing dictionary
    void addDefaultHandler(void (*function)());    // A handler to call when no valid command received. 
    void addDisconnectedFunction(void (*function)());    // A handler to call when no valid command received. 
    void addInitFunction(void (*function)());    // A handler to call when no valid command received. 
    void addPrintFunction(void (*function)(char[]));    // A handler to call when no valid command received. 
    void launchCommand(char * command);
    
    //Helpers
    int charToInt(char * arg); //Converts char to int
    void delay(unsigned int duration);    // Main entry point.  

    // Uduino specific commands
    static void printIdentity();   
    static void arduinoFound(); 
    static void arduinoDisconnected();   
    bool isInit(); 
    bool isConnected(); 
    void launchInit();
    static bool init; 
    char * getPrintedIdentity();

    static bool customPrintFunctionPreset;
    
    size_t write(uint8_t);
    size_t write(const char *str) {
        if(str == NULL)
            return 0;
        return write((const uint8_t *) str, strlen(str));
    }
    size_t write(const uint8_t *buffer, size_t size);
    size_t write(const char *buffer, size_t size) {
        return write((const uint8_t *) buffer, size);}

  private:

    char inChar;                     // A character read from the serial stream 
    char buffer[RECEIVE_MAX_BUFFER];       // Buffer of stored characters while waiting for terminator character
    char parameterBuffer[20];        // Buffer of the parameter
    uint16_t  bufPos;                // Current position in the buffer
    char delim[MAXDELIMETER];        // null-terminated list of character to be used as delimeters for tokenizing (default " ")
    char delimBundle[MAXDELIMETER];  // null-terminated list of character to be used as delimeters for tokenizing (default " ")
    char term;                       // Character that signalsc end of command (default '\r')
    char *token;                     // Returned token from the command buffer as returned by strtok_r
    char *last;                      // State variable used by strtok_r during processing
    
    typedef struct _callback {
      char command[MAX_COMMAND_NAME];
      void (*function)();
    } UduinoCallback;            // Data structure to hold Command/Handler function key-value pairs
   
   // Commands
    int numCommand;
    UduinoCallback CommandList[MAX_COMMANDS];   // Actual definition for command/handler array
    void (*defaultHandler)();           // Pointer to the default handler function 
    void (*customDisconnected)();           // Pointer to the default disconnected function 
    void (*customInit)();           // Pointer to the default init function 
    void (*customPrint)(char data[]);           // Pointer to the default init function 
    static void Empty();

    void Init(const char* identity, const char* customDelimitier);

    bool defaultFunctionPreset = false;
    bool initFunctionPreset = false;
    bool disconnectFunctionPreset = false;
   //bool customRead = false;

    #ifndef UDUINO_HARDWAREONLY 
    SoftwareSerial *SoftSerial;         // Pointer to a user-created SoftwareSerial object
    int usingSoftwareSerial;            // Used as boolean to see if we're using SoftwareSerial object or not
    #endif
};

#endif //Uduino_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "Arduino.h"
#else
#include "WProgram.h"
#endif

#include "Uduino.h"

#include <string.h>
#ifndef UDUINO_HARDWAREONLY
#include <SoftwareSerial.h>
#endif

char *Uduino::_identity = (char*)"none";
bool Uduino::init = false;
Uduino * Uduino::_instance = NULL;
bool Uduino::customPrintFunctionPreset = false;

// Constructor makes sure some things are set. 
Uduino::Uduino(const char* identity)
{
 Init(identity," ");
}


Uduino::Uduino(const char* identity, const char* customDelimitier)
{
 Init(identity, customDelimitier);  // strtok_r needs a null-terminated string
}


void Uduino::Init(const char* identity, const char* customDelimitier)
{
  Uduino::_instance = this;
  init = false;

  #ifndef UDUINO_HARDWAREONLY
  usingSoftwareSerial=0;
  #endif

  strncpy(delim,customDelimitier,MAXDELIMETER);  // strtok_r needs a null-terminated string

  //TODO !!!! Bundle pas par défault " ," faire \t par défaut

  strncpy(delimBundle," ,",MAXDELIMETER);  // strtok_r needs a null-terminated string
  term='\r';   // return character, default terminator for commands
  numCommand=0;    // Number of callback handlers installed
  clearBuffer(); 

  /* // Set comands to non-null
  for(int p=0; p < UDUINO_MAXCOMMANDS;p++)
    CommandList[p].function = &Uduino::Empty;
    */

  Uduino::_identity = (char*)identity;
  this->addCommand("identity",Uduino::printIdentity);
  this->addCommand("connected",Uduino::arduinoFound);
  this->addCommand("disconnected",Uduino::arduinoDisconnected);
  this->addInitFunction(Uduino::Empty);
  this->addDisconnectedFunction(Uduino::Empty);
}


#ifndef UDUINO_HARDWAREONLY
// Constructor to use a SoftwareSerial object
Uduino::Uduino(SoftwareSerial &_SoftSer,char* identity)
{
  usingSoftwareSerial=1; 
  SoftSerial = &_SoftSer;
  Init(identity," ");
}
#endif


char * Uduino::getPrintedIdentity() {
  char* additionnal = (char*)"uduinoIdentity "; //TODO here use custom bundle
  char* full_text;
  full_text = (char*)malloc(strlen(additionnal)+strlen(Uduino::_identity)+1); 
  strcpy(full_text, additionnal ); 
  strcat(full_text, Uduino::_identity);
  return full_text;
}

void Uduino::printIdentity() { 
  if(Uduino::_instance->customPrintFunctionPreset) {
    (*Uduino::_instance->customPrint)( Uduino::_instance->getPrintedIdentity());
  } else {
    Serial.println ( Uduino::_instance->getPrintedIdentity());
  }
  delayMicroseconds(30000); // Give sme time before executing the action
}

void Uduino::launchInit() {
  delayMicroseconds(30000); // Give sme time before executing the action
  (*customInit)();
} 

void Uduino::arduinoDisconnected() { 
  #ifdef UDUINO_DEBUG
    Serial.println(F("Arduino disconnected"));
  #endif
  init = false;
}

void Uduino::arduinoFound() { 
  init = true;
  #ifdef UDUINO_DEBUG
    Serial.println(F("Arduino connected to the software"));
  #endif
}

size_t Uduino::write(uint8_t c) {
   return Serial.print(c);
}

size_t Uduino::write(const uint8_t *buffer, size_t size) {
  if(size)
   return Serial.print((char*)buffer);
 else
  return 0;
}

bool Uduino::isConnected()
{
  return Uduino::init;
}

/* Depreciated */
bool Uduino::isInit()
{
  return Uduino::init;
}

//
// Initialize the command buffer being processed to all null characters
//
void Uduino::clearBuffer()
{
  for (int i=0; i<RECEIVE_MAX_BUFFER; i++) 
  {
    buffer[i]='\0';
  }
  bufPos=0; 
}

// Retrieve the next token ("word" or "argument") from the Command buffer.  
// returns a NULL if no more tokens exist.   
char *Uduino::nextParameter() 
{
  char *nextToken;
 // nextToken = strtok_r(NULL, delim, &last); 
  nextToken = strtok_r(NULL, delimBundle, &last); 
  return nextToken; 
}

char *Uduino::next() 
{
  return Uduino::nextParameter(); 
}

int Uduino::getNumberOfParameters() {
  size_t count = 0;
  size_t i = 0;

  if(last == NULL) return count; 
  for(i = 0; i != strlen(last); i++) {
     if(last[i] == delim[0] || last[i] == delim[1])
       count++;
  }
  return count + 1;
}

char* Uduino::getParameter(unsigned short index) 
{
  size_t len = strlen(last); 
  char *lastCopy = new char[len+1];
  strcpy(lastCopy, last); 
  lastCopy[len] = '\0';   

  char *token = strtok(lastCopy, delim);
 // parameterBuffer = {0};
  for (int i=0; i<20; i++) 
  {
    parameterBuffer[i]='\0';
  }
 // char buffer[20];

  byte i=0;
  while (token != NULL)
  {
    if(i == index) {
      strcpy(parameterBuffer, token);
      break;
    }
    token = strtok(NULL, delim);
    i++;
  }
  free(lastCopy);
 // parameterBuffer[strlen(token)]  = '\0';
  return parameterBuffer;
}

// Launch a command
void Uduino::launchCommand(char * command) {
  char * t = NULL;

  t = strtok_r(command,delimBundle,&last);

  if (t == NULL) return; 
    for (int i=0; i<numCommand; i++) {
        #ifdef UDUINO_DEBUG
        Serial.print(F("Comparing [")); 
        Serial.print(token); 
        Serial.print(F("] to ["));
        Serial.print(CommandList[i].command);
        Serial.println(F("]"));
        #endif

      if (strncmp(t,CommandList[i].command,MAX_COMMAND_NAME) == 0) 
      {
        #ifdef UDUINO_DEBUG
        Serial.print(F("Matched Command: ")); 
        Serial.println(t);
        #endif
        // Execute the stored handler function for the command
        (*CommandList[i].function)(); 
        break; 
      }
    }
}

void Uduino::update(char inputChar) 
{
    #ifdef UDUINO_DEBUG
    Serial.print(F("Uduino::update ")); 
    Serial.println(inputChar);
    #endif

    inChar = inputChar;
    int i; 
    boolean matched; 

  if (inChar==term) {     // Check for the terminator (default '\r') meaning end of command
      #ifdef UDUINO_DEBUG
      Serial.print(F("Received: ")); 
      Serial.println(buffer);
    #endif
      bufPos=0;           // Reset to start of buffer
      token = strtok_r(buffer,delimBundle,&last);   // Search for command at start of buffer
    if (token == NULL) return; 
     // if (token == '\r') return; 
     // if (token == '\n') return; 
      matched=false; 
      for (i=0; i<numCommand; i++) {

        #ifdef UDUINO_DEBUG
        Serial.print(F("Comparing [")); 
        Serial.print(token); 
        Serial.print(F("] to ["));
        Serial.print(CommandList[i].command);
        Serial.println(F("]"));
        #endif
        // Compare the found command against the list of known commands for a match
        if (strncmp(token,CommandList[i].command,RECEIVE_MAX_BUFFER) == 0) 
        {
         #ifdef UDUINO_DEBUG
          Serial.print(F("Matched Command: ")); 
          Serial.println(token);
          #endif

        /*  for(int p=0; p < UDUINO_MAXCOMMANDS;p++) {
        Serial.printf("Address of iptr: %p\n", (void*)&CommandList[p].function);
        }
        Serial.printf("Address of value: %p\n", &CommandList[i].function);
        Serial.printf("Address of iptr: %p\n", (void*)&CommandList[i].function);
        */

          #ifdef UDUINO_DEBUG
          Serial.print(F("Execute Command: ")); 
          Serial.println(token);
          #endif
          (*CommandList[i].function)(); // Execute the stored handler function for the command
          if(disconnectFunctionPreset &&  strcmp(token, "disconnected") == 0) {
            (*customDisconnected)();
          } 
          else if(initFunctionPreset && strcmp(token, "identity") == 0 ) {
           (*customInit)();
          } //else


          #ifdef UDUINO_DEBUG
          Serial.print(F("Command Executed: ")); 
          Serial.println(token);
          Serial.println((int)&token);
          #endif

          clearBuffer(); 
          matched=true; 
          break; 
        }
      }
      if (matched==false) {
        if(defaultFunctionPreset)
          (*defaultHandler)(); 
        clearBuffer(); 
      }
    }
    if (isprint(inChar))   // Only printable characters into the buffer
    {
      buffer[bufPos++]=inChar;   // Put character into buffer
      buffer[bufPos]='\0';  // Null terminate
      if (bufPos > RECEIVE_MAX_BUFFER-1) {
      	Serial.println("Error: Edit RECEIVE_MAX_BUFFER in Uduino.h to increase receive buffer");
     		 bufPos=0; // wrap buffer around if full  
  		}
    }
}

// This checks the Serial stream for characters, and assembles them into a buffer.  
// When the terminator character (default '\r') is seen, it starts parsing the 
// buffer for a prefix command, and calls handlers setup by addCommand() member
void Uduino::update() 
{
	  // If we're using the Hardware port, check it.   Otherwise check the user-created SoftwareSerial Port
  #ifdef UDUINO_HARDWAREONLY
  while (Serial.available() > 0) 
  #else
  while ((usingSoftwareSerial==0 && Serial.available() > 0) || (usingSoftwareSerial==1 && SoftSerial->available() > 0) )
  #endif
  {
    #ifndef UDUINO_HARDWAREONLY
    // SoftwareSerial port
    //  inChar = ();   // Read single available character, there may be more waiting
      update(SoftSerial->read());
    #else 
      update(Serial.read());
    #endif

    #ifdef UDUINO_DEBUG
    Serial.print(inChar);   // Echo back to serial stream
    #endif
  }
}


// Depreciated
void Uduino::readSerial() {
  Uduino::update();
}
void Uduino::readSerial(char inputChar) {
  Uduino::update(inputChar);
}

// Adds a "command" and a handler function to the list of available commands.  
// This is used for matching a found token in the buffer, and gives the pointer
// to the handler function to deal with it. 
void Uduino::addCommand(const char *command, void (*function)())
{
  if (numCommand < MAX_COMMANDS) {
    for (int i=0; i<numCommand; i++) {
      if (strncmp(command,CommandList[i].command,MAX_COMMAND_NAME) == 0) 
      {
         #ifdef UDUINO_DEBUG
          Serial.print(i); 
          Serial.print(F("-")); 
          Serial.print(F("Command ")); 
          Serial.print(command); 
          Serial.println(F(" exists")); 
         #endif
          CommandList[i].function = function; 
          return;
      }
    }
    #ifdef UDUINO_DEBUG
      Serial.print(numCommand); 
      Serial.print(F("-")); 
      Serial.print(F("Adding command for ")); 
      Serial.println(command); 
    #endif
    strncpy(CommandList[numCommand].command,command,MAX_COMMAND_NAME); 
    CommandList[numCommand].function = function; 
    numCommand++; 
  } else {
    // In this case, you tried to push more commands into the buffer than it is compiled to hold.  
    // Not much we can do since there is no real visible error assertion, we just ignore adding
    // the command
    #ifdef UDUINO_DEBUG
    Serial.println(F("Too many handlers - recompile changing UDUINO_MAXCOMMANDS")); 
    #endif 
  }
}

// This sets up a handler to be called in the event that the receveived command string
// isn't in the list of things with handlers.
void Uduino::addDefaultHandler(void (*function)())
{
  defaultFunctionPreset = true;
  defaultHandler = function;
}

void Uduino::addDisconnectedFunction(void (*function)())
{
  disconnectFunctionPreset = true;
  customDisconnected = function;
}

void Uduino::addInitFunction(void (*function)())
{
  initFunctionPreset = true;
  customInit = function;
}

void Uduino::addPrintFunction(void (*function)(char data[]))
{
    #ifdef UDUINO_DEBUG
    Serial.println(F("Adding custom print function")); 
    #endif
    customPrint = function;
    customPrintFunctionPreset = true;
}

//Converts a char * to int
int Uduino::charToInt(char * arg) {
 // String msgString = String(arg);
//  return msgString.toInt();
    return atoi(arg);
}

void Uduino::Empty() {}


void Uduino::delay(unsigned int duration) {
   unsigned long time_now = millis();
    while(millis() < time_now + duration){
          Uduino::update();
    }
}
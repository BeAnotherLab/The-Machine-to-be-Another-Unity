#include <mtba.h>

// Internal state variables
enum WallStatus {
  WALL_IDLE = 0,
  WALL_START_MOTOR = 1,
  WALL_CHECK_ENDPOINTS = 2,
  WALL_STOP = 3,
  WALL_TIMEOUT = 4,
  WALL_MD_FAULT = 5,
  WALL_MD_BLOCK = 6
};

WallStatus status = WALL_IDLE;
int timeoutCounter = 0;

// Wall movement request
struct WallRequest {
  bool requested;     // true if a movement is requested
  bool direction;     // true = ON, false = OFF
};

WallRequest wallRequest = {false, false};

// Public function to request wall movement
void requestWallMove(bool direction) {
  if (status == WALL_IDLE) {
    wallRequest.requested = true;
    wallRequest.direction = direction;
  } else {
    error(4); // State machine bussy
  }
}

// Main wall control function
void wall() {
  switch (status) {

    case WALL_IDLE:
      if (wallRequest.requested) {
        if (requestDriveRelease()) {
          digitalWrite(OUT_WALL_DIR, wallRequest.direction); // Set direction
          status = WALL_START_MOTOR;
          timeoutCounter = 0;
        } else {
          status = requestDriveReady() ? WALL_MD_BLOCK : WALL_MD_FAULT;
        }
      }
      break;

    case WALL_START_MOTOR:
      if (requestDriveRelease()) {
        digitalWrite(OUT_WALL_START, HIGH); // Start motor
        status = WALL_CHECK_ENDPOINTS;
      } else {
        status = requestDriveReady() ? WALL_MD_BLOCK : WALL_MD_FAULT;
      }
      break;

    case WALL_CHECK_ENDPOINTS:
      if (requestDriveRelease()) {
        bool endpointReached = wallRequest.direction ?
          !digitalRead(IN_ENDPOINT_ON) :
          !digitalRead(IN_ENDPOINT_OFF);

        if (endpointReached) {
          status = WALL_STOP;
        } else if (timeoutCounter >= 500) {
          status = WALL_TIMEOUT;
        } else {
          timeoutCounter++;
        }
      } else {
        status = requestDriveReady() ? WALL_MD_BLOCK : WALL_MD_FAULT;
      }
      break;

    case WALL_STOP:
      digitalWrite(OUT_WALL_START, LOW); // Stop motor
      wallRequest.requested = false;
      error(0); // No error
      //digitalWrite(OUT_WALL_DIRE, !wallRequest.direction); // Preset opposite direction
      status = WALL_IDLE;
      break;

    case WALL_TIMEOUT:
      digitalWrite(OUT_WALL_START, LOW); // Stop motor
      error(1); // Timeout error
      wallRequest.requested = false;
      status = WALL_IDLE;
      break;

    case WALL_MD_FAULT:
      digitalWrite(OUT_WALL_START, LOW); // Stop motor
      error(2); // Drive fault
      wallRequest.requested = false;
      status = WALL_IDLE;
      break;

    case WALL_MD_BLOCK:
      digitalWrite(OUT_WALL_START, LOW); // Stop motor
      error(3); // Drive block
      wallRequest.requested = false;
      status = WALL_IDLE;
      break;
  }
}
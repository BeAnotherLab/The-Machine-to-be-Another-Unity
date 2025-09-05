#include <mtba.h>

// Structure to hold the current drive status
struct DriveRequest {
  bool released;  // True if drive is released (ready && state)
  bool ready;     // True if drive is ready
  bool state;     // True if drive is OK
};

// Global instance to store drive status
DriveRequest driveRequest = {false, false, false};

// Accessor functions for drive status
bool requestDriveRelease() {
  return driveRequest.released;
}

bool requestDriveReady() {
  return driveRequest.ready;
}

bool requestDriveState() {
  return driveRequest.state;
}

// Updates the drive status based on input signals
void drive() {
  driveRequest.ready = digitalRead(CONTROLLINO_A3);  // Read drive ready signal
  driveRequest.state = digitalRead(CONTROLLINO_A2);  // Read drive OK signal
  driveRequest.released = driveRequest.ready && driveRequest.state;
}

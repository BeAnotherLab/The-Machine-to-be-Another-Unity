#include <mtba.h>

bool DEBUG = false;  // Global debug flag

void setup() {
  Serial.begin(9600);
  Serial.println();

  // === INPUTS ===
  pinMode(IN_ENDPOINT_ON, INPUT);     // Wall endpoint ON (inverted)
  pinMode(IN_ENDPOINT_OFF, INPUT);    // Wall endpoint OFF (inverted)
  pinMode(IN_DRIVE_FAULT, INPUT);     // Drive fault (true = OK)
  pinMode(IN_DRIVE_READY, INPUT);     // Drive ready (true = ready)

  // === OUTPUTS ===
  pinMode(OUT_WALL_START, OUTPUT);    // Start wall movement
  pinMode(OUT_WALL_DIR, OUTPUT);      // Wall movement direction

  // === LANGUAGE BUTTONS ===
  pinMode(IN_LANG_BUT_DE, INPUT);
  pinMode(IN_LANG_BUT_EN, INPUT);
  pinMode(IN_LANG_BUT_FR, INPUT);
  pinMode(IN_LANG_BUT_IT, INPUT);

  // === LANGUAGE INDICATORS ===
  pinMode(OUT_LANG_IND_DE, OUTPUT);
  pinMode(OUT_LANG_IND_EN, OUTPUT);
  pinMode(OUT_LANG_IND_FR, OUTPUT);
  pinMode(OUT_LANG_IND_IT, OUTPUT);

  // === RELAY ===
  pinMode(REL_MIRROR, OUTPUT);        // Mirror control relay
}

// === Time-sliced tasks ===
// Each function runs at a different interval using millis()

void timeSlice_1() {
  static unsigned long lastRun;
  if (millis() - lastRun >= 5) {
    lastRun = millis();
    drive();
  }
}

void timeSlice_2() {
  static unsigned long lastRun;
  if (millis() - lastRun >= 10) {
    lastRun = millis();
    mirror();
  }
}

void timeSlice_3() {
  static unsigned long lastRun;
  if (millis() - lastRun >= 10) {
    lastRun = millis();
    wall();
  }
}

void timeSlice_4() {
  static unsigned long lastRun;
  if (millis() - lastRun >= 20) {
    lastRun = millis();
    serial();
  }
}

void timeSlice_5() {
  static unsigned long lastRun;
  if (millis() - lastRun >= 20) {
    lastRun = millis();
    language();
  }
}

void loop() {
  timeSlice_1();
  timeSlice_2();
  timeSlice_3();
  timeSlice_4();
  timeSlice_5();
}

#include <TM1638.h>

TM1638 module(8, 9, 7);

word leds [10] = { 0, 1, 3, 7, 2063, 6175, 14399, 30783, 63551, 65280 };

byte buttons, oldbuttons, page;
byte gear, spd_h, spd_l, shift, rpm_h, rpm_l, pitLimiter, fuel, fuelLaps, waterTemp, oilTemp, lap, laps, pos, cars, minutes, seconds, millis_h, millis_l;
word rpm, spd, milliseconds;
boolean changedPage, blinkrpm;
unsigned long milstart, milstart2 = 0;
char s[8];

void setup() {
  module.setupDisplay(true, 7);
  module.setDisplayToString("BOOT", 0, 0);
  for (int i = 0; i < 10; i++) {
    delay(100);
    module.setLEDs(leds[i]);
  }
  delay(500);
  module.setLEDs(255);
  module.clearDisplay();
  module.setDisplayToString("READY", 0, 0);
  delay(1000);
  module.setLEDs(0);
  module.clearDisplay();

  Serial.begin(9600);

  oldbuttons = 0;
  page = 0;
  changedPage = false;
  blinkrpm = false;
}

void loop() {
  bool hasData = false;
  if (Serial.available() > 0) {
    if (Serial.available() == 20) {
      if (Serial.read() == 255) {
        gear = Serial.read();
        spd_h = Serial.read();
        spd_l = Serial.read();
        rpm_h = Serial.read();
        rpm_l = Serial.read();
        fuel = Serial.read();
        fuelLaps = Serial.read();
        shift = Serial.read();
        pitLimiter = Serial.read();
        waterTemp = Serial.read();
        oilTemp = Serial.read();
        lap = Serial.read();
        laps = Serial.read();
        pos = Serial.read();
        cars = Serial.read();
        minutes = Serial.read();
        seconds = Serial.read();
        millis_h = Serial.read();
        millis_l = Serial.read();
        
        rpm = (rpm_h << 8) | rpm_l;
        spd = (spd_h << 8) | spd_l;
        milliseconds = (millis_h << 8) | millis_l;
        
        hasData = true;
      }
    }
  }

  buttons = module.getButtons();
  
  /*if (!hasData && buttons == 0) {
    module.setDisplayToString("READY", 0, 0);
    module.setLEDs(256);
    delay(1000);
    return;
  }*/

  if (buttons != 0) {
    if (buttons != oldbuttons) {
      oldbuttons = buttons;
      page = buttons;
      module.clearDisplay();

      switch (page) {
        case 1:
          module.setDisplayToString("GEAR SPD", 0, 0);
          break;
        case 2:
          module.setDisplayToString("RPM", 0, 0);
          break;
        case 4:
          module.setDisplayToString("FUEL", 0, 0);
          break;
        case 8:
          module.setDisplayToString("POSITION", 0, 0);
          break;
        case 16:
          module.setDisplayToString("LAPS", 0, 0);
          break;
        case 32:
          module.setDisplayToString("WAT TEMP", 0, 0);
          break;
        case 64:
          module.setDisplayToString("OIL TEMP", 0, 0);
          break;
       case 128:
          module.setDisplayToString("LAPTIME", 0, 0);
          break;
      }

      changedPage = true;
      milstart = millis();
    }
  }
  
  if (changedPage == false) {
    switch (page) {
      case 1:
        // Gear / speed
        if (gear == 0) {
          sprintf(s, "R%7d", spd);
        } else if (gear == 1) {
          sprintf(s, "N%7d", spd);
        } else {
          sprintf(s, "%1d%7d", gear - 1, spd);
        }
        module.setDisplayToString(s);
        break;                                                                                                
      case 2:
        // RPM
        module.setDisplayToDecNumber(rpm, rpm >= 1000 ? 8 : 0, false);
        break;
      case 4:
        // Fuel
        sprintf(s, "%3dl%4d", fuel, fuelLaps);
        module.setDisplayToString(s);
        break;
      case 8:
        // position
        sprintf(s, "%4d/%3d", pos, cars);
        module.setDisplayToString(s);
        break;
      case 16:
        // laps
        sprintf(s, "%4d/%3d", lap, laps);
        module.setDisplayToString(s);
        break;
      case 32:
        // water
        sprintf(s, "%6d c", waterTemp);
        module.setDisplayToString(s);
        break;
      case 64:
        // oil
        sprintf(s, "%6d c", oilTemp);
        module.setDisplayToString(s);
        break;
      case 128:
        // lap time
        sprintf(s, "%3d%02d%03d", minutes, seconds, milliseconds);
        module.setDisplayToString(s, 40, 0);
        break;
    }
  } else {
    if ((millis() - milstart) > 2000) {
      changedPage = false;
      module.clearDisplay();
    }
  }

  // LEDs.
  if (pitLimiter == 1 || shift == 10) {
    if ((millis() - milstart2) > (pitLimiter == 1 ? 150 : 75)) {
      if (blinkrpm == false) {
        module.setLEDs(0x0000);
        blinkrpm = true;
      } else {
        module.setLEDs(pitLimiter == 1 ? 59136 : 0xFF00);
        blinkrpm = false;
      }
      milstart2 = millis();
    }
  } else {
    module.setLEDs(leds[shift]);
  }
}

#include <SoftwareSerial.h>

// RDM6300
// A1 = RX del Arduino
// A2 = TX del Arduino (no lo conectaremos físicamente)
SoftwareSerial rfid(A1, A2);

void setup() {
  Serial.begin(9600);
  rfid.begin(9600);

  Serial.println("RDM6300 listo.");
  Serial.println("Acerca una tarjeta...");
}

void loop() {

  if (rfid.available()) {

    Serial.print("Datos recibidos: ");

    while (rfid.available()) {
      byte dato = rfid.read();

      Serial.print(dato, HEX);
      Serial.print(" ");
    }

    Serial.println();
    
  }
}
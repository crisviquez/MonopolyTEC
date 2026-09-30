#include <SoftwareSerial.h>

SoftwareSerial RFID(2, 3);

byte buffer[14];
int posicion = 0;

bool tarjetaDetectada = false;

void setup() {
  Serial.begin(9600);
  RFID.begin(9600);

  Serial.println("================================");
  Serial.println("RDM6300 LISTO");
  Serial.println("================================");
}

void loop() {

  while (RFID.available()) {

    byte dato = RFID.read();


    if (dato == 0x02) {
      posicion = 0;
      buffer[posicion++] = dato;
    }

    else if (posicion > 0 && posicion < 14) {

      buffer[posicion++] = dato;

 
      if (dato == 0x03) {

      
        if (!tarjetaDetectada) {

          Serial.print("ID: ");

          for (int i = 1; i <= 10; i++) {
            Serial.write(buffer[i]);
          }

          Serial.println();

          tarjetaDetectada = true;
        }

        posicion = 0;
      }
    }
  }


  static unsigned long ultimaLectura = 0;

  if (RFID.available()) {
    ultimaLectura = millis();
  }

  if (tarjetaDetectada && millis() - ultimaLectura > 1000) {
    tarjetaDetectada = false;
  }
}
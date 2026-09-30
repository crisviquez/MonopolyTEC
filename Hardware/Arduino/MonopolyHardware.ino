#include <SoftwareSerial.h>

// Pines de los displays de 7 segmentos (A, B, C, D, E, F, G)
int display1[7] = {13, 12, 11, 10, 9, 8, 7};
int display2[7] = {A3, A4, A5, A1, A0, 4, 5};

int boton = 6;

// TX del RDM6300 -> D2
SoftwareSerial RFID(2, 3);

byte buffer[14];
int posicion = 0;

String ultimoID = "";
unsigned long ultimaTarjeta = 0;

// Segmentos encendidos para los numeros 1 al 6
int numeros[6][7] = {
  {0,1,1,0,0,0,0},
  {1,1,0,1,1,0,1},
  {1,1,1,1,0,0,1},
  {0,1,1,0,0,1,1},
  {1,0,1,1,0,1,1},
  {1,0,1,1,1,1,1}
};

void setup() {
  for (int i = 0; i < 7; i++) {
    pinMode(display1[i], OUTPUT);
    pinMode(display2[i], OUTPUT);
  }

  pinMode(boton, INPUT_PULLUP);
  randomSeed(analogRead(A2));

  Serial.begin(9600);
  RFID.begin(9600);
}

void mostrarNumero(int pines[], int numero) {
  for (int i = 0; i < 7; i++) {
    digitalWrite(pines[i], numeros[numero - 1][i]);
  }
}

void leerRFID() {
  while (RFID.available()) {
    byte dato = RFID.read();

    // Inicio de trama
    if (dato == 0x02) {
      posicion = 0;
      buffer[posicion++] = dato;
    }
    else if (posicion > 0 && posicion < 14) {
      buffer[posicion++] = dato;

      // Fin de trama
      if (dato == 0x03) {
        String id = "";
        for (int i = 1; i <= 10; i++) {
          id += (char)buffer[i];
        }

        // Evita repetir la misma tarjeta muchas veces seguidas
        if (id != ultimoID || millis() - ultimaTarjeta > 1500) {
          Serial.print("RFID:");
          Serial.println(id);
          ultimoID = id;
          ultimaTarjeta = millis();
        }

        posicion = 0;
      }
    }
  }
}

void lanzarDados() {
  int dado1 = random(1, 7);
  int dado2 = random(1, 7);

  mostrarNumero(display1, dado1);
  mostrarNumero(display2, dado2);

  // La PC (cliente C#) lee esta linea
  Serial.print("DADOS:");
  Serial.print(dado1);
  Serial.print(",");
  Serial.println(dado2);
}

void loop() {
  leerRFID();

  if (digitalRead(boton) == LOW) {
    lanzarDados();

    // Espera a que se suelte el boton
    while (digitalRead(boton) == LOW) {
    }
    delay(200);
  }
}

#include <SoftwareSerial.h>

// =========================
// DISPLAY 1
// =========================
int A = 13;
int B = 12;
int C = 11;
int D = 10;
int E = 9;
int F = 8;
int G = 7;

// =========================
// DISPLAY 2
// =========================
int AA = A3;
int BB = A4;
int CC = A5;
int DD = A1;
int EE = A0;
int FF = 4;
int GG = 5;

// =========================
// BOTÓN
// =========================
int boton = 6;

// =========================
// RFID
// TX del RDM6300 -> D2
// =========================
SoftwareSerial RFID(2, 3);

byte buffer[14];
int posicion = 0;

String ultimoID = "";
unsigned long ultimaTarjeta = 0;

// =========================
// SETUP
// =========================
void setup() {

  // Display 1
  pinMode(A, OUTPUT);
  pinMode(B, OUTPUT);
  pinMode(C, OUTPUT);
  pinMode(D, OUTPUT);
  pinMode(E, OUTPUT);
  pinMode(F, OUTPUT);
  pinMode(G, OUTPUT);

  // Display 2
  pinMode(AA, OUTPUT);
  pinMode(BB, OUTPUT);
  pinMode(CC, OUTPUT);
  pinMode(DD, OUTPUT);
  pinMode(EE, OUTPUT);
  pinMode(FF, OUTPUT);
  pinMode(GG, OUTPUT);

  // Botón
  pinMode(boton, INPUT_PULLUP);

  // Semilla aleatoria
  randomSeed(analogRead(A2));

  // Comunicación con PC
  Serial.begin(9600);

  // Comunicación con RFID
  RFID.begin(9600);

  Serial.println("SISTEMA INICIADO");
  Serial.println("RDM6300 LISTO");
}

// =========================
// MOSTRAR NÚMERO DISPLAY 1
// =========================
void mostrarNumero(int numero) {

  int numeros[6][7] = {

    // 1
    {0,1,1,0,0,0,0},

    // 2
    {1,1,0,1,1,0,1},

    // 3
    {1,1,1,1,0,0,1},

    // 4
    {0,1,1,0,0,1,1},

    // 5
    {1,0,1,1,0,1,1},

    // 6
    {1,0,1,1,1,1,1}
  };

  digitalWrite(A, numeros[numero - 1][0]);
  digitalWrite(B, numeros[numero - 1][1]);
  digitalWrite(C, numeros[numero - 1][2]);
  digitalWrite(D, numeros[numero - 1][3]);
  digitalWrite(E, numeros[numero - 1][4]);
  digitalWrite(F, numeros[numero - 1][5]);
  digitalWrite(G, numeros[numero - 1][6]);
}

// =========================
// MOSTRAR NÚMERO DISPLAY 2
// =========================
void mostrarNumero2(int numero) {

  int numeros[6][7] = {

    // 1
    {0,1,1,0,0,0,0},

    // 2
    {1,1,0,1,1,0,1},

    // 3
    {1,1,1,1,0,0,1},

    // 4
    {0,1,1,0,0,1,1},

    // 5
    {1,0,1,1,0,1,1},

    // 6
    {1,0,1,1,1,1,1}
  };

  digitalWrite(AA, numeros[numero - 1][0]);
  digitalWrite(BB, numeros[numero - 1][1]);
  digitalWrite(CC, numeros[numero - 1][2]);
  digitalWrite(DD, numeros[numero - 1][3]);
  digitalWrite(EE, numeros[numero - 1][4]);
  digitalWrite(FF, numeros[numero - 1][5]);
  digitalWrite(GG, numeros[numero - 1][6]);
}

// =========================
// LEER RFID
// =========================
void leerRFID() {

  while (RFID.available()) {

    byte dato = RFID.read();

    // Inicio de trama
    if (dato == 0x02) {

      posicion = 0;
      buffer[posicion++] = dato;
    }

    // Estamos leyendo una tarjeta
    else if (posicion > 0 && posicion < 14) {

      buffer[posicion++] = dato;

      // Fin de trama
      if (dato == 0x03) {

        String id = "";

        // Los 10 caracteres del ID
        for (int i = 1; i <= 10; i++) {

          id += (char)buffer[i];
        }

        // Evitar enviar la misma tarjeta demasiadas veces
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

// =========================
// LANZAR DADOS
// =========================
void lanzarDados() {

  int dado1 = random(1, 7);
  int dado2 = random(1, 7);

  mostrarNumero(dado1);
  mostrarNumero2(dado2);

  // Enviar a C#
  Serial.print("DADOS:");
  Serial.print(dado1);
  Serial.print(",");
  Serial.println(dado2);
}

// =========================
// LOOP
// =========================
void loop() {

  // Leer RFID constantemente
  leerRFID();

  // Botón
  if (digitalRead(boton) == LOW) {

    lanzarDados();

    // Esperar a que se suelte el botón
    while (digitalRead(boton) == LOW) {
    }

    delay(200);
  }
}
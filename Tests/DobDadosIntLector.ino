#include <SoftwareSerial.h>


// DISPLAY 1


int A = 13;
int B = 12;
int C = 11;
int D = 10;
int E = 9;
int F = 8;
int G = 7;



// DISPLAY 2


int AA = A3;
int BB = A4;
int CC = A5;
int DD = A1;
int EE = A0;
int FF = 4;
int GG = 5;



// BOTON


int boton = 6;



// RFID RDM6300


SoftwareSerial RFID(2, 3);

byte buffer[14];
int posicion = 0;

bool tarjetaDetectada = false;




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

  // Boton
  pinMode(boton, INPUT_PULLUP);

  // Semilla aleatoria
  randomSeed(analogRead(A2));

  // Serial para comunicacion con PC
  Serial.begin(9600);

  // Serial para RDM6300
  RFID.begin(9600);

  Serial.println("================================");
  Serial.println("SISTEMA INICIADO");
  Serial.println("RDM6300 LISTO");
  Serial.println("================================");
}



// MOSTRAR NUMERO DISPLAY 1


void mostrarNumero(int numero) {

  int numeros[7][7] = {

    {0,1,1,0,0,0,0},  // 1
    {1,1,0,1,1,0,1},  // 2
    {1,1,1,1,0,0,1},  // 3
    {0,1,1,0,0,1,1},  // 4
    {1,0,1,1,0,1,1},  // 5
    {1,0,1,1,1,1,1}   // 6

  };

  digitalWrite(A, numeros[numero - 1][0]);
  digitalWrite(B, numeros[numero - 1][1]);
  digitalWrite(C, numeros[numero - 1][2]);
  digitalWrite(D, numeros[numero - 1][3]);
  digitalWrite(E, numeros[numero - 1][4]);
  digitalWrite(F, numeros[numero - 1][5]);
  digitalWrite(G, numeros[numero - 1][6]);
}



// MOSTRAR NUMERO DISPLAY 2


void mostrarNumero2(int numero) {

  int numeros[7][7] = {

    {0,1,1,0,0,0,0},  // 1
    {1,1,0,1,1,0,1},  // 2
    {1,1,1,1,0,0,1},  // 3
    {0,1,1,0,0,1,1},  // 4
    {1,0,1,1,0,1,1},  // 5
    {1,0,1,1,1,1,1}   // 6

  };

  digitalWrite(AA, numeros[numero - 1][0]);
  digitalWrite(BB, numeros[numero - 1][1]);
  digitalWrite(CC, numeros[numero - 1][2]);
  digitalWrite(DD, numeros[numero - 1][3]);
  digitalWrite(EE, numeros[numero - 1][4]);
  digitalWrite(FF, numeros[numero - 1][5]);
  digitalWrite(GG, numeros[numero - 1][6]);
}


// LEER RFID


void leerRFID() {

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
}



// LOOP


void loop() {

 
  // DADOS
  
  if (digitalRead(boton) == LOW) {

    int resultado1 = random(1, 7);
    int resultado2 = random(1, 7);

    mostrarNumero(resultado1);
    mostrarNumero2(resultado2);

    Serial.print("Dado 1: ");
    Serial.println(resultado1);

    Serial.print("Dado 2: ");
    Serial.println(resultado2);

    Serial.print("Suma: ");
    Serial.println(resultado1 + resultado2);

    Serial.println("--------------------");

    delay(300);

    // Esperar a que se suelte el boton
    while (digitalRead(boton) == LOW) {
    }
  }


  
  // RFID
  

  leerRFID();


  
  // PERMITIR NUEVA TARJETA
  

  static unsigned long ultimaLectura = 0;

  if (RFID.available()) {

    ultimaLectura = millis();

  }

  if (tarjetaDetectada &&
      millis() - ultimaLectura > 1000) {

    tarjetaDetectada = false;

  }
}

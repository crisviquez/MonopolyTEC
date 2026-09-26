int A = 13;
int B = 12;
int C = 11;
int D = 10;
int E = 9;
int F = 8;
int G = 7;

int AA = A3;
int BB = A4;
int CC = A5;
int DD = 2;
int EE = 3;
int FF = 4;
int GG = 5;

int boton = 6;

void setup() {
  pinMode(A, OUTPUT);
  pinMode(B, OUTPUT);
  pinMode(C, OUTPUT);
  pinMode(D, OUTPUT);
  pinMode(E, OUTPUT);
  pinMode(F, OUTPUT);
  pinMode(G, OUTPUT);

  pinMode(AA, OUTPUT);
  pinMode(BB, OUTPUT);
  pinMode(CC, OUTPUT);
  pinMode(DD, OUTPUT);
  pinMode(EE, OUTPUT);
  pinMode(FF, OUTPUT);
  pinMode(GG, OUTPUT);

  pinMode(boton, INPUT_PULLUP);

  randomSeed(analogRead(A0));
}

void mostrarNumero(int numero) {

  int numeros[7][7] = {
    {0,1,1,0,0,0,0},
    {1,1,0,1,1,0,1},
    {1,1,1,1,0,0,1},
    {0,1,1,0,0,1,1},
    {1,0,1,1,0,1,1},
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

void mostrarNumero2(int numero) {

  int numeros[7][7] = {
    {0,1,1,0,0,0,0},
    {1,1,0,1,1,0,1},
    {1,1,1,1,0,0,1},
    {0,1,1,0,0,1,1},
    {1,0,1,1,0,1,1},
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

void loop() {

  if (digitalRead(boton) == LOW) {

    int resultado1 = random(1, 7);
    int resultado2 = random(1, 7);

    mostrarNumero(resultado1);
    mostrarNumero2(resultado2);

    delay(300);

    while (digitalRead(boton) == LOW) {
    }
  }
}

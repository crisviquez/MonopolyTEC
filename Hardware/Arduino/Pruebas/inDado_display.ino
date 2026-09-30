
int A = 13;
int B = 12;
int C = 11;
int D = 10;
int E = 9;
int F = 8;
int G = 7;

int boton = 6;

void setup() {
  pinMode(A, OUTPUT);
  pinMode(B, OUTPUT);
  pinMode(C, OUTPUT);
  pinMode(D, OUTPUT);
  pinMode(E, OUTPUT);
  pinMode(F, OUTPUT);
  pinMode(G, OUTPUT);

  pinMode(boton, INPUT_PULLUP);

  randomSeed(analogRead(A0));
}

void mostrarNumero(int numero) {

  int numeros[7][7] = {
    {0,1,1,0,0,0,0}, // 1
    {1,1,0,1,1,0,1}, // 2
    {1,1,1,1,0,0,1}, // 3
    {0,1,1,0,0,1,1}, // 4
    {1,0,1,1,0,1,1}, // 5
    {1,0,1,1,1,1,1}  // 6
  };

  digitalWrite(A, numeros[numero - 1][0]);
  digitalWrite(B, numeros[numero - 1][1]);
  digitalWrite(C, numeros[numero - 1][2]);
  digitalWrite(D, numeros[numero - 1][3]);
  digitalWrite(E, numeros[numero - 1][4]);
  digitalWrite(F, numeros[numero - 1][5]);
  digitalWrite(G, numeros[numero - 1][6]);
}

void loop() {

  if (digitalRead(boton) == LOW) {

    int resultado = random(1, 7);

    mostrarNumero(resultado);

    delay(300);

    while (digitalRead(boton) == LOW) {
    }
  }
}


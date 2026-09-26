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
}

void mostrarNumero(int numero) {

  int numeros[10][7] = {
    {1,1,1,1,1,1,0},
    {0,1,1,0,0,0,0},
    {1,1,0,1,1,0,1},
    {1,1,1,1,0,0,1},
    {0,1,1,0,0,1,1},
    {1,0,1,1,0,1,1},
    {1,0,1,1,1,1,1},
    {1,1,1,0,0,0,0},
    {1,1,1,1,1,1,1},
    {1,1,1,1,0,1,1}
  };

  digitalWrite(A, numeros[numero][0]);
  digitalWrite(B, numeros[numero][1]);
  digitalWrite(C, numeros[numero][2]);
  digitalWrite(D, numeros[numero][3]);
  digitalWrite(E, numeros[numero][4]);
  digitalWrite(F, numeros[numero][5]);
  digitalWrite(G, numeros[numero][6]);

  digitalWrite(AA, numeros[numero][0]);
  digitalWrite(BB, numeros[numero][1]);
  digitalWrite(CC, numeros[numero][2]);
  digitalWrite(DD, numeros[numero][3]);
  digitalWrite(EE, numeros[numero][4]);
  digitalWrite(FF, numeros[numero][5]);
  digitalWrite(GG, numeros[numero][6]);
}

void loop() {

  for (int i = 0; i <= 9; i++) {

    mostrarNumero(i);

    delay(1000);
  }
}

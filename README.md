# MonopolyTEC

Monopoly distribuido con estructuras de datos lineales propias, arquitectura cliente-servidor por TCP y un módulo electrónico (Arduino) con dados de 2 dígitos y lector RFID. Proyecto 1 de Algoritmos y Estructuras de Datos 1, ITCR, Semestre 2 2026.

Temática: equipos deportivos (fútbol, NFL, NBA, MLB).

## Cómo funciona

- Un **servidor** (el banco) guarda el estado oficial: saldos, posiciones, propiedades, turnos, dados y transacciones.
- Hasta **4 clientes** (consola o gráfico) piden acciones al servidor, que las valida.
- Los **dados** son físicos (Arduino con 2 displays de 7 segmentos y un botón).
- Las **tarjetas RFID** identifican al jugador y confirman compras, pagos y fin de turno. El saldo nunca está en la tarjeta.

## Estructuras propias

| Estructura | Uso |
|---|---|
| `ListaCircularDoble` | Tablero de 40 casillas |
| `ColaCircular` | Turnos y mazo de cartas |
| `ListaDoble` | Historial de transacciones |
| `ListaSimple` | Jugadores, propiedades, clientes, eventos |

## Estructura del repositorio

```text
Core/            Lógica del juego (Juego, Banco, Tablero, casillas, cartas, transacciones)
DataStructures/  Listas, cola circular y nodos propios
Network/         Protocolo (Mensaje, DTOs) y ClienteTcp
Server/          Servidor TCP y consola del organizador
Client/          Cliente de consola y lectura del puerto serial
ClientGUI/       Cliente gráfico (Windows Forms)
Tests/           Pruebas de consola
Hardware/        Programas del Arduino
Documentation/   Documentación
```

## Inicio rápido

```text
dotnet run --project Server
dotnet run --project ClientGUI
```

Ver el manual para el hardware, la segunda computadora y el registro de tarjetas.

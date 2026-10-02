# Protocolo de Comunicación TCP

## 1. Introducción

Este documento describe el protocolo entre el **servidor** (el banco, que guarda el estado oficial de la partida) y los **clientes** (uno por jugador). Los mensajes viajan por **TCP**, en **JSON**, uno por línea.

```text
Cliente ── Mensaje JSON + "\n" ──► Servidor
Cliente ◄── Mensaje JSON + "\n" ── Servidor
```

El cliente **nunca** modifica saldo, posición, propiedades, turno, dados ni transacciones. Solo pide acciones; el servidor valida y responde.

Las definiciones están en `Network/Protocolo.cs`, compartido por `Client`, `ClientGUI` y `Server`.

---

## 2. Tecnologías

- **TCP** (`TcpClient` / `TcpListener`) en el puerto **5000**.
- **NetworkStream** para enviar y recibir bytes.
- **UTF-8** como codificación.
- **JSON** (`System.Text.Json`) como formato.
- **Thread** para escuchar en segundo plano (un hilo por cliente en el servidor, uno en `ClienteTcp`).

---

## 3. Formato de un mensaje

Todo mensaje es un objeto `Mensaje`:

```csharp
public class Mensaje
{
    public TipoMensaje Tipo { get; set; }
    public object? Datos { get; set; }
}
```

- **Tipo:** qué representa el mensaje. En el JSON va **como texto** (`"CONECTAR"`, no un número), porque `TipoMensaje` tiene `[JsonConverter(typeof(JsonStringEnumConverter))]`.
- **Datos:** el contenido. Es `null` si el tipo no necesita datos.

Los nombres de las propiedades JSON son exactamente los de C# (`Tipo`, `Datos`, `IdJugador`, ...).

Ejemplo real de lo que viaja por el cable (una sola línea terminada en `\n`):

```text
{"Tipo":"TIRAR_DADO","Datos":{"Dado1":3,"Dado2":5}}\n
```

### Delimitador

TCP es un flujo de bytes sin límites de mensaje, así que **cada mensaje termina en `\n`**. El JSON de `System.Text.Json` no contiene saltos de línea, por lo que una línea = un mensaje.

---

## 4. Tipos de mensajes

### 4.1 Cliente → Servidor

| Tipo | Datos | Requiere tarjeta | Descripción |
|---|---|---|---|
| `CONECTAR` | `DatosConectar` | No | Pide unirse a la partida. |
| `REGISTRAR_TARJETA` | `DatosTarjeta` (`IdTarjeta`, `NombreJugador`) | No | Asocia una tarjeta RFID a un jugador. Solo en la sala de espera. |
| `TIRAR_DADO` | `DatosTirarDado` | No | Envía los valores de los dados físicos. |
| `COMPRAR_PROPIEDAD` | `null` | Sí | Quiere comprar la propiedad ofrecida. |
| `NO_COMPRAR` | `null` | Sí | Rechaza la propiedad ofrecida. |
| `PAGAR_DEUDA` | `null` | Sí | Paga la deuda pendiente (alquiler, impuesto, carta). |
| `TERMINAR_TURNO` | `null` | Sí | Termina su turno. |
| `CONSULTAR_TRANSACCIONES` | `null` | Sí | Pide el historial de transacciones. |
| `TARJETA_RFID` | `DatosTarjeta` (solo `IdTarjeta`) | — | Se acercó una tarjeta al lector; confirma la acción pendiente de su dueño. |
| `DESCONECTAR` | `null` | No | Sale de la sala o abandona la partida. |

### 4.2 Servidor → Cliente

| Tipo | Datos | A quién | Descripción |
|---|---|---|---|
| `CONEXION_ACEPTADA` | `DatosConexionAceptada` | Solo al que se conectó | Confirma la conexión y entrega su id. |
| `CONEXION_RECHAZADA` | `DatosConexionRechazada` | Solo al que se conectó | Motivo del rechazo; después el servidor cierra el socket. |
| `ACTUALIZAR_ESTADO` | `DatosActualizarEstado` | Todos | Estado de todos los jugadores. |
| `TU_TURNO` | `DatosTuTurno` | Todos | Qué jugador tiene el turno (solo cuando cambia). |
| `RESULTADO_DADO` | `DatosResultadoDado` | Todos | Dados que sacó el jugador del turno. |
| `OFERTA_COMPRA` | `DatosOfertaCompra` | Solo al jugador que cayó | Propiedad libre que puede comprar. |
| `HISTORIAL_TRANSACCIONES` | `DatosHistorialTransacciones` | Solo al que lo pidió | Historial de la más antigua a la más reciente. |
| `EVENTO` | `DatosEvento` | Todos (o solo uno) | Texto informativo. Con `IdJugador = 0` es un evento general. |
| `ERROR` | `DatosError` | Solo al afectado | Una validación falló. |
| `FIN_JUEGO` | `DatosFinJuego` | Todos | La partida terminó. |

> No existe `CONSULTAR_ESTADO`: el servidor envía `ACTUALIZAR_ESTADO` a todos después de cada acción importante, así que el cliente no necesita pedirlo.

---

## 5. Datos de cada mensaje

### `DatosConectar`
```json
{ "Tipo": "CONECTAR", "Datos": { "Nombre": "Cristopher" } }
```
Si el nombre viene vacío el servidor asigna `JugadorN`. Los nombres no distinguen mayúsculas.

### `DatosConexionAceptada`
```json
{
  "Tipo": "CONEXION_ACEPTADA",
  "Datos": { "IdJugador": 1, "NumeroCasillas": 40, "Jugadores": [] }
}
```
`NumeroCasillas` es el tamaño real del tablero. Si llega 0 el cliente usa 40.

### `DatosConexionRechazada`
```json
{ "Tipo": "CONEXION_RECHAZADA", "Datos": { "Motivo": "La partida esta llena" } }
```
Motivos posibles: `La partida ya inicio`, `La partida esta llena`, `Ese nombre ya esta en uso`, `No se pudo agregar al jugador`.

### `JugadorEstado` (dentro de `ACTUALIZAR_ESTADO` y `CONEXION_ACEPTADA`)
```json
{
  "Id": 1, "Nombre": "Ana", "Posicion": 12, "Saldo": 1340,
  "Propiedades": [1, 3], "EnBancarrota": false, "TieneTarjeta": true
}
```
`Propiedades` es la lista de ids de casilla que posee el jugador.

### `DatosActualizarEstado`
```json
{ "Tipo": "ACTUALIZAR_ESTADO", "Datos": { "Jugadores": [ { "Id": 1, "...": "..." } ] } }
```

### `DatosTuTurno`
```json
{ "Tipo": "TU_TURNO", "Datos": { "IdJugador": 2 } }
```
El primer `TU_TURNO` indica además que la partida comenzó.

### `DatosTirarDado` (dados físicos)
```json
{ "Tipo": "TIRAR_DADO", "Datos": { "Dado1": 3, "Dado2": 4 } }
```

### `DatosResultadoDado`
```json
{ "Tipo": "RESULTADO_DADO", "Datos": { "IdJugador": 1, "Dado1": 4, "Dado2": 6 } }
```

### `DatosOfertaCompra`
```json
{
  "Tipo": "OFERTA_COMPRA",
  "Datos": { "IdCasilla": 3, "NombrePropiedad": "Saprissa", "Precio": 60 }
}
```
El jugador responde con `COMPRAR_PROPIEDAD` o `NO_COMPRAR`.

### `DatosHistorialTransacciones`
```json
{
  "Tipo": "HISTORIAL_TRANSACCIONES",
  "Datos": {
    "Transacciones": [
      {
        "Id": 1, "FechaHora": "2026-10-02 14:03:11", "Turno": 1,
        "Tipo": "COMPRA_PROPIEDAD", "JugadorOrigen": "Ana", "JugadorDestino": "Banco",
        "Monto": 60, "Descripcion": "Ana compro Saprissa"
      }
    ]
  }
}
```
`TransaccionInfo` es solo el formato de red (DTO); no es la clase `Transaccion` del servidor. Los tipos son: `COMPRA_PROPIEDAD`, `PAGO_ALQUILER`, `PAGO_BANCO`, `PAGO_ENTRE_JUGADORES`, `GANANCIA_EVENTO`, `PERDIDA_EVENTO`, `PREMIO_INICIO`.

### `DatosEvento`
```json
{ "Tipo": "EVENTO", "Datos": { "IdJugador": 0, "Descripcion": "Ana avanza 7 casillas" } }
```

### `DatosError`
```json
{ "Tipo": "ERROR", "Datos": { "Mensaje": "No es tu turno" } }
```

### `DatosFinJuego`
```json
{ "Tipo": "FIN_JUEGO", "Datos": { "IdGanador": 2 } }
```

---

## 6. Ciclo de una partida

### 6.1 Sala de espera

1. El cliente envía `CONECTAR`.
2. El servidor responde `CONEXION_ACEPTADA` (o `CONEXION_RECHAZADA` y cierra), y envía `ACTUALIZAR_ESTADO` + `EVENTO` a todos.
3. Cada jugador registra su tarjeta con `REGISTRAR_TARJETA`. Una tarjeta no puede pertenecer a dos jugadores y no se acepta con la partida iniciada.
4. La partida inicia cuando:
   - el organizador presiona **ENTER** en la consola del servidor (mínimo 2 jugadores, todos con tarjeta), o
   - hay 4 jugadores y todas las tarjetas están registradas (inicia sola).
5. Si falta alguna tarjeta el servidor no inicia y avisa con un `EVENTO` quiénes faltan.

`DESCONECTAR` (o cerrar el socket) en la sala de espera saca al jugador de la sala.

### 6.2 Partida en curso

Solo el jugador con el turno puede actuar. Un turno típico:

```text
TIRAR_DADO ─► (mover) ─► caer en casilla ─► [OFERTA_COMPRA | deuda | carta | nada]
          ─► COMPRAR / NO_COMPRAR / PAGAR_DEUDA (con tarjeta)
          ─► TERMINAR_TURNO (con tarjeta) ─► TU_TURNO del siguiente
```

El servidor rechaza `TERMINAR_TURNO` si no se lanzaron los dados, si hay una deuda sin pagar o si hay una oferta de compra sin decidir.

`DESCONECTAR` durante la partida equivale a abandonarla: el jugador queda eliminado.

### 6.3 Fin de la partida

La partida termina cuando queda un único jugador activo o se alcanza el límite de turnos (configurable al arrancar el servidor, 60 por defecto; cuenta turnos de todos los jugadores). Con el límite de turnos gana el mayor patrimonio (saldo + precio de las propiedades). El servidor envía `FIN_JUEGO` y exporta `transacciones_partida.txt`.

---

## 7. Dados físicos

Los dados y el lector RFID pertenecen al módulo Arduino. **El servidor no genera números aleatorios**: solo valida los que recibe.

`TIRAR_DADO` no requiere tarjeta. Como el módulo es compartido, el servidor aplica la tirada **al jugador que tiene el turno**, sin importar qué cliente la envió. Se rechaza (`ERROR`) si:

- los valores no están entre 1 y 6,
- la partida no ha iniciado o ya terminó,
- ya se lanzaron los dados en ese turno.

Después de una tirada válida el servidor mueve al jugador por los nodos del tablero, ejecuta la casilla y publica los cambios.

---

## 8. Acciones con tarjeta (dos pasos)

Todas las acciones, excepto lanzar los dados y salir, requieren la tarjeta del jugador:

1. El jugador elige la acción (`COMPRAR_PROPIEDAD`, `NO_COMPRAR`, `PAGAR_DEUDA`, `TERMINAR_TURNO` o `CONSULTAR_TRANSACCIONES`, todas con `Datos = null`).
2. El servidor guarda la acción como **pendiente** de ese cliente y le responde solo a él con un `EVENTO`: *"Acerca tu tarjeta al lector para ..."*.
3. Se acerca la tarjeta al lector y el cliente que tiene el módulo envía:
   ```json
   { "Tipo": "TARJETA_RFID", "Datos": { "IdTarjeta": "0A1B2C3D4E" } }
   ```
4. El servidor identifica al dueño de la tarjeta, ejecuta **su** acción pendiente y publica los cambios (`ACTUALIZAR_ESTADO`, `EVENTO`). Para `CONSULTAR_TRANSACCIONES` responde con `HISTORIAL_TRANSACCIONES` solo a ese jugador.

Como la tarjeta identifica al jugador, el módulo puede estar en un solo PC y servir a todos. El saldo y las validaciones siempre están en el servidor.

Si el jugador **no alcanza a pagar** una deuda obligatoria, `PAGAR_DEUDA` no devuelve error: queda eliminado, entrega lo que le queda al acreedor y libera sus propiedades.

```text
Cliente                              Servidor
   │ TIRAR_DADO {4,6}                   │
   ├───────────────────────────────────►│
   │ ACTUALIZAR_ESTADO, TU_TURNO*, RESULTADO_DADO, EVENTO...
   │◄───────────────────────────────────┤
   │ OFERTA_COMPRA (solo al que cayó)   │
   │◄───────────────────────────────────┤
   │ COMPRAR_PROPIEDAD                  │
   ├───────────────────────────────────►│
   │ EVENTO "Acerca tu tarjeta..."      │
   │◄───────────────────────────────────┤
   │ TARJETA_RFID {IdTarjeta}           │
   ├───────────────────────────────────►│
   │ ACTUALIZAR_ESTADO + EVENTO         │
   │◄───────────────────────────────────┤
   * TU_TURNO solo si el turno cambió
```

---

## 9. Orden de los mensajes tras una acción

El cliente redibuja el tablero al recibir estado o turno, y eso borra lo impreso antes. Por eso el servidor siempre publica en este orden:

1. `ACTUALIZAR_ESTADO` (a todos)
2. `TU_TURNO` (solo si el turno cambió)
3. `RESULTADO_DADO` (solo si fue una tirada)
4. Un `EVENTO` por cada texto acumulado
5. `FIN_JUEGO` (solo si la partida terminó)

Después de una tirada, el `OFERTA_COMPRA` (si corresponde) llega al final y solo al jugador que cayó en la propiedad.

---

## 10. Errores

Los `ERROR` llegan solo al cliente afectado. En acciones con tarjeta, los errores de la acción van al **dueño de la tarjeta**; los de identificación (tarjeta desconocida, etc.) van al cliente que envió `TARJETA_RFID`.

| Situación | Mensaje |
|---|---|
| Acción sin haberse conectado | `Primero debes conectarte` |
| Tipo de mensaje no válido | `Mensaje no reconocido` |
| `CONECTAR` repetido | `Ya estas conectado` |
| Datos de dados o tarjeta ausentes | `Faltan los valores de los dados` / `Faltan los datos de la tarjeta` |
| Partida sin iniciar | `La partida no ha iniciado` |
| Partida terminada | `La partida ya termino` |
| Jugador eliminado | `Estas eliminado` |
| Fuera de turno | `No es tu turno` |
| Dados repetidos | `Ya lanzaste los dados en este turno` |
| Dado fuera de rango | `Cada dado debe valer entre 1 y 6` |
| Comprar sin oferta | `No hay ninguna propiedad disponible para comprar` |
| Propiedad ya comprada | `La propiedad ya tiene propietario` |
| Saldo insuficiente al comprar | `No tienes saldo suficiente para comprar <propiedad>` |
| No comprar sin oferta | `No hay ninguna oferta de compra pendiente` |
| Pagar sin deuda | `No tienes ninguna deuda pendiente` |
| Terminar sin tirar | `Debes lanzar los dados antes de terminar el turno` |
| Terminar con deuda | `Tienes una deuda pendiente, debes pagarla antes de terminar el turno` |
| Terminar con oferta abierta | `Debes decidir si compras la propiedad (comprar o no comprar)` |
| Tarjeta desconocida | `Tarjeta no registrada` |
| Dueño de la tarjeta desconectado | `El dueno de la tarjeta no esta conectado` |
| Sin acción pendiente | `<nombre> no ha elegido ninguna accion que requiera tarjeta` |
| Registro con partida iniciada | `No se pueden registrar tarjetas con la partida iniciada` |
| Tarjeta vacía | `Tarjeta invalida` |
| Jugador inexistente al registrar | `No existe un jugador llamado <nombre>` |
| Tarjeta de otro jugador | `Esa tarjeta ya pertenece a <nombre>` |

---

## 11. Envío y recepción

### Enviar

```text
Mensaje ─► Serializar() ─► JSON ─► + "\n" ─► UTF-8 ─► bytes ─► NetworkStream
```

```csharp
string textoSerializado = mensaje.Serializar() + "\n";
byte[] bytes = Encoding.UTF8.GetBytes(textoSerializado);
stream.Write(bytes, 0, bytes.Length);
```

### Recibir

TCP puede entregar un mensaje partido en varios `Read()` o varios mensajes pegados en uno. Por eso `ClienteTcp` **acumula** el texto en un `StringBuilder` y corta por cada `\n`:

```text
bytes ─► UTF-8 ─► acumulador ─► buscar "\n" ─► línea completa ─► Deserializar() ─► Mensaje
```

El servidor (`ClienteConectado`) usa `StreamReader.ReadLine()`, que ya hace ese corte.

Si una línea no se puede deserializar, o si quien escucha el evento lanza una excepción, el mensaje se descarta y el hilo de lectura **sigue vivo** (`ClienteTcp.ProcesarLinea`).

### Leer `Datos`

`Datos` es `object?`, así que se convierte con `LeerDatos<T>()`:

```csharp
DatosResultadoDado resultado = mensaje.LeerDatos<DatosResultadoDado>();
```

### Eventos de `ClienteTcp`

```csharp
public event Action<Mensaje>? MensajeRecibido;   // llegó un mensaje completo
public event Action? Desconectado;                // se cerró la conexión
```

Ambos se disparan desde el hilo de red. En `ClientGUI` se pasan al hilo de la interfaz con `BeginInvoke`.

---

## 12. Flujo general

```text
┌──────────────┐                     ┌──────────────┐
│    CLIENTE   │                     │   SERVIDOR   │
└──────┬───────┘                     └──────┬───────┘
       │ CONECTAR                           │
       ├───────────────────────────────────►│
       │ CONEXION_ACEPTADA                  │
       │◄───────────────────────────────────┤
       │ REGISTRAR_TARJETA                  │
       ├───────────────────────────────────►│
       │ ACTUALIZAR_ESTADO, EVENTO          │
       │◄───────────────────────────────────┤
       │            (inicia la partida)     │
       │ ACTUALIZAR_ESTADO, TU_TURNO, EVENTO│
       │◄───────────────────────────────────┤
       │ TIRAR_DADO                         │
       ├───────────────────────────────────►│
       │ ACTUALIZAR_ESTADO, RESULTADO_DADO, EVENTO
       │◄───────────────────────────────────┤
       │ ... acciones con tarjeta ...       │
       │ TERMINAR_TURNO + TARJETA_RFID      │
       ├───────────────────────────────────►│
       │ ACTUALIZAR_ESTADO, TU_TURNO        │
       │◄───────────────────────────────────┤
       │ FIN_JUEGO                          │
       │◄───────────────────────────────────┤
```

---

## 13. Lectura serial del Arduino

El Arduino escribe por serial (9600 baudios), una línea por evento. El cliente C# (`DispositivoHardware`) las lee y las convierte en mensajes del protocolo:

| Línea serial | Qué hace el cliente |
|---|---|
| `DADOS:3,5` | Envía `TIRAR_DADO` con `Dado1 = 3`, `Dado2 = 5`. |
| `RFID:0A1B2C3D4E` | Si está registrando una tarjeta envía `REGISTRAR_TARJETA`; si no, envía `TARJETA_RFID`. |

El ID RFID son los 10 caracteres ASCII de la trama del RDM6300.

---

## 14. Reglas del protocolo

1. Todos los mensajes usan la estructura `Mensaje`.
2. `Tipo` es un valor de `TipoMensaje`, escrito como texto en el JSON.
3. `Datos` usa la clase correspondiente al tipo, o es `null`.
4. Todo mensaje termina en `\n` y se codifica en UTF-8.
5. El receptor acumula bytes hasta encontrar `\n`; cada línea es un mensaje.
6. El cliente mantiene la escucha del servidor en segundo plano.
7. Una desconexión se notifica con el evento `Desconectado`.
8. El cliente solo solicita acciones; el servidor valida todo y es la única fuente del estado.
9. Cliente y servidor deben compilarse con la misma versión de `Network/Protocolo.cs`.
# Protocolo de Comunicación TCP

## 1. Introducción

Este documento describe el protocolo utilizado para la comunicación entre el **cliente** y el **servidor** del juego.

La comunicación utiliza **TCP** y los mensajes se intercambian en formato **JSON**.

```text
Cliente
   │
   │  Mensaje JSON + "\n"
   ▼
Servidor
   │
   │  Mensaje JSON + "\n"
   ▼
Cliente
```

El protocolo define qué tipos de mensajes existen, qué datos contiene cada uno y cómo deben enviarse y recibirse.

---

## 2. Tecnologías utilizadas

- **TCP:** conexión confiable entre cliente y servidor.
- **TcpClient:** conexión TCP utilizada por el cliente.
- **NetworkStream:** canal para enviar y recibir bytes.
- **UTF-8:** codificación del texto.
- **JSON:** formato de los mensajes.
- **Thread:** permite escuchar al servidor en segundo plano.

---

## 3. Estructura de un mensaje

Todos los mensajes utilizan la clase `Mensaje`:

```csharp
public class Mensaje
{
    public TipoMensaje Tipo { get; set; }
    public object? Datos { get; set; }
}
```

Tiene dos componentes:

- **Tipo:** indica qué acción o evento representa el mensaje.
- **Datos:** contiene la información adicional necesaria.

Por ejemplo:

```text
Tipo = RESULTADO_DADO
Datos = IdJugador: 1, Dado1: 4, Dado2: 6
```

---

## 4. Tipos de mensajes

### Cliente → Servidor

| Tipo | Descripción | Tarjeta |
|---|---|---|
| `CONECTAR` | Solicita conectarse al servidor. | No |
| `REGISTRAR_TARJETA` | Asocia una tarjeta RFID a un jugador (solo en la sala de espera). | No |
| `TIRAR_DADO` | Envía el resultado de los dados físicos. | No |
| `DESCONECTAR` | Solicita cerrar la conexión. | No |
| `COMPRAR_PROPIEDAD` | Solicita comprar la propiedad ofrecida. | Sí |
| `NO_COMPRAR` | Rechaza la propiedad ofrecida. | Sí |
| `PAGAR_DEUDA` | Solicita pagar la deuda pendiente. | Sí |
| `TERMINAR_TURNO` | Solicita terminar el turno. | Sí |
| `CONSULTAR_TRANSACCIONES` | Solicita el historial de transacciones. | Sí |
| `TARJETA_RFID` | Se acercó una tarjeta al lector; confirma la acción pendiente de su dueño. | — |

### Servidor → Cliente

| Tipo | Descripción |
|---|---|
| `CONEXION_ACEPTADA` | Confirma la conexión. |
| `CONEXION_RECHAZADA` | Informa que la conexión fue rechazada. |
| `ACTUALIZAR_ESTADO` | Envía el estado actual de los jugadores. |
| `TU_TURNO` | Indica qué jugador tiene el turno. |
| `RESULTADO_DADO` | Envía el resultado de los dados. |
| `OFERTA_COMPRA` | El jugador cayó en una propiedad libre y puede comprarla. |
| `HISTORIAL_TRANSACCIONES` | Envía el historial solicitado. |
| `EVENTO` | Notifica un evento del juego (también pide la tarjeta al jugador). |
| `ERROR` | Informa sobre un error. |
| `FIN_JUEGO` | Indica que la partida terminó. |

## 5. Datos asociados a los mensajes

Cada tipo de mensaje puede tener una clase específica para `Datos`.

### `CONECTAR`

Utiliza `DatosConectar`:

```csharp
public class DatosConectar
{
    public string Nombre { get; set; } = "";
}
```

Ejemplo:

```json
{
    "Tipo": "CONECTAR",
    "Datos": {
        "Nombre": "Cristopher"
    }
}
```

### `CONEXION_ACEPTADA`

Utiliza `DatosConexionAceptada`:

```text
IdJugador
Jugadores
```

Ejemplo:

```json
{
    "Tipo": "CONEXION_ACEPTADA",
    "Datos": {
        "IdJugador": 1,
        "Jugadores": []
    }
}
```

### `CONEXION_RECHAZADA`

Utiliza `DatosConexionRechazada`:

```text
Motivo
```

Ejemplo:

```json
{
    "Tipo": "CONEXION_RECHAZADA",
    "Datos": {
        "Motivo": "La partida está llena"
    }
}
```

### `ACTUALIZAR_ESTADO`

Utiliza `DatosActualizarEstado`, que contiene una lista de `JugadorEstado`.

Cada jugador tiene:

```text
Id
Nombre
Posicion
Saldo
Propiedades
EnBancarrota
TieneTarjeta
```

### `TU_TURNO`

Utiliza `DatosTuTurno`:

```json
{
    "Tipo": "TU_TURNO",
    "Datos": {
        "IdJugador": 1
    }
}
```

### `RESULTADO_DADO`

Utiliza `DatosResultadoDado`:

```text
IdJugador
Dado1
Dado2
```

Ejemplo:

```json
{
    "Tipo": "RESULTADO_DADO",
    "Datos": {
        "IdJugador": 1,
        "Dado1": 4,
        "Dado2": 6
    }
}
```

### `EVENTO`

Utiliza `DatosEvento`:

```text
IdJugador
Descripcion
```

Ejemplo:

```json
{
    "Tipo": "EVENTO",
    "Datos": {
        "IdJugador": 1,
        "Descripcion": "Cristopher compró la propiedad 12"
    }
}
```

### `ERROR`

Utiliza `DatosError`:

```json
{
    "Tipo": "ERROR",
    "Datos": {
        "Mensaje": "No tienes suficiente dinero"
    }
}
```

### `FIN_JUEGO`

Utiliza `DatosFinJuego`:

```json
{
    "Tipo": "FIN_JUEGO",
    "Datos": {
        "IdGanador": 2
    }
}
```

---

## 6. Serialización

Antes de enviar un mensaje, el objeto `Mensaje` se convierte a JSON:

```csharp
public string Serializar()
{
    return JsonSerializer.Serialize(this);
}
```

Por ejemplo:

```csharp
Mensaje mensaje = new Mensaje(
    TipoMensaje.CONECTAR,
    new DatosConectar
    {
        Nombre = "Cristopher"
    }
);
```

Se convierte conceptualmente en:

```json
{
    "Tipo": "CONECTAR",
    "Datos": {
        "Nombre": "Cristopher"
    }
}
```

---

## 7. Envío mediante TCP

El método `Enviar()` realiza tres pasos principales:

```csharp
string textoSerializado = mensaje.Serializar() + "\n";
byte[] bytes = Encoding.UTF8.GetBytes(textoSerializado);
stream.Write(bytes, 0, bytes.Length);
```

El proceso es:

```text
Mensaje
   ↓
JSON
   ↓
Agregar "\n"
   ↓
UTF-8
   ↓
Bytes
   ↓
NetworkStream
   ↓
Servidor
```

### ¿Por qué se agrega `\n`?

TCP es un flujo continuo de bytes. No indica automáticamente dónde termina un mensaje.

Por eso este protocolo utiliza `\n` como **delimitador**.

Por ejemplo:

```text
{"Tipo":"CONECTAR","Datos":{"Nombre":"Cristopher"}}\n
{"Tipo":"TIRAR_DADO","Datos":null}\n
```

Cada salto de línea representa el final de un mensaje.

---

## 8. Recepción de mensajes

`EscucharServidor()` permanece esperando información mientras:

```csharp
activo == true
```

Primero recibe bytes:

```csharp
int leidos = stream.Read(buffer, 0, buffer.Length);
```

Después los convierte a texto:

```csharp
string textoRecibido =
    Encoding.UTF8.GetString(buffer, 0, leidos);
```

El texto se guarda en un `StringBuilder`.

---

## 9. ¿Por qué existe un acumulador?

Un mensaje TCP puede llegar dividido en varias recepciones.

Por ejemplo, un mensaje podría llegar así:

```text
Recepción 1:
{"Tipo":"RESULTADO

Recepción 2:
_DADO","Datos":...}\n
```

Por eso no se debe asumir que cada `Read()` contiene un mensaje completo.

El cliente acumula los datos hasta encontrar `\n`:

```text
Datos recibidos
      ↓
Acumulador
      ↓
Buscar "\n"
      ↓
Mensaje completo
      ↓
Deserializar
```

Esto permite procesar correctamente mensajes completos aunque lleguen fragmentados.

---

## 10. Deserialización

Cuando se encuentra un mensaje completo, se utiliza:

```csharp
Mensaje.Deserializar(linea);
```

El objetivo es convertir:

```text
JSON → objeto Mensaje
```

La implementación debe retornar el resultado de la deserialización:

```csharp
public static Mensaje? Deserializar(string json)
{
    return JsonSerializer.Deserialize<Mensaje>(json);
}
```

---

## 11. Lectura de `Datos`

Como `Datos` es de tipo `object?`, después de recibir un mensaje se debe convertir al tipo correspondiente.

Para esto existe:

```csharp
LeerDatos<T>()
```

Por ejemplo:

```csharp
DatosResultadoDado resultado =
    mensaje.LeerDatos<DatosResultadoDado>();
```

Después se puede acceder a:

```csharp
resultado.IdJugador
resultado.Dado1
resultado.Dado2
```

El proceso es:

```text
JSON
 ↓
Mensaje
 ↓
Datos
 ↓
LeerDatos<T>()
 ↓
Clase específica
```

---

## 12. Eventos

`ClienteTcp` utiliza dos eventos:

```csharp
public event Action<Mensaje>? MensajeRecibido;
public event Action? Desconectado;
```

### `MensajeRecibido`

Se ejecuta cuando llega correctamente un mensaje del servidor.

```csharp
MensajeRecibido.Invoke(mensaje);
```

Esto permite que la lógica del juego reaccione sin tener que leer directamente el `NetworkStream`.

```text
Servidor
   ↓
ClienteTcp
   ↓
MensajeRecibido
   ↓
Lógica del juego
   ↓
Actualizar interfaz
```

### `Desconectado`

Se ejecuta cuando la conexión se pierde o el servidor cierra la conexión.

---

## 13. Ejemplo completo: tirar los dados

### 1. El cliente crea el mensaje

```csharp
Mensaje mensaje =
    new Mensaje(TipoMensaje.TIRAR_DADO);
```

### 2. Lo envía

```csharp
cliente.Enviar(mensaje);
```

### 3. Se convierte a JSON

```json
{
    "Tipo": "TIRAR_DADO",
    "Datos": null
}
```

### 4. Se agrega el delimitador

```text
{"Tipo":"TIRAR_DADO","Datos":null}\n
```

### 5. Se convierte a bytes y se envía

```text
Cliente ───────────────► Servidor
          TIRAR_DADO
```

### 6. El servidor procesa la solicitud

El servidor realiza la tirada.

### 7. El servidor responde

```json
{
    "Tipo": "RESULTADO_DADO",
    "Datos": {
        "IdJugador": 1,
        "Dado1": 4,
        "Dado2": 6
    }
}
```

### 8. El cliente recibe y deserializa

```csharp
DatosResultadoDado resultado =
    mensaje.LeerDatos<DatosResultadoDado>();
```

El juego obtiene:

```text
Jugador: 1
Dado 1: 4
Dado 2: 6
Total: 10
```

---

## 14. Flujo general

```text
┌──────────────┐                     ┌──────────────┐
│    CLIENTE   │                     │   SERVIDOR   │
└──────┬───────┘                     └──────┬───────┘
       │                                    │
       │ CONECTAR                           │
       ├───────────────────────────────────►│
       │                                    │
       │ CONEXION_ACEPTADA                  │
       │◄───────────────────────────────────┤
       │                                    │
       │ TIRAR_DADO                         │
       ├───────────────────────────────────►│
       │                                    │
       │ RESULTADO_DADO                     │
       │◄───────────────────────────────────┤
       │                                    │
       │ ACTUALIZAR_ESTADO                  │
       │◄───────────────────────────────────┤
       │                                    │
       │ TERMINAR_TURNO                     │
       ├───────────────────────────────────►│
       │                                    │
       │ TU_TURNO                            │
       │◄───────────────────────────────────┤
       │                                    │
       │ FIN_JUEGO                           │
       │◄───────────────────────────────────┤
```

---

## 15. Reglas del protocolo

Para mantener la compatibilidad entre cliente y servidor:

1. Todos los mensajes deben utilizar la estructura `Mensaje`.
2. `Tipo` debe corresponder a un valor de `TipoMensaje`.
3. `Datos` debe utilizar la clase correspondiente al tipo de mensaje.
4. Los mensajes deben serializarse como JSON.
5. Cada mensaje debe terminar con `\n`.
6. El texto debe codificarse utilizando UTF-8.
7. Los datos recibidos deben acumularse hasta encontrar `\n`.
8. Cada línea completa representa un mensaje independiente.
9. Los mensajes deben deserializarse antes de ser procesados.
10. `Datos` debe convertirse al tipo correspondiente mediante `LeerDatos<T>()`.
11. El cliente debe mantener la escucha del servidor en segundo plano.
12. Una desconexión debe notificarse mediante el evento `Desconectado`.

---

## 16. Resumen

### Para enviar

```text
Objeto Mensaje
      ↓
 Serializar()
      ↓
    JSON
      ↓
   + "\n"
      ↓
   UTF-8
      ↓
    Bytes
      ↓
    TCP
```

### Para recibir

```text
    TCP
     ↓
   Bytes
     ↓
   UTF-8
     ↓
Acumulador
     ↓
 Buscar "\n"
     ↓
   JSON
     ↓
Deserializar()
     ↓
  Mensaje
     ↓
LeerDatos<T>()
     ↓
Datos específicos
```

De esta forma, cliente y servidor comparten un protocolo común, estructurado y predecible para comunicarse durante la partida.

## 17. Extensiones agregadas (historial y oferta de compra)

Estos mensajes se agregaron para cubrir dos partes del enunciado que faltaban en el protocolo original: la consulta del historial de transacciones y la decisión explícita de comprar o no una propiedad.

### Cliente → Servidor

| Tipo | Descripción |
|---|---|
| `NO_COMPRAR` | El jugador decide no comprar la propiedad ofrecida. |
| `CONSULTAR_TRANSACCIONES` | Solicita el historial completo de transacciones de la partida. |

### Servidor → Cliente

| Tipo | Descripción |
|---|---|
| `OFERTA_COMPRA` | Avisa que el jugador cayó en una propiedad libre y puede comprarla. |
| `HISTORIAL_TRANSACCIONES` | Envía el historial de transacciones solicitado. |

### `OFERTA_COMPRA`

Utiliza `DatosOfertaCompra`:

```csharp
public class DatosOfertaCompra
{
    public int IdCasilla { get; set; }
    public string NombrePropiedad { get; set; } = "";
    public int Precio { get; set; }
}
```

Ejemplo:

```json
{
    "Tipo": "OFERTA_COMPRA",
    "Datos": {
        "IdCasilla": 12,
        "NombrePropiedad": "Avenida Central",
        "Precio": 200
    }
}
```

El cliente responde con `COMPRAR_PROPIEDAD` o `NO_COMPRAR` según lo que decida el jugador.

### `HISTORIAL_TRANSACCIONES`

Utiliza `DatosHistorialTransacciones`, que contiene una lista de `TransaccionInfo`:

```csharp
public class TransaccionInfo
{
    public int Id { get; set; }
    public string FechaHora { get; set; } = "";
    public int Turno { get; set; }
    public string Tipo { get; set; } = "";
    public string JugadorOrigen { get; set; } = "";
    public string JugadorDestino { get; set; } = "";
    public int Monto { get; set; }
    public string Descripcion { get; set; } = "";
}
```

`TransaccionInfo` es solo el formato de red (DTO); no es la clase `Transaccion` que use el servidor internamente, que puede tener su propia representación mientras al enviarla la convierta a este formato.

### Cambio en `CONEXION_ACEPTADA`

Se agregó el campo `NumeroCasillas` a `DatosConexionAceptada`, para que el cliente sepa el tamaño real del tablero sin tener que asumirlo:

```csharp
public class DatosConexionAceptada
{
    public int IdJugador { get; set; }
    public int NumeroCasillas { get; set; }
    public List<JugadorEstado> Jugadores { get; set; } = new List<JugadorEstado>();
}
```

Si el servidor no lo envía (o envía 0), el cliente usa 40 como valor por defecto.

## 18. Dados físicos y tarjetas RFID

Los dados y el lector RFID pertenecen al módulo físico (Arduino). El servidor no genera números aleatorios: solo valida los valores que recibe.

### Registro obligatorio de tarjetas

Todos los jugadores deben tener una tarjeta registrada. El servidor rechaza iniciar la partida (por `ENTER` en consola o automáticamente con 4 jugadores) mientras falte alguna, y avisa con un `EVENTO` quiénes faltan. La partida inicia sola cuando hay 4 jugadores y todas las tarjetas están registradas.

`REGISTRAR_TARJETA` solo se acepta en la sala de espera. Una tarjeta no puede pertenecer a dos jugadores.

```json
{
    "Tipo": "REGISTRAR_TARJETA",
    "Datos": { "IdTarjeta": "0A1B2C3D4E", "NombreJugador": "Ana" }
}
```

### Acciones con tarjeta (dos pasos)

Todas las acciones, excepto lanzar los dados y salir de la partida, requieren tarjeta:

1. El jugador elige la acción (`COMPRAR_PROPIEDAD`, `NO_COMPRAR`, `PAGAR_DEUDA`, `TERMINAR_TURNO` o `CONSULTAR_TRANSACCIONES`, todas con `Datos = null`).
2. El servidor guarda la acción como pendiente y responde solo a ese cliente con un `EVENTO`: "Acerca tu tarjeta al lector para ...".
3. Se acerca la tarjeta al lector y el cliente con el módulo envía `TARJETA_RFID`:

```json
{
    "Tipo": "TARJETA_RFID",
    "Datos": { "IdTarjeta": "0A1B2C3D4E" }
}
```

4. El servidor identifica al dueño de la tarjeta, ejecuta **su** acción pendiente y publica los cambios (`ACTUALIZAR_ESTADO`, `EVENTO`). Para `CONSULTAR_TRANSACCIONES` responde con `HISTORIAL_TRANSACCIONES` solo a ese jugador.

Como la tarjeta identifica al jugador, el módulo puede estar en un solo PC y servir a todos. El saldo y las validaciones siempre están en el servidor.

Errores posibles (`ERROR`): tarjeta no registrada, el dueño de la tarjeta no está conectado, el dueño no ha elegido ninguna acción, no es su turno, no tiene deuda pendiente, saldo insuficiente.

### `TIRAR_DADO` (sin tarjeta)

```json
{
    "Tipo": "TIRAR_DADO",
    "Datos": { "Dado1": 3, "Dado2": 4 }
}
```

El módulo es compartido, por lo que el servidor aplica la tirada al jugador que tiene el turno. Rechaza la tirada si los valores no están entre 1 y 6, si la partida no ha iniciado, si ya se lanzaron los dados en ese turno o si la partida terminó.

### Ejemplo: comprar una propiedad

```text
Cliente                         Servidor
   │ TIRAR_DADO {4,6}              │
   ├──────────────────────────────►│
   │ RESULTADO_DADO, ACTUALIZAR_ESTADO, EVENTO
   │◄──────────────────────────────┤
   │ OFERTA_COMPRA (solo al que cayó)
   │◄──────────────────────────────┤
   │ COMPRAR_PROPIEDAD             │
   ├──────────────────────────────►│
   │ EVENTO "Acerca tu tarjeta..." │
   │◄──────────────────────────────┤
   │ TARJETA_RFID {IdTarjeta}      │
   ├──────────────────────────────►│
   │ ACTUALIZAR_ESTADO + EVENTO    │
   │◄──────────────────────────────┤
```

### Lectura serial del Arduino (cliente C#)

El Arduino escribe por serial (9600 baudios), una línea por evento:

```text
DADOS:3,5
RFID:0A1B2C3D4E
```
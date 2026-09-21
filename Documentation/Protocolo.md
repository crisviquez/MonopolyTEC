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

| Tipo | Descripción |
|---|---|
| `CONECTAR` | Solicita conectarse al servidor. |
| `TIRAR_DADO` | Solicita realizar una tirada. |
| `COMPRAR_PROPIEDAD` | Solicita comprar una propiedad. |
| `PAGAR_DEUDA` | Indica que se debe pagar una deuda. |
| `TERMINAR_TURNO` | Indica que el jugador terminó su turno. |
| `DESCONECTAR` | Solicita cerrar la conexión. |

### Servidor → Cliente

| Tipo | Descripción |
|---|---|
| `CONEXION_ACEPTADA` | Confirma la conexión. |
| `CONEXION_RECHAZADA` | Informa que la conexión fue rechazada. |
| `ACTUALIZAR_ESTADO` | Envía el estado actual de los jugadores. |
| `TU_TURNO` | Indica qué jugador tiene el turno. |
| `RESULTADO_DADO` | Envía el resultado de los dados. |
| `EVENTO` | Notifica un evento del juego. |
| `ERROR` | Informa sobre un error. |
| `FIN_JUEGO` | Indica que la partida terminó. |

---

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

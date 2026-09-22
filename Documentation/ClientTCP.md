# ClienteTcp - Documentación

## 1. ¿Qué es `ClienteTcp`?

`ClienteTcp` es una clase encargada de manejar la comunicación entre un **cliente y un servidor utilizando TCP**.

Permite:

* Conectarse a un servidor.
* Enviar objetos `Mensaje`.
* Recibir mensajes del servidor.
* Detectar cuando el servidor se desconecta.
* Mantener la recepción de mensajes en segundo plano.
* Cerrar la conexión.

La clase pertenece al namespace:

```csharp
namespace Network
```

Por lo tanto, normalmente se utiliza desde otras partes del proyecto mediante:

```csharp
using Network;
```

---

# 2. ¿Cómo funciona la comunicación?

La comunicación sigue este proceso:

```text
CLIENTE                         SERVIDOR
   |                               |
   |------ Conectar() ------------>|
   |                               |
   |------ Mensaje ---------------->|
   |                               |
   |<----- Mensaje -----------------|
   |                               |
   |------ Mensaje ---------------->|
   |                               |
   |------ Cerrar() -------------->|
```

La conexión utiliza:

* **TCP:** protocolo utilizado para la comunicación.
* **IP:** dirección del servidor.
* **Puerto:** puerta utilizada para establecer la conexión.
* **NetworkStream:** canal por donde se envían y reciben los datos.
* **UTF-8:** codificación utilizada para convertir texto a bytes.
* **`\n`:** indica el final de cada mensaje.

---

# 3. Requisito: clase `Mensaje`

`ClienteTcp` depende de una clase llamada `Mensaje`.

Esta clase debe tener, al menos, los métodos:

```csharp
string Serializar()
```

y

```csharp
static Mensaje? Deserializar(string texto)
```

La idea es convertir un objeto `Mensaje` en texto para enviarlo por TCP y luego convertir ese texto nuevamente en un objeto.

Por ejemplo:

```text
Objeto Mensaje
      ↓
Serializar()
      ↓
Texto
      ↓
UTF-8
      ↓
Bytes
      ↓
TCP
      ↓
Servidor
```

Cuando se recibe:

```text
TCP
 ↓
Bytes
 ↓
Texto
 ↓
Deserializar()
 ↓
Objeto Mensaje
```

---

# 4. Crear una instancia del cliente

Para utilizar `ClienteTcp`, primero se crea un objeto:

```csharp
ClienteTcp cliente = new ClienteTcp();
```

A partir de ese objeto se pueden utilizar sus métodos:

```csharp
cliente.Conectar(...);
cliente.Enviar(...);
cliente.Cerrar();
```

---

# 5. Conectarse al servidor

La conexión se realiza mediante:

```csharp
cliente.Conectar(ip, puerto);
```

Por ejemplo:

```csharp
bool conectado = cliente.Conectar("127.0.0.1", 5000);
```

Los parámetros son:

| Parámetro | Descripción                      |
| --------- | -------------------------------- |
| `ip`      | Dirección IP del servidor        |
| `puerto`  | Puerto donde escucha el servidor |

### `127.0.0.1`

Esta dirección significa **la misma computadora**.

Por ejemplo, si el servidor y el cliente están ejecutándose en la misma computadora:

```csharp
cliente.Conectar("127.0.0.1", 5000);
```

Si el servidor está en otra computadora de la red, se utiliza su dirección IP:

```csharp
cliente.Conectar("192.168.1.100", 5000);
```

---

# 6. ¿Qué devuelve `Conectar()`?

El método devuelve un `bool`.

```csharp
bool conectado = cliente.Conectar("127.0.0.1", 5000);
```

Si la conexión funciona:

```csharp
conectado == true
```

Si ocurre algún error:

```csharp
conectado == false
```

Por ejemplo:

```csharp
if (cliente.Conectar("127.0.0.1", 5000))
{
    Console.WriteLine("Conectado al servidor");
}
else
{
    Console.WriteLine("No se pudo conectar");
}
```

---

# 7. ¿Qué ocurre internamente al conectar?

Cuando se ejecuta:

```csharp
socket.Connect(ip, puerto);
```

se intenta establecer una conexión TCP con el servidor.

Después:

```csharp
stream = socket.GetStream();
```

obtiene el `NetworkStream`.

Este `NetworkStream` será utilizado para enviar y recibir información.

Finalmente:

```csharp
activo = true;
```

indica que el cliente está conectado y funcionando.

---

# 8. Hilo para escuchar mensajes

Después de conectarse, el cliente crea un hilo:

```csharp
Thread hilo = new Thread(EscucharServidor);
hilo.IsBackground = true;
hilo.Start();
```

Este hilo ejecuta:

```csharp
EscucharServidor()
```

Su función es quedarse esperando mensajes del servidor.

Esto es importante porque el programa principal **no queda bloqueado esperando mensajes**.

Conceptualmente:

```text
Hilo principal
      |
      |-- Ejecuta el programa normalmente
      |
      +----------------------------

Hilo secundario
      |
      |-- Escucha mensajes del servidor
      |-- Procesa mensajes
      |-- Ejecuta eventos
```

---

# 9. Enviar un mensaje

Para enviar un mensaje se utiliza:

```csharp
cliente.Enviar(mensaje);
```

Por ejemplo:

```csharp
Mensaje mensaje = new Mensaje(...);

cliente.Enviar(mensaje);
```

El método `Enviar()` realiza varios pasos.

### Paso 1: comprobar la conexión

```csharp
if (stream == null)
{
    return;
}
```

Si no existe un `NetworkStream`, no se envía nada.

### Paso 2: serializar el mensaje

```csharp
string textoSerializado = mensaje.Serializar() + "\n";
```

El objeto `Mensaje` se convierte en texto.

Se agrega:

```text
\n
```

al final.

Esto permite saber dónde termina un mensaje y comienza el siguiente.

Por ejemplo:

```text
{"tipo":"CONECTAR"}\n
```

### Paso 3: convertir a bytes

```csharp
byte[] bytes = Encoding.UTF8.GetBytes(textoSerializado);
```

TCP trabaja con bytes, por lo que el texto debe convertirse.

### Paso 4: enviar

```csharp
stream.Write(bytes, 0, bytes.Length);
```

Los bytes son enviados al servidor.

---

# 10. Recepción de mensajes

La recepción se realiza automáticamente mediante:

```csharp
EscucharServidor()
```

Este método se ejecuta en el hilo secundario.

Primero crea un buffer:

```csharp
byte[] buffer = new byte[4096];
```

Este espacio se utiliza para almacenar temporalmente los datos recibidos.

También crea:

```csharp
StringBuilder acumulado = new StringBuilder();
```

Este objeto almacena los datos que todavía no han formado un mensaje completo.

---

# 11. ¿Por qué existe `acumulado`?

TCP no garantiza que un mensaje llegue completo en una sola lectura.

Por ejemplo, podríamos enviar:

```text
MENSAJE_COMPLETO\n
```

pero recibir:

```text
MENSAJE_CO
```

y después:

```text
MPLETO\n
```

Por eso se utiliza:

```csharp
acumulado
```

para juntar las partes.

El resultado sería:

```text
Primera recepción:
" MENSAJE_CO"

Segunda recepción:
"MPLETO\n"

Acumulado:
" MENSAJE_COMPLETO\n"
```

Cuando encuentra `\n`, significa que ya tiene un mensaje completo.

---

# 12. Separar los mensajes

El código busca:

```csharp
IndexOf('\n')
```

Esto permite encontrar el final de un mensaje.

Por ejemplo:

```text
Mensaje1\nMensaje2\nMensaje3\n
```

El programa procesa primero:

```text
Mensaje1
```

Después:

```text
Mensaje2
```

Y finalmente:

```text
Mensaje3
```

Esto permite recibir varios mensajes aunque lleguen juntos.

---

# 13. Deserializar el mensaje

Una vez obtenida una línea completa:

```csharp
string linea = ...
```

se utiliza:

```csharp
Mensaje? mensaje = Mensaje.Deserializar(linea);
```

Esto convierte el texto nuevamente en un objeto `Mensaje`.

Por ejemplo:

```text
Texto recibido
      ↓
Deserializar()
      ↓
Mensaje
```

Si la deserialización funciona:

```csharp
if (mensaje != null)
```

el mensaje puede ser procesado.

---

# 14. Evento `MensajeRecibido`

La clase tiene este evento:

```csharp
public event Action<Mensaje>? MensajeRecibido;
```

Este evento permite que otras clases sepan cuándo llegó un mensaje.

Por ejemplo:

```csharp
cliente.MensajeRecibido += mensaje =>
{
    Console.WriteLine("Mensaje recibido");
};
```

Cuando `ClienteTcp` recibe un mensaje, ejecuta:

```csharp
MensajeRecibido.Invoke(mensaje);
```

Esto notifica a todos los métodos que estén suscritos al evento.

---

# 15. Ejemplo de recepción

Una forma sencilla de utilizarlo:

```csharp
ClienteTcp cliente = new ClienteTcp();

cliente.MensajeRecibido += mensaje =>
{
    Console.WriteLine("Llegó un mensaje del servidor");

    // Procesar mensaje aquí
};

cliente.Conectar("127.0.0.1", 5000);
```

A partir de ese momento, cada vez que el servidor envíe un mensaje, se ejecutará automáticamente el código dentro de:

```csharp
cliente.MensajeRecibido += ...
```

---

# 16. Evento `Desconectado`

También existe:

```csharp
public event Action? Desconectado;
```

Este evento avisa cuando se pierde la conexión con el servidor.

Se puede utilizar:

```csharp
cliente.Desconectado += () =>
{
    Console.WriteLine("El servidor se desconectó");
};
```

Cuando `EscucharServidor()` detecta que la conexión terminó, ejecuta:

```csharp
Desconectado.Invoke();
```

---

# 17. ¿Cómo detecta una desconexión?

El método utiliza:

```csharp
leidos = stream!.Read(buffer, 0, buffer.Length);
```

Si `Read()` devuelve:

```csharp
0
```

significa que el otro extremo cerró la conexión.

Por eso se utiliza:

```csharp
if (leidos == 0)
{
    break;
}
```

También puede ocurrir una excepción mientras se está leyendo. En ese caso, el método también sale del ciclo.

---

# 18. Cerrar el cliente

Para cerrar la conexión manualmente:

```csharp
cliente.Cerrar();
```

El método realiza:

```csharp
activo = false;
```

para indicar que el cliente ya no debe seguir funcionando.

Después cierra el stream:

```csharp
stream.Close();
```

y finalmente el socket:

```csharp
socket.Close();
```

---

# 19. Ejemplo completo de uso

Un ejemplo básico sería:

```csharp
using System;
using Network;

class Program
{
    static void Main()
    {
        ClienteTcp cliente = new ClienteTcp();

        // Evento para recibir mensajes
        cliente.MensajeRecibido += mensaje =>
        {
            Console.WriteLine("Mensaje recibido del servidor");
            
            // Procesar mensaje
        };

        // Evento para detectar desconexión
        cliente.Desconectado += () =>
        {
            Console.WriteLine("Servidor desconectado");
        };

        // Conectarse
        bool conectado = cliente.Conectar("127.0.0.1", 5000);

        if (!conectado)
        {
            Console.WriteLine("No se pudo conectar al servidor");
            return;
        }

        Console.WriteLine("Conectado al servidor");

        // Crear y enviar un mensaje
        // Mensaje mensaje = new Mensaje(...);
        // cliente.Enviar(mensaje);

        Console.ReadLine();

        // Cerrar conexión
        cliente.Cerrar();
    }
}
```

---

# 20. ¿Cómo crear el servidor?

`ClienteTcp` **no crea el servidor**.

El servidor debe ser otro programa que utilice un `TcpListener`.

Conceptualmente, el servidor debe:

1. Elegir un puerto.
2. Iniciar un `TcpListener`.
3. Esperar clientes.
4. Aceptar la conexión.
5. Obtener el `NetworkStream`.
6. Recibir mensajes.
7. Procesar los mensajes.
8. Enviar respuestas.

Por ejemplo, un servidor TCP básico puede comenzar así:

```csharp
TcpListener servidor = new TcpListener(
    IPAddress.Any,
    5000
);

servidor.Start();

Console.WriteLine("Servidor iniciado");

TcpClient cliente = servidor.AcceptTcpClient();

Console.WriteLine("Cliente conectado");
```

El puerto debe coincidir con el puerto utilizado por el cliente.

Si el servidor utiliza:

```csharp
5000
```

el cliente debe conectarse utilizando:

```csharp
cliente.Conectar("127.0.0.1", 5000);
```

---

# 21. Servidor y cliente

La relación sería:

```text
                 SERVIDOR
             Puerto 5000
                  |
                  |
             TCP Listener
                  |
                  |
            Espera clientes
                  |
        +---------+---------+
        |                   |
        ↓                   ↓
    Cliente 1           Cliente 2
    127.0.0.1           127.0.0.1
```

Si están en computadoras diferentes:

```text
COMPUTADORA A                 COMPUTADORA B
     |                              |
   CLIENTE                      SERVIDOR
     |                              |
     +------ TCP / Puerto 5000 ----+
```

En ese caso, el cliente debe utilizar la IP de la computadora donde está ejecutándose el servidor.

---

# 22. Resumen de las partes principales

| Parte                | Función                             |
| -------------------- | ----------------------------------- |
| `TcpClient`          | Representa la conexión TCP          |
| `NetworkStream`      | Permite enviar y recibir datos      |
| `Conectar()`         | Conecta el cliente al servidor      |
| `Enviar()`           | Envía un `Mensaje`                  |
| `EscucharServidor()` | Recibe y procesa mensajes           |
| `Cerrar()`           | Cierra la conexión                  |
| `Thread`             | Permite escuchar en segundo plano   |
| `MensajeRecibido`    | Evento cuando llega un mensaje      |
| `Desconectado`       | Evento cuando se pierde la conexión |
| `Serializar()`       | Convierte `Mensaje` en texto        |
| `Deserializar()`     | Convierte texto en `Mensaje`        |
| `Encoding.UTF8`      | Convierte texto a bytes y viceversa |
| `StringBuilder`      | Acumula datos recibidos             |
| `\n`                 | Marca el final de cada mensaje      |

---

# 23. Flujo completo

El funcionamiento general de `ClienteTcp` puede resumirse así:

```text
1. Crear ClienteTcp
        ↓
2. Conectar(IP, puerto)
        ↓
3. Crear NetworkStream
        ↓
4. Iniciar hilo de escucha
        ↓
5. Enviar mensajes con Enviar()
        ↓
6. Servidor responde
        ↓
7. EscucharServidor() recibe bytes
        ↓
8. Convertir bytes → texto
        ↓
9. Buscar '\n'
        ↓
10. Obtener mensaje completo
        ↓
11. Deserializar()
        ↓
12. Ejecutar MensajeRecibido
        ↓
13. Si se pierde conexión → Desconectado
        ↓
14. Cerrar()
```

---

# 24. Importante

Cada mensaje enviado debe terminar con:

```text
\n
```

Esto es fundamental para que `EscucharServidor()` pueda identificar dónde termina cada mensaje.

Por ejemplo:

```text
Mensaje1\n
Mensaje2\n
Mensaje3\n
```

No se recomienda enviar mensajes sin el salto de línea porque el receptor podría no saber cuándo termina un mensaje.

Además, tanto cliente como servidor deben utilizar el mismo formato de serialización para que puedan entender los objetos `Mensaje`.

---

# 25. Resumen rápido de uso

El uso básico de la clase es:

```csharp
// 1. Crear cliente
ClienteTcp cliente = new ClienteTcp();

// 2. Configurar eventos
cliente.MensajeRecibido += mensaje =>
{
    // Procesar mensaje
};

cliente.Desconectado += () =>
{
    // Manejar desconexión
};

// 3. Conectarse
cliente.Conectar("127.0.0.1", 5000);

// 4. Enviar mensajes
cliente.Enviar(mensaje);

// 5. Cerrar cuando termine
cliente.Cerrar();
```

De esta manera, `ClienteTcp` se encarga de toda la parte de **conexión TCP, envío, recepción, separación de mensajes, deserialización y notificación mediante eventos**, mientras que el resto del programa se puede concentrar en la lógica del juego o aplicación.

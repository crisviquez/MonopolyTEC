# Guía de uso del Cliente TCP

## 1. ¿Qué es `Cliente`?

La clase `Cliente` es la encargada de manejar la comunicación entre el jugador y el servidor de Monopoly mediante TCP.

Su función principal es:

* Conectarse al servidor.
* Enviar acciones del jugador.
* Recibir mensajes del servidor.
* Mantener una copia local del estado de los jugadores.
* Avisar al programa cuando ocurre un evento, error o cambio de estado.

La clase utiliza `ClienteTcp`, perteneciente al proyecto `Network`, para realizar la comunicación por TCP.

---

## 2. Crear un cliente

Para utilizar la clase solamente se necesita crear un objeto:

```csharp
Cliente cliente = new Cliente();
```

Internamente, el constructor crea la conexión de red y la estructura donde se almacenan los jugadores:

```csharp
red = new ClienteTcp();
jugadores = new ListaSimple<JugadorEstado>();
```

`Cliente` utiliza `ListaSimple<JugadorEstado>` para mantener su propia copia local de los jugadores recibida desde el servidor.

---

## 3. Conectarse al servidor

La conexión se realiza mediante:

```csharp
bool conectado = cliente.Conectar("127.0.0.1", 5000, "Cristopher");
```

Los parámetros son:

| Parámetro | Descripción                      |
| --------- | -------------------------------- |
| `ip`      | Dirección IP del servidor        |
| `puerto`  | Puerto donde escucha el servidor |
| `nombre`  | Nombre del jugador               |

Por ejemplo:

```csharp
cliente.Conectar("127.0.0.1", 5000, "Jugador1");
```

Al conectarse, el cliente envía automáticamente un mensaje `CONECTAR` con el nombre del jugador.

El servidor posteriormente puede aceptar o rechazar la conexión.

---

## 4. Recibir información del servidor

El cliente utiliza eventos para avisar al programa principal cuando ocurre algo.

### `OnEvento`

Se utiliza para eventos del juego:

```csharp
cliente.OnEvento += MostrarEvento;
```

Por ejemplo, puede recibir:

```text
[EVENTO] Jugador 2 saco 4 y 5
```

### `OnError`

Se utiliza para errores:

```csharp
cliente.OnError += MostrarError;
```

Por ejemplo:

```text
[ERROR] No puedes comprar esta propiedad
```

### `OnEstadoActualizado`

Se ejecuta cuando cambia el estado del juego:

```csharp
cliente.OnEstadoActualizado += MostrarTablero;
```

Esto permite actualizar la interfaz o mostrar nuevamente el estado del tablero.

---

## 5. Acciones del jugador

La clase proporciona métodos sencillos para enviar las acciones al servidor:

```csharp
cliente.TirarDado();
cliente.ComprarPropiedad();
cliente.PagarDeuda();
cliente.TerminarTurno();
```

Cada método crea un `Mensaje` con el tipo correspondiente y lo envía mediante `ClienteTcp`.

Por ejemplo:

```csharp
public void TirarDado()
{
    Mensaje mensaje = new Mensaje(TipoMensaje.TIRAR_DADO);
    red.Enviar(mensaje);
}
```

El `Cliente` **no realiza directamente la lógica del juego**. Solamente informa al servidor de la acción que el jugador quiere realizar.

---

## 6. Saber si es el turno del jugador

Se puede comprobar mediante:

```csharp
if (cliente.EsMiTurno())
{
    cliente.TirarDado();
}
```

El servidor indica qué jugador tiene el turno mediante `TU_TURNO`.

El cliente guarda ese ID en:

```csharp
turnoActualId
```

y lo compara con:

```csharp
idPropio
```

Por eso `EsMiTurno()` devuelve `true` cuando ambos IDs coinciden.

---

## 7. Obtener información de los jugadores

La información local de los jugadores se puede obtener con:

```csharp
ListaSimple<JugadorEstado> jugadores = cliente.ObtenerJugadores();
```

Por ejemplo:

```csharp
foreach (JugadorEstado jugador in cliente.ObtenerJugadores())
{
    Console.WriteLine(jugador.Nombre);
    Console.WriteLine(jugador.Posicion);
    Console.WriteLine(jugador.Saldo);
}
```

También se puede obtener el ID del jugador actual:

```csharp
int id = cliente.ObtenerIdPropio();
```

---

## 8. Actualización del estado

Cuando el servidor envía `ACTUALIZAR_ESTADO`, el cliente recibe una lista de jugadores y actualiza su copia local.

El método:

```csharp
ActualizarJugadores(...)
```

busca cada jugador por su ID.

* Si el jugador ya existe, actualiza sus datos.
* Si no existe, lo agrega a `ListaSimple`.

Esto permite que cada cliente tenga una representación local del estado actual del juego.

---

## 9. Desconectarse

Para salir correctamente del juego:

```csharp
cliente.Desconectar();
```

Esto envía:

```csharp
TipoMensaje.DESCONECTAR
```

al servidor y posteriormente cierra la conexión TCP.

---

# 10. Ejemplo básico de uso

`Program.cs` muestra una posible forma de utilizar `Cliente`:

```csharp
Cliente cliente = new Cliente();

cliente.OnError += MostrarError;
cliente.OnEvento += MostrarEvento;
cliente.OnEstadoActualizado += MostrarTablero;

bool conectado = cliente.Conectar(
    "127.0.0.1",
    5000,
    "Jugador1"
);

if (conectado)
{
    if (cliente.EsMiTurno())
    {
        cliente.TirarDado();
    }
}
```

En la versión final del proyecto, `Program.cs` puede cambiar completamente. Lo importante es que la interfaz, menú o juego utilice los métodos y eventos proporcionados por `Cliente`.

---

# 11. Flujo general

El funcionamiento puede resumirse así:

```text
Program / Interfaz
       |
       v
     Cliente
       |
       v
   ClienteTcp
       |
       v
     Servidor
       |
       v
   ClienteTcp
       |
       v
     Cliente
       |
       v
Program / Interfaz
```

**En resumen:** `Cliente` funciona como una capa intermedia entre la interfaz del juego y la comunicación TCP. El programa indica qué quiere hacer el jugador mediante los métodos de `Cliente`, mientras que `Cliente` recibe y procesa las respuestas enviadas por el servidor.

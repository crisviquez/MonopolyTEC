using System.Collections.Generic;
using System.Text.Json;

namespace Network
{
    public enum TipoMensaje
    {
        // Cliente -> Servidor
        CONECTAR,
        TIRAR_DADO,
        COMPRAR_PROPIEDAD,
        NO_COMPRAR,
        PAGAR_DEUDA,
        TERMINAR_TURNO,
        CONSULTAR_TRANSACCIONES,
        DESCONECTAR,

        // Servidor -> Cliente
        CONEXION_ACEPTADA,
        CONEXION_RECHAZADA,
        ACTUALIZAR_ESTADO,
        TU_TURNO,
        RESULTADO_DADO,
        OFERTA_COMPRA,
        HISTORIAL_TRANSACCIONES,
        EVENTO,
        ERROR,
        FIN_JUEGO
    }

    public class Mensaje
    {
        public TipoMensaje Tipo { get; set; }
        public object? Datos { get; set; }

        public Mensaje()
        {
            // Constructor vacio para que se pueda deserializar
        }

        public Mensaje(TipoMensaje tipo, object? datos = null)
        {
            Tipo = tipo;
            Datos = datos;
        }

        // Convierte esta instancia a un string JSON
        public string Serializar()
        {
            return JsonSerializer.Serialize(this);
        }

        // Convierte un JSON de vuelta a un objeto Mensaje
        public static Mensaje? Deserializar(string json)
        {
            return JsonSerializer.Deserialize<Mensaje>(json);
        }

        // Extrae el contenido de Datos y lo convierte al tipo (T) que se pida
        public T LeerDatos<T>()
        {
            if (Datos == null)
            {
                return default!;
            }

            string jsonDatos = JsonSerializer.Serialize(Datos);
            return JsonSerializer.Deserialize<T>(jsonDatos)!;
        }
    }

    // -- Payloads Cliente -> Servidor --
    public class DatosConectar
    {
        public string Nombre { get; set; } = "";
    }

    // -- Payloads Servidor -> Cliente --
    public class JugadorEstado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int Posicion { get; set; }
        public int Saldo { get; set; }
        public List<int> Propiedades { get; set; } = new List<int>();
        public bool EnBancarrota { get; set; }
    }

    public class DatosConexionAceptada
    {
        public int IdJugador { get; set; }
        public int NumeroCasillas { get; set; }
        public List<JugadorEstado> Jugadores { get; set; } = new List<JugadorEstado>();
    }

    public class DatosConexionRechazada
    {
        public string Motivo { get; set; } = "";
    }

    public class DatosActualizarEstado
    {
        public List<JugadorEstado> Jugadores { get; set; } = new List<JugadorEstado>();
    }

    public class DatosTuTurno
    {
        public int IdJugador { get; set; }
    }

    public class DatosResultadoDado
    {
        public int IdJugador { get; set; }
        public int Dado1 { get; set; }
        public int Dado2 { get; set; }
    }

    // El servidor avisa que el jugador cayo en una propiedad libre y puede comprarla
    public class DatosOfertaCompra
    {
        public int IdCasilla { get; set; }
        public string NombrePropiedad { get; set; } = "";
        public int Precio { get; set; }
    }

    // Un registro de transaccion tal como viaja por la red (no es la clase Transaccion del servidor)
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

    public class DatosHistorialTransacciones
    {
        public List<TransaccionInfo> Transacciones { get; set; } = new List<TransaccionInfo>();
    }

    public class DatosEvento
    {
        public int IdJugador { get; set; }
        public string Descripcion { get; set; } = "";
    }

    public class DatosError
    {
        public string Mensaje { get; set; } = "";
    }

    public class DatosFinJuego
    {
        public int IdGanador { get; set; }
    }
}
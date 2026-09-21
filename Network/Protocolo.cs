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
        PAGAR_DEUDA,
        TERMINAR_TURNO,
        DESCONECTAR,

        // Servidor -> Cliente
        CONEXION_ACEPTADA,
        CONEXION_RECHAZADA,
        ACTUALIZAR_ESTADO,
        TU_TURNO,
        RESULTADO_DADO,
        EVENTO,
        ERROR,
        FIN_JUEGO
    }

    public class Mensaje
    {
        public TipoMensaje Tipo {get; set;}
        public object? Datos {get; set;}

        public Mensaje()
        {
            // Contructor vacio para q se pueda deserializar
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

        // Convierte un JSON devuelta a un objeto Mensaje
        public static Mensaje? Deserializar(string json) 
        {
            JsonSerializer.Deserialize<Mensaje>(json);
        }

        // Extrae contenido de Datos y lo convierte a la clase/tipo (T)
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

    // -- Payloads Cliente -> Servidro --
    public class DatosConectar
    {
        public string Nombre {get; set;} = "";
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
        // Agergar mas si es necesario
    }

    public class DatosConexionAceptada
    {
        public int IdJugador { get; set; }
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
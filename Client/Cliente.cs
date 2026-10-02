using Monopoly.Estructuras;
using Network;

namespace Client
{
    public class Cliente
    {
        private ClienteTcp red;
        private ListaSimple<JugadorEstado> jugadores;
        private ListaSimple<TransaccionInfo> historial;
        private int idPropio;
        private int turnoActualId;
        private int numeroCasillas;
        private bool conectadoAlServidor;

        public event Action<string>? OnEvento;
        public event Action? OnEstadoActualizado;
        public event Action<string>? OnError;
        public event Action<DatosOfertaCompra>? OnOfertaCompra;
        public event Action? OnHistorial;
        public event Action<DatosResultadoDado>? OnResultadoDado;
        public event Action<int>? OnFinJuego;

        public Cliente()
        {
            red = new ClienteTcp();
            jugadores = new ListaSimple<JugadorEstado>();
            historial = new ListaSimple<TransaccionInfo>();
            idPropio = 0;
            turnoActualId = 0;
            numeroCasillas = 40; // valor por defecto mientras el servidor no confirme el real
            conectadoAlServidor = false;
        }

        public bool EsMiTurno()
        {
            if (idPropio == 0)
            {
                return false;
            }
            if (turnoActualId == idPropio)
            {
                return true;
            }
            return false;
        }

        // El servidor manda el primer TU_TURNO al iniciar la partida
        public bool PartidaIniciada()
        {
            if (turnoActualId != 0)
            {
                return true;
            }
            return false;
        }

        public ListaSimple<JugadorEstado> ObtenerJugadores()
        {
            return jugadores;
        }

        public ListaSimple<TransaccionInfo> ObtenerHistorial()
        {
            return historial;
        }

        public int ObtenerIdPropio()
        {
            return idPropio;
        }

        public int ObtenerNumeroCasillas()
        {
            return numeroCasillas;
        }

        public int ObtenerTurnoActualId()
        {
            return turnoActualId;
        }

        public bool EstaConectado()
        {
            return conectadoAlServidor;
        }

        private void ManejarDesconexion()
        {
            if (conectadoAlServidor == true)
            {
                conectadoAlServidor = false;
                if (OnError != null)
                {
                    OnError("Se perdio la conexion con el servidor");
                }
            }
        }

        public bool Conectar(string ip, int puerto, string nombre)
        {
            red.MensajeRecibido += ManejarMensaje;
            red.Desconectado += ManejarDesconexion;

            bool exito = red.Conectar(ip, puerto);
            if (exito == false)
            {
                return false;
            }

            conectadoAlServidor = true;

            DatosConectar datos = new DatosConectar();
            datos.Nombre = nombre;

            Mensaje mensaje = new Mensaje(TipoMensaje.CONECTAR, datos);
            red.Enviar(mensaje);

            return true;
        }

        private void ManejarMensaje(Mensaje mensaje)
        {
            if (mensaje.Tipo == TipoMensaje.CONEXION_ACEPTADA)
            {
                DatosConexionAceptada datos = mensaje.LeerDatos<DatosConexionAceptada>();
                idPropio = datos.IdJugador;
                if (datos.NumeroCasillas > 0)
                {
                    numeroCasillas = datos.NumeroCasillas;
                }
                ActualizarJugadores(datos.Jugadores);
            }
            else if (mensaje.Tipo == TipoMensaje.CONEXION_RECHAZADA)
            {
                DatosConexionRechazada datos = mensaje.LeerDatos<DatosConexionRechazada>();
                if (OnError != null)
                {
                    OnError(datos.Motivo);
                }
            }
            else if (mensaje.Tipo == TipoMensaje.ACTUALIZAR_ESTADO)
            {
                DatosActualizarEstado datos = mensaje.LeerDatos<DatosActualizarEstado>();
                ActualizarJugadores(datos.Jugadores);
            }
            else if (mensaje.Tipo == TipoMensaje.TU_TURNO)
            {
                DatosTuTurno datos = mensaje.LeerDatos<DatosTuTurno>();
                turnoActualId = datos.IdJugador;
                if (OnEstadoActualizado != null)
                {
                    OnEstadoActualizado();
                }
            }
            else if (mensaje.Tipo == TipoMensaje.RESULTADO_DADO)
            {
                DatosResultadoDado datos = mensaje.LeerDatos<DatosResultadoDado>();
                string texto = "Jugador " + datos.IdJugador + " saco " + datos.Dado1 + " y " + datos.Dado2;
                if (OnEvento != null)
                {
                    OnEvento(texto);
                }
                if (OnResultadoDado != null)
                {
                    OnResultadoDado(datos);
                }
            }
            else if (mensaje.Tipo == TipoMensaje.OFERTA_COMPRA)
            {
                DatosOfertaCompra datos = mensaje.LeerDatos<DatosOfertaCompra>();
                if (OnOfertaCompra != null)
                {
                    OnOfertaCompra(datos);
                }
            }
            else if (mensaje.Tipo == TipoMensaje.HISTORIAL_TRANSACCIONES)
            {
                DatosHistorialTransacciones datos = mensaje.LeerDatos<DatosHistorialTransacciones>();
                ActualizarHistorial(datos.Transacciones);
                if (OnHistorial != null)
                {
                    OnHistorial();
                }
            }
            else if (mensaje.Tipo == TipoMensaje.EVENTO)
            {
                DatosEvento datos = mensaje.LeerDatos<DatosEvento>();
                if (OnEvento != null)
                {
                    OnEvento(datos.Descripcion);
                }
            }
            else if (mensaje.Tipo == TipoMensaje.ERROR)
            {
                DatosError datos = mensaje.LeerDatos<DatosError>();
                if (OnError != null)
                {
                    OnError(datos.Mensaje);
                }
            }
            else if (mensaje.Tipo == TipoMensaje.FIN_JUEGO)
            {
                DatosFinJuego datos = mensaje.LeerDatos<DatosFinJuego>();
                string texto = "Gano el jugador " + datos.IdGanador;
                if (OnEvento != null)
                {
                    OnEvento(texto);
                }
                if (OnFinJuego != null)
                {
                    OnFinJuego(datos.IdGanador);
                }
            }
        }

        // Recibe la lista de jugadores que manda el servidor y actualiza
        // nuestra copia local (guardada en la estructura ListaSimple)
        private void ActualizarJugadores(List<JugadorEstado> listaNueva)
        {
            jugadores = new ListaSimple<JugadorEstado>();
            for (int i = 0; i < listaNueva.Count; i++)
            {
                jugadores.Agregar(listaNueva[i]);
            }

            if (OnEstadoActualizado != null)
            {
                OnEstadoActualizado();
            }
        }

        // Reemplaza el historial local con la lista recibida del servidor
        private void ActualizarHistorial(List<TransaccionInfo> listaNueva)
        {
            historial = new ListaSimple<TransaccionInfo>();
            for (int i = 0; i < listaNueva.Count; i++)
            {
                historial.Agregar(listaNueva[i]);
            }
        }

        // Busca un jugador por Id recorriendo la lista uno por uno
        private JugadorEstado? BuscarJugador(int id)
        {
            foreach (JugadorEstado j in jugadores)
            {
                if (j.Id == id)
                {
                    return j;
                }
            }
            return null;
        }

        // Los valores vienen de los dados fisicos; el servidor los valida
        public void TirarDado(int dado1, int dado2)
        {
            DatosTirarDado datos = new DatosTirarDado();
            datos.Dado1 = dado1;
            datos.Dado2 = dado2;

            Mensaje mensaje = new Mensaje(TipoMensaje.TIRAR_DADO, datos);
            red.Enviar(mensaje);
        }

        public void ComprarPropiedad()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.COMPRAR_PROPIEDAD);
            red.Enviar(mensaje);
        }

        public void NoComprar()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.NO_COMPRAR);
            red.Enviar(mensaje);
        }

        public void PagarDeuda()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.PAGAR_DEUDA);
            red.Enviar(mensaje);
        }

        public void RegistrarTarjeta(string idTarjeta, string nombreJugador)
        {
            DatosTarjeta datos = new DatosTarjeta();
            datos.IdTarjeta = idTarjeta;
            datos.NombreJugador = nombreJugador;

            Mensaje mensaje = new Mensaje(TipoMensaje.REGISTRAR_TARJETA, datos);
            red.Enviar(mensaje);
        }

        // El servidor identifica al jugador por la tarjeta y paga su deuda
        public void UsarTarjeta(string idTarjeta)
        {
            DatosTarjeta datos = new DatosTarjeta();
            datos.IdTarjeta = idTarjeta;

            Mensaje mensaje = new Mensaje(TipoMensaje.TARJETA_RFID, datos);
            red.Enviar(mensaje);
        }

        public void TerminarTurno()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.TERMINAR_TURNO);
            red.Enviar(mensaje);
        }

        public void ConsultarHistorial()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.CONSULTAR_TRANSACCIONES);
            red.Enviar(mensaje);
        }

        public void Desconectar()
        {
            conectadoAlServidor = false;
            Mensaje mensaje = new Mensaje(TipoMensaje.DESCONECTAR);
            red.Enviar(mensaje);
            red.Cerrar();
        }
    }
}

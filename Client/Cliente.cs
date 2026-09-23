using System;
using System.Collections.Generic;
using Monopoly.Estructuras;
using Network;

namespace Client
{
    public class Cliente
    {
        private ClienteTcp red;
        private ListaSimple<JugadorEstado> jugadores;
        private int idPropio;
        private int turnoActualId;

        public event Action<string> OnEvento;
        public event Action OnEstadoActualizado;
        public event Action<string> OnError;

        public Cliente()
        {
            red = new ClienteTcp();
            jugadores = new ListaSimple<JugadorEstado>();
            idPropio = 0;
            turnoActualId = 0;
        }

        public bool EsMiTurno()
        {
            if (turnoActualId == idPropio)
            {
                return true;
            }
            return false;
        }

        public ListaSimple<JugadorEstado> ObtenerJugadores()
        {
            return jugadores;
        }

        public int ObtenerIdPropio()
        {
            return idPropio;
        }

        public bool Conectar(string ip, int puerto, string nombre)
        {
            red.MensajeRecibido += ManejarMensaje;

            bool conectado = red.Conectar(ip, puerto);
            if (conectado == false)
            {
                return false;
            }

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
            }
        }

        // Recibe la lista de jugadores que manda el servidor y actualiza
        // nuestra copia local (guardada en la estructura ListaSimple)
        private void ActualizarJugadores(List<JugadorEstado> listaNueva)
        {
            for (int i = 0; i < listaNueva.Count; i++)
            {
                JugadorEstado nuevo = listaNueva[i];
                JugadorEstado existente = BuscarJugador(nuevo.Id);

                if (existente != null)
                {
                    existente.Nombre = nuevo.Nombre;
                    existente.Posicion = nuevo.Posicion;
                    existente.Saldo = nuevo.Saldo;
                    existente.Propiedades = nuevo.Propiedades;
                    existente.EnBancarrota = nuevo.EnBancarrota;
                }
                else
                {
                    jugadores.Agregar(nuevo);
                }
            }

            if (OnEstadoActualizado != null)
            {
                OnEstadoActualizado();
            }
        }

        // Busca un jugador por Id recorriendo la lista uno por uno
        private JugadorEstado BuscarJugador(int id)
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

        public void TirarDado()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.TIRAR_DADO);
            red.Enviar(mensaje);
        }

        public void ComprarPropiedad()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.COMPRAR_PROPIEDAD);
            red.Enviar(mensaje);
        }

        public void PagarDeuda()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.PAGAR_DEUDA);
            red.Enviar(mensaje);
        }

        public void TerminarTurno()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.TERMINAR_TURNO);
            red.Enviar(mensaje);
        }

        public void Desconectar()
        {
            Mensaje mensaje = new Mensaje(TipoMensaje.DESCONECTAR);
            red.Enviar(mensaje);
            red.Cerrar();
        }
    }
}
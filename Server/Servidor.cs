using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Monopoly.Estructuras;
using Network;

namespace Server
{
    public class Servidor
    {
        private const string RUTA_TXT = "transacciones_partida.txt";

        private TcpListener listener;
        private Juego juego;
        private ListaSimple<ClienteConectado> clientes;
        private object candado;
        private bool activo;
        private bool finEnviado;
        private int idTurnoNotificado;

        public Servidor(int puerto, int maxTurnos)
        {
            listener = new TcpListener(IPAddress.Any, puerto);
            juego = new Juego(Juego.MINIMO_CASILLAS, maxTurnos);
            clientes = new ListaSimple<ClienteConectado>();
            candado = new object();
            activo = false;
            finEnviado = false;
            idTurnoNotificado = 0;
        }

        public void Iniciar()
        {
            listener.Start();
            activo = true;

            Thread hilo = new Thread(AceptarClientes);
            hilo.IsBackground = true;
            hilo.Start();
        }

        public void Detener()
        {
            activo = false;

            lock (candado)
            {
                if (juego.HaIniciado() == true && finEnviado == false)
                {
                    ExportarHistorial();
                }

                foreach (ClienteConectado c in clientes)
                {
                    c.Cerrar();
                }
            }

            listener.Stop();
        }

        public bool HaIniciado()
        {
            lock (candado)
            {
                return juego.HaIniciado();
            }
        }

        // Lo llama el organizador desde la consola (o solo cuando se conectan 4)
                public bool IniciarPartida()
        {
            lock (candado)
            {
                string error = juego.Iniciar();
                if (error != "")
                {
                    Console.WriteLine(error);
                    juego.AgregarEvento(error);
                    PublicarCambios(null);
                    return false;
                }

                Console.WriteLine("Partida iniciada");
                PublicarCambios(null);
                return true;
            }
        }

        public void ExportarHistorial()
        {
            bool exportado = juego.ObtenerHistorial().ExportarTxt(RUTA_TXT);
            if (exportado == true)
            {
                Console.WriteLine("Historial exportado en: " + Path.GetFullPath(RUTA_TXT));
            }
            else
            {
                Console.WriteLine("No se pudo exportar el historial");
            }
        }

        private void AceptarClientes()
        {
            while (activo == true)
            {
                try
                {
                    TcpClient socket = listener.AcceptTcpClient();
                    ClienteConectado nuevo = new ClienteConectado(socket, this);

                    lock (candado)
                    {
                        clientes.Agregar(nuevo);
                    }
                    nuevo.Iniciar();
                }
                catch (Exception)
                {
                    // el listener se detuvo
                    break;
                }
            }
        }

        // ---------- Mensajes que llegan de los clientes ----------

        // Todo el procesamiento va dentro del lock para que solo una accion modifique el estado a la vez
        public void ProcesarMensaje(ClienteConectado cliente, Mensaje mensaje)
        {
            lock (candado)
            {
                if (mensaje.Tipo == TipoMensaje.CONECTAR)
                {
                    ManejarConectar(cliente, mensaje);
                    return;
                }

                Jugador? jugador = cliente.JugadorAsociado;
                if (jugador == null)
                {
                    EnviarError(cliente, "Primero debes conectarte");
                    return;
                }

                if (mensaje.Tipo == TipoMensaje.TIRAR_DADO)
                {
                    ManejarTirarDado(cliente, mensaje);
                }
                                else if (mensaje.Tipo == TipoMensaje.COMPRAR_PROPIEDAD)
                {
                    SolicitarTarjeta(cliente, AccionTarjeta.COMPRAR, "comprar la propiedad");
                }
                else if (mensaje.Tipo == TipoMensaje.NO_COMPRAR)
                {
                    SolicitarTarjeta(cliente, AccionTarjeta.NO_COMPRAR, "no comprar la propiedad");
                }
                else if (mensaje.Tipo == TipoMensaje.PAGAR_DEUDA)
                {
                    SolicitarTarjeta(cliente, AccionTarjeta.PAGAR, "pagar la deuda");
                }
                else if (mensaje.Tipo == TipoMensaje.TERMINAR_TURNO)
                {
                    SolicitarTarjeta(cliente, AccionTarjeta.TERMINAR_TURNO, "terminar el turno");
                }
                else if (mensaje.Tipo == TipoMensaje.CONSULTAR_TRANSACCIONES)
                {
                    SolicitarTarjeta(cliente, AccionTarjeta.CONSULTAR, "consultar el historial");
                }
                else if (mensaje.Tipo == TipoMensaje.REGISTRAR_TARJETA)
                {
                    ManejarRegistrarTarjeta(cliente, mensaje);
                }
                else if (mensaje.Tipo == TipoMensaje.TARJETA_RFID)
                {
                    ManejarTarjetaRfid(cliente, mensaje);
                }
                else if (mensaje.Tipo == TipoMensaje.DESCONECTAR)
                {
                    ClienteDesconectado(cliente);
                }
                else
                {
                    EnviarError(cliente, "Mensaje no reconocido");
                }
            }
        }

        public void ClienteDesconectado(ClienteConectado cliente)
        {
            lock (candado)
            {
                if (activo == false)
                {
                    return;
                }

                // Si ya se habia quitado (DESCONECTAR y luego se cierra el socket) no se hace nada
                bool estaba = clientes.Eliminar(cliente);
                if (estaba == false)
                {
                    return;
                }

                Jugador? jugador = cliente.JugadorAsociado;
                if (jugador != null)
                {
                    juego.QuitarJugador(jugador);
                    PublicarCambios(null);
                }

                cliente.Cerrar();
            }
        }

        private void ManejarConectar(ClienteConectado cliente, Mensaje mensaje)
        {
            if (cliente.JugadorAsociado != null)
            {
                EnviarError(cliente, "Ya estas conectado");
                return;
            }

            DatosConectar? datos = mensaje.LeerDatos<DatosConectar>();
            string nombre = "";
            if (datos != null)
            {
                nombre = datos.Nombre.Trim();
            }
            if (nombre == "")
            {
                nombre = "Jugador" + (juego.ObtenerJugadores().Cantidad + 1);
            }

            if (juego.HaIniciado() == true)
            {
                Rechazar(cliente, "La partida ya inicio");
                return;
            }
            if (juego.ObtenerJugadores().Cantidad >= Juego.MAX_JUGADORES)
            {
                Rechazar(cliente, "La partida esta llena");
                return;
            }
            if (NombreEnUso(nombre) == true)
            {
                Rechazar(cliente, "Ese nombre ya esta en uso");
                return;
            }

            Jugador? nuevo = juego.AgregarJugador(nombre);
            if (nuevo == null)
            {
                Rechazar(cliente, "No se pudo agregar al jugador");
                return;
            }
            cliente.JugadorAsociado = nuevo;

            DatosConexionAceptada aceptada = new DatosConexionAceptada();
            aceptada.IdJugador = nuevo.Id;
            aceptada.NumeroCasillas = juego.NumeroCasillas;
            aceptada.Jugadores = ConvertirJugadores();
            cliente.Enviar(new Mensaje(TipoMensaje.CONEXION_ACEPTADA, aceptada));

            int cantidad = juego.ObtenerJugadores().Cantidad;
            juego.AgregarEvento(nombre + " se unio a la partida (" + cantidad + "/" + Juego.MAX_JUGADORES + ")");
            PublicarCambios(null);
            
        }

        // Los dados son fisicos y el modulo es compartido: se aplican al jugador que tiene el turno
        private void ManejarTirarDado(ClienteConectado cliente, Mensaje mensaje)
        {
            DatosTirarDado? datos = mensaje.LeerDatos<DatosTirarDado>();
            if (datos == null)
            {
                EnviarError(cliente, "Faltan los valores de los dados");
                return;
            }
            if (juego.HaIniciado() == false)
            {
                EnviarError(cliente, "La partida no ha iniciado");
                return;
            }

            Jugador actual = juego.ObtenerJugadorActual();
            string error = juego.TirarDados(actual, datos.Dado1, datos.Dado2);
            if (error != "")
            {
                EnviarError(cliente, error);
                return;
            }

            PublicarCambios(actual);

            // La oferta de compra le llega al jugador que cayo, no a quien tiene el modulo
            ClienteConectado? clienteActual = BuscarClienteDeJugador(actual);
            if (clienteActual != null)
            {
                EnviarOfertaCompra(clienteActual);
            }
        }

                private void ManejarRegistrarTarjeta(ClienteConectado cliente, Mensaje mensaje)
        {
            DatosTarjeta? datos = mensaje.LeerDatos<DatosTarjeta>();
            if (datos == null)
            {
                EnviarError(cliente, "Faltan los datos de la tarjeta");
                return;
            }

            string error = juego.RegistrarTarjeta(datos.NombreJugador, datos.IdTarjeta);
            ResponderAccion(cliente, error);

            // Con 4 jugadores y todas las tarjetas registradas la partida inicia sola
            if (error == "" && juego.ObtenerJugadores().Cantidad == Juego.MAX_JUGADORES && juego.TodosTienenTarjeta() == true)
            {
                IniciarPartida();
            }
        }

        // La tarjeta identifica al jugador y confirma la accion que ese jugador eligio antes
        private void ManejarTarjetaRfid(ClienteConectado cliente, Mensaje mensaje)
        {
            DatosTarjeta? datos = mensaje.LeerDatos<DatosTarjeta>();
            if (datos == null)
            {
                EnviarError(cliente, "Faltan los datos de la tarjeta");
                return;
            }

            Jugador? dueno = juego.BuscarJugadorPorTarjeta(datos.IdTarjeta);
            if (dueno == null)
            {
                EnviarError(cliente, "Tarjeta no registrada");
                return;
            }

            ClienteConectado? clienteDueno = BuscarClienteDeJugador(dueno);
            if (clienteDueno == null)
            {
                EnviarError(cliente, "El dueno de la tarjeta no esta conectado");
                return;
            }

            AccionTarjeta accion = clienteDueno.AccionPendiente;
            if (accion == AccionTarjeta.NINGUNA)
            {
                EnviarError(cliente, dueno.Nombre + " no ha elegido ninguna accion que requiera tarjeta");
                return;
            }
            clienteDueno.AccionPendiente = AccionTarjeta.NINGUNA;

            if (accion == AccionTarjeta.COMPRAR)
            {
                ResponderAccion(clienteDueno, juego.ComprarPropiedad(dueno));
            }
            else if (accion == AccionTarjeta.NO_COMPRAR)
            {
                ResponderAccion(clienteDueno, juego.NoComprar(dueno));
            }
            else if (accion == AccionTarjeta.PAGAR)
            {
                ResponderAccion(clienteDueno, juego.PagarDeuda(dueno));
            }
            else if (accion == AccionTarjeta.TERMINAR_TURNO)
            {
                ResponderAccion(clienteDueno, juego.TerminarTurno(dueno));
            }
            else if (accion == AccionTarjeta.CONSULTAR)
            {
                EnviarHistorial(clienteDueno);
            }
        }

        // Guarda la accion y le pide la tarjeta al jugador
        private void SolicitarTarjeta(ClienteConectado cliente, AccionTarjeta accion, string texto)
        {
            cliente.AccionPendiente = accion;
            EnviarEventoA(cliente, "Acerca tu tarjeta al lector para " + texto);
        }

        private void EnviarEventoA(ClienteConectado cliente, string texto)
        {
            DatosEvento datos = new DatosEvento();
            datos.IdJugador = 0;
            datos.Descripcion = texto;
            cliente.Enviar(new Mensaje(TipoMensaje.EVENTO, datos));
        }

        private ClienteConectado? BuscarClienteDeJugador(Jugador jugador)
        {
            foreach (ClienteConectado c in clientes)
            {
                Jugador? asociado = c.JugadorAsociado;
                if (asociado != null && asociado.Id == jugador.Id)
                {
                    return c;
                }
            }
            return null;
        }

        private bool NombreEnUso(string nombre)
        {
            foreach (Jugador j in juego.ObtenerJugadores())
            {
                if (j.Nombre.ToLower() == nombre.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        private void Rechazar(ClienteConectado cliente, string motivo)
        {
            DatosConexionRechazada datos = new DatosConexionRechazada();
            datos.Motivo = motivo;
            cliente.Enviar(new Mensaje(TipoMensaje.CONEXION_RECHAZADA, datos));
            cliente.Cerrar();
        }

        // ---------- Mensajes hacia los clientes ----------

        private void ResponderAccion(ClienteConectado cliente, string error)
        {
            if (error != "")
            {
                EnviarError(cliente, error);
            }
            else
            {
                PublicarCambios(null);
            }
        }

        // El cliente redibuja el tablero al recibir estado o turno y eso borra lo impreso antes,
        // por eso primero van el estado y el turno, y despues los textos (dados y eventos)
        private void PublicarCambios(Jugador? tirador)
        {
            DatosActualizarEstado estado = new DatosActualizarEstado();
            estado.Jugadores = ConvertirJugadores();
            EnviarATodos(new Mensaje(TipoMensaje.ACTUALIZAR_ESTADO, estado));

            NotificarTurno();

            if (tirador != null)
            {
                DatosResultadoDado dados = new DatosResultadoDado();
                dados.IdJugador = tirador.Id;
                dados.Dado1 = juego.ObtenerDado1();
                dados.Dado2 = juego.ObtenerDado2();
                EnviarATodos(new Mensaje(TipoMensaje.RESULTADO_DADO, dados));
            }

            EnviarEventos();

            if (juego.EstaTerminado() == true && finEnviado == false)
            {
                finEnviado = true;
                DatosFinJuego fin = new DatosFinJuego();
                fin.IdGanador = juego.ObtenerIdGanador();
                EnviarATodos(new Mensaje(TipoMensaje.FIN_JUEGO, fin));
                ExportarHistorial();
            }
        }

        // Solo avisa TU_TURNO cuando el turno realmente cambio
        private void NotificarTurno()
        {
            if (juego.HaIniciado() == false || juego.EstaTerminado() == true)
            {
                return;
            }

            Jugador actual = juego.ObtenerJugadorActual();
            if (actual.Id == idTurnoNotificado)
            {
                return;
            }

            idTurnoNotificado = actual.Id;
            DatosTuTurno datos = new DatosTuTurno();
            datos.IdJugador = actual.Id;
            EnviarATodos(new Mensaje(TipoMensaje.TU_TURNO, datos));
        }

        private void EnviarEventos()
        {
            ListaSimple<string> lista = juego.TomarEventos();
            foreach (string texto in lista)
            {
                DatosEvento datos = new DatosEvento();
                datos.IdJugador = 0; // 0 = evento general
                datos.Descripcion = texto;
                EnviarATodos(new Mensaje(TipoMensaje.EVENTO, datos));
                Console.WriteLine("[EVENTO] " + texto);
            }
        }

        // La oferta solo se le manda al jugador que cayo en la propiedad
        private void EnviarOfertaCompra(ClienteConectado cliente)
        {
            Propiedad? oferta = juego.ObtenerOfertaPendiente();
            if (oferta == null)
            {
                return;
            }

            DatosOfertaCompra datos = new DatosOfertaCompra();
            datos.IdCasilla = oferta.Id;
            datos.NombrePropiedad = oferta.Nombre;
            datos.Precio = oferta.Precio;
            cliente.Enviar(new Mensaje(TipoMensaje.OFERTA_COMPRA, datos));
        }

        private void EnviarHistorial(ClienteConectado cliente)
        {
            DatosHistorialTransacciones datos = new DatosHistorialTransacciones();

            ListaSimple<Transaccion> lista = juego.ObtenerHistorial().ObtenerDesdeAntigua();
            foreach (Transaccion t in lista)
            {
                TransaccionInfo info = new TransaccionInfo();
                info.Id = t.Id;
                info.FechaHora = t.FechaHoraTexto();
                info.Turno = t.Turno;
                info.Tipo = t.Tipo.ToString();
                info.JugadorOrigen = t.NombreOrigen;
                info.JugadorDestino = t.NombreDestino;
                info.Monto = t.Monto;
                info.Descripcion = t.Descripcion;
                datos.Transacciones.Add(info);
            }

            cliente.Enviar(new Mensaje(TipoMensaje.HISTORIAL_TRANSACCIONES, datos));
        }

        private void EnviarError(ClienteConectado cliente, string texto)
        {
            DatosError datos = new DatosError();
            datos.Mensaje = texto;
            cliente.Enviar(new Mensaje(TipoMensaje.ERROR, datos));
        }

        private void EnviarATodos(Mensaje mensaje)
        {
            foreach (ClienteConectado c in clientes)
            {
                if (c.JugadorAsociado != null)
                {
                    c.Enviar(mensaje);
                }
            }
        }

        // Convierte los jugadores del juego al formato de red (los DTO si usan List)
        private List<JugadorEstado> ConvertirJugadores()
        {
            List<JugadorEstado> resultado = new List<JugadorEstado>();

            foreach (Jugador j in juego.ObtenerJugadores())
            {
                JugadorEstado estado = new JugadorEstado();
                estado.Id = j.Id;
                estado.Nombre = j.Nombre;
                estado.Posicion = j.Posicion;
                estado.Saldo = j.Saldo;
                estado.EnBancarrota = j.EnBancarrota;

                if (j.IdTarjeta != "")
                {
                    estado.TieneTarjeta = true;
                }

                foreach (Propiedad p in j.Propiedades)
                {
                    estado.Propiedades.Add(p.Id);
                }

                resultado.Add(estado);
            }

            return resultado;
        }
    }
}

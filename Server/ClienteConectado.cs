using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Network;

namespace Server
{
    // Un cliente conectado al servidor: lee sus mensajes en un hilo propio y le envia los del servidor
    public class ClienteConectado
    {
        public Jugador? JugadorAsociado { get; set; }
        public AccionTarjeta AccionPendiente { get; set; }
        private TcpClient socket;
        private NetworkStream stream;
        private StreamReader lector;
        private Servidor servidor;
        private object candadoEnvio;
        private bool activo;

        public ClienteConectado(TcpClient socket, Servidor servidor)
        {
            this.socket = socket;
            this.servidor = servidor;
            stream = socket.GetStream();
            lector = new StreamReader(stream, Encoding.UTF8);
            candadoEnvio = new object();
            activo = true;
            JugadorAsociado = null;
            AccionPendiente = AccionTarjeta.NINGUNA;
        }

        public void Iniciar()
        {
            Thread hilo = new Thread(Escuchar);
            hilo.IsBackground = true;
            hilo.Start();
        }

        public void Enviar(Mensaje mensaje)
        {
            if (activo == false)
            {
                return;
            }

            string texto = mensaje.Serializar() + "\n";
            byte[] bytes = Encoding.UTF8.GetBytes(texto);

            lock (candadoEnvio)
            {
                try
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception)
                {
                    activo = false;
                }
            }
        }

        public void Cerrar()
        {
            activo = false;
            try
            {
                socket.Close();
            }
            catch (Exception)
            {
                // ya estaba cerrado
            }
        }

        // Cada linea que llega es un mensaje completo (termina en \n)
        private void Escuchar()
        {
            while (activo == true)
            {
                string? linea = LeerLinea();
                if (linea == null)
                {
                    break;
                }
                if (linea.Trim() == "")
                {
                    continue;
                }

                Mensaje? mensaje = null;
                try
                {
                    mensaje = Mensaje.Deserializar(linea);
                }
                catch (Exception)
                {
                    mensaje = null;
                }

                if (mensaje != null)
                {
                    try
                    {
                        servidor.ProcesarMensaje(this, mensaje);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error procesando un mensaje: " + ex.Message);
                    }
                }
            }

            activo = false;
            servidor.ClienteDesconectado(this);
        }

        // Devuelve null si la conexion se cerro o fallo
        private string? LeerLinea()
        {
            try
            {
                return lector.ReadLine();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
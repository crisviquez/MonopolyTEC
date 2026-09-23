using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Network
{
    public class ClienteTcp
    {
        private TcpClient socket = new TcpClient();
        private NetworkStream? stream;
        private bool activo;

        // Eventos para avisar cuando llega un mensaje o se corta la conexion
        public event Action<Mensaje>? MensajeRecibido;
        public event Action? Desconectado;

        public bool Conectar(string ip, int puerto)
        {
            try
            {
                socket.Connect(ip, puerto);
                stream = socket.GetStream();
                activo = true;

                Thread hilo = new Thread(EscucharServidor);
                hilo.IsBackground = true;
                hilo.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Enviar(Mensaje mensaje)
        {
            if (stream == null)
            {
                return;
            }

            string textoSerializado = mensaje.Serializar() + "\n";
            byte[] bytes = Encoding.UTF8.GetBytes(textoSerializado);

            stream.Write(bytes, 0, bytes.Length);
        }

        private void EscucharServidor()
        {
            byte[] buffer = new byte[4096];
            StringBuilder acumulado = new StringBuilder();

            while (activo)
            {
                int leidos = 0;

                try
                {
                    leidos = stream!.Read(buffer, 0, buffer.Length);
                }
                catch (Exception)
                {
                    break;
                }

                if (leidos == 0)
                {
                    break;
                }

                string textoRecibido = Encoding.UTF8.GetString(buffer, 0, leidos);
                acumulado.Append(textoRecibido);

                int posicionSaltoDeLinea = acumulado.ToString().IndexOf('\n');

                while (posicionSaltoDeLinea >= 0)
                {
                    string textoCompleto = acumulado.ToString();

                    string linea = textoCompleto.Substring(0, posicionSaltoDeLinea);

                    acumulado.Clear();
                    acumulado.Append(textoCompleto.Substring(posicionSaltoDeLinea + 1));

                    Mensaje? mensaje = Mensaje.Deserializar(linea);

                    if (mensaje != null)
                    {
                        if (MensajeRecibido != null)
                        {
                            MensajeRecibido.Invoke(mensaje);
                        }
                    }

                    posicionSaltoDeLinea = acumulado.ToString().IndexOf('\n');
                }
            }

            activo = false;

            if (Desconectado != null)
            {
                Desconectado.Invoke();
            }
        }

        public void Cerrar()
        {
            activo = false;

            if (stream != null)
            {
                stream.Close();
            }

            socket.Close();
        }
    }
}
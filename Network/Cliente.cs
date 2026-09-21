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

        // Eventos para avisar cuando llega un mensaje o se corta la conexión
        public event Action<Mensaje>? MensajeRecibido;
        public event Action? Desconectado;

        public bool Conectar(string ip, int puerto)
        {
            try
            {
                // Conectar servidor
                socket.Connect(ip, puerto);
                stream = socket.GetStream();
                activo = true;

                // Crear un hilo secundario para mensajes en segundo plano
                Thread hilo = new Thread(EscucharServidor);
                hilo.IsBackground = true;
                hilo.Start();

                return true;
            }
            catch (Exception)
            {
                // Si ocurre un error en la conexión
                return false;
            }
        }

        public void Enviar(Mensaje mensaje)
        {
            if (stream == null)
            {
                return;
            }

            // Converierte el mensaje serializado en bytes
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
                    // Si falla, salimos del ciclo
                    break;
                }

                // Si leidos es 0 significa que el servidor cerro la conexion
                if (leidos == 0)
                {
                    break;
                }

                // Agregalos datos recibidos al acumulador de texto
                string textoRecibido = Encoding.UTF8.GetString(buffer, 0, leidos);
                acumulado.Append(textoRecibido);

                // Procesa cada linea dividida por '\n'
                int posicionSaltoDeLinea = acumulado.ToString().IndexOf('\n');

                while (posicionSaltoDeLinea >= 0)
                {
                    string textoCompleto = acumulado.ToString();

                    // Corta la primera linea recibida
                    string linea = textoCompleto.Substring(0, posicionSaltoDeLinea);

                    // Deja en el acumulador el resto del texto sin procesar
                    acumulado.Clear();
                    acumulado.Append(textoCompleto.Substring(posicionSaltoDeLinea + 1));

                    // Deserializa el texto a un objeto Mensaje
                    Mensaje? mensaje = Mensaje.Deserializar(linea);

                    if (mensaje != null)
                    {
                        // Si hay suscriptores al evento enviamos el mensaje recibido
                        if (MensajeRecibido != null)
                        {
                            MensajeRecibido.Invoke(mensaje);
                        }
                    }

                    // Buscamos si hay otro salto de línea en lo que queda acumulado
                    posicionSaltoDeLinea = acumulado.ToString().IndexOf('\n');
                }
            }

            // Si salimos del ciclo desactivamos el cliente y notificamos la desconexión
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
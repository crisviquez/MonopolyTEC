using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Network;

namespace Tests
{
    // Prueba el punto mas delicado del protocolo: que ClienteTcp reconstruya
    // mensajes aunque lleguen partidos en varios Read(), o varios pegados en uno solo
    public static class PruebaFragmentacion
    {
        public static void Ejecutar()
        {
            Console.WriteLine("=== Prueba: Mensajes fragmentados y pegados ===");

            int puerto = 6001;
            TcpListener listener = new TcpListener(IPAddress.Loopback, puerto);
            listener.Start();

            int mensajesRecibidos = 0;

            Thread servidorFalso = new Thread(() =>
            {
                using TcpClient cliente = listener.AcceptTcpClient();
                using NetworkStream stream = cliente.GetStream();

                // Mensaje 1: se envia partido en dos Write() con pausa entre ellos
                string json1 = new Mensaje(TipoMensaje.EVENTO, new DatosEvento { IdJugador = 1, Descripcion = "Fragmentado" }).Serializar() + "\n";
                byte[] bytes1 = Encoding.UTF8.GetBytes(json1);
                int mitad = bytes1.Length / 2;
                stream.Write(bytes1, 0, mitad);
                Thread.Sleep(200);
                stream.Write(bytes1, mitad, bytes1.Length - mitad);

                Thread.Sleep(200);

                // Mensajes 2 y 3: van pegados en un solo Write()
                string json2 = new Mensaje(TipoMensaje.EVENTO, new DatosEvento { IdJugador = 2, Descripcion = "Pegado A" }).Serializar() + "\n";
                string json3 = new Mensaje(TipoMensaje.EVENTO, new DatosEvento { IdJugador = 3, Descripcion = "Pegado B" }).Serializar() + "\n";
                byte[] bytesJuntos = Encoding.UTF8.GetBytes(json2 + json3);
                stream.Write(bytesJuntos, 0, bytesJuntos.Length);

                Thread.Sleep(300);
                cliente.Close();
            });
            servidorFalso.IsBackground = true;
            servidorFalso.Start();

            ClienteTcp clienteTcp = new ClienteTcp();
            clienteTcp.MensajeRecibido += (mensaje) =>
            {
                DatosEvento datos = mensaje.LeerDatos<DatosEvento>();
                Console.WriteLine("Recibido: " + datos.Descripcion);
                Interlocked.Increment(ref mensajesRecibidos);
            };

            clienteTcp.Conectar("127.0.0.1", puerto);

            Thread.Sleep(1200);

            Verificar(mensajesRecibidos == 3, $"Se reconstruyeron los 3 mensajes correctamente (recibidos: {mensajesRecibidos})");

            clienteTcp.Cerrar();
            listener.Stop();
            Console.WriteLine();
        }

        private static void Verificar(bool condicion, string descripcion)
        {
            Console.WriteLine((condicion ? "[OK] " : "[FALLO] ") + descripcion);
        }
    }
}

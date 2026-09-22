using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Network;

namespace Tests
{
    // Levanta un servidor TCP falso (no el Servidor real) para probar ClienteTcp:
    // conexion, envio, recepcion de eventos MensajeRecibido y Desconectado
    public static class PruebaEcoServidor
    {
        public static void Ejecutar()
        {
            Console.WriteLine("=== Prueba: Conexion y mensajes con servidor simulado ===");

            int puerto = 6000;
            TcpListener listener = new TcpListener(IPAddress.Loopback, puerto);
            listener.Start();

            bool recibioTuTurno = false;
            bool recibioDesconexion = false;

            Thread servidorFalso = new Thread(() =>
            {
                using TcpClient cliente = listener.AcceptTcpClient();
                using NetworkStream stream = cliente.GetStream();

                byte[] buffer = new byte[4096];
                int leidos = stream.Read(buffer, 0, buffer.Length);
                string recibido = Encoding.UTF8.GetString(buffer, 0, leidos);
                Console.WriteLine("Servidor simulado recibio: " + recibido.Trim());

                Mensaje aceptada = new Mensaje(TipoMensaje.CONEXION_ACEPTADA, new DatosConexionAceptada { IdJugador = 1 });
                EnviarCrudo(stream, aceptada);

                Mensaje turno = new Mensaje(TipoMensaje.TU_TURNO, new DatosTuTurno { IdJugador = 1 });
                EnviarCrudo(stream, turno);

                Thread.Sleep(300);
                cliente.Close(); // Para probar el evento Desconectado
            });
            servidorFalso.IsBackground = true;
            servidorFalso.Start();

            ClienteTcp clienteTcp = new ClienteTcp();
            clienteTcp.MensajeRecibido += (mensaje) =>
            {
                Console.WriteLine("Cliente recibio: " + mensaje.Tipo);
                if (mensaje.Tipo == TipoMensaje.TU_TURNO)
                {
                    recibioTuTurno = true;
                }
            };
            clienteTcp.Desconectado += () => { recibioDesconexion = true; };

            bool conectado = clienteTcp.Conectar("127.0.0.1", puerto);
            Verificar(conectado, "Conexion establecida con el servidor simulado");

            clienteTcp.Enviar(new Mensaje(TipoMensaje.CONECTAR, new DatosConectar { Nombre = "Cristopher" }));

            Thread.Sleep(1000); // Da tiempo a que lleguen los mensajes y se cierre la conexion

            Verificar(recibioTuTurno, "Se recibio el evento TU_TURNO desde el servidor simulado");
            Verificar(recibioDesconexion, "Se disparo el evento Desconectado al cerrar el servidor");

            clienteTcp.Cerrar();
            listener.Stop();
            Console.WriteLine();
        }

        private static void EnviarCrudo(NetworkStream stream, Mensaje mensaje)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(mensaje.Serializar() + "\n");
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void Verificar(bool condicion, string descripcion)
        {
            Console.WriteLine((condicion ? "[OK] " : "[FALLO] ") + descripcion);
        }
    }
}

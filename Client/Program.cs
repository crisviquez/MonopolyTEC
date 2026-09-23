using System;
using System.Threading;
using Network;

namespace Client
{
    class Program
    {
        static Cliente cliente = new Cliente();

        static void Main(string[] args)
        {
            Console.Write("IP del servidor: ");
            string ip = Console.ReadLine();
            if (string.IsNullOrEmpty(ip))
            {
                ip = "127.0.0.1";
            }

            Console.Write("Nombre: ");
            string nombre = Console.ReadLine();
            if (string.IsNullOrEmpty(nombre))
            {
                nombre = "Jugador";
            }

            cliente.OnError += MostrarError;
            cliente.OnEvento += MostrarEvento;
            cliente.OnEstadoActualizado += MostrarTablero;

            bool conectado = cliente.Conectar(ip, 5000, nombre);
            if (conectado == false)
            {
                Console.WriteLine("No se pudo conectar al servidor.");
                return;
            }

            bool jugando = true;
            while (jugando)
            {
                if (cliente.EsMiTurno())
                {
                    Console.WriteLine("");
                    Console.WriteLine("[1] Tirar dado  [2] Comprar  [3] Pagar deuda  [4] Terminar turno  [0] Salir");
                    string opcion = Console.ReadLine();

                    if (opcion == "1")
                    {
                        cliente.TirarDado();
                    }
                    else if (opcion == "2")
                    {
                        cliente.ComprarPropiedad();
                    }
                    else if (opcion == "3")
                    {
                        cliente.PagarDeuda();
                    }
                    else if (opcion == "4")
                    {
                        cliente.TerminarTurno();
                    }
                    else if (opcion == "0")
                    {
                        cliente.Desconectar();
                        jugando = false;
                    }
                }
                else
                {
                    Thread.Sleep(200);
                }
            }
        }

        static void MostrarError(string mensaje)
        {
            Console.WriteLine("[ERROR] " + mensaje);
        }

        static void MostrarEvento(string mensaje)
        {
            Console.WriteLine("[EVENTO] " + mensaje);
        }

        static void MostrarTablero()
        {
            Console.Clear();
            Console.WriteLine("=== MONOPOLY TEC ===");
            // TODO: recorrer el Tablero de Core (ListaCircularDoble<Casilla>) para dibujar el tablero completo.

            foreach (JugadorEstado j in cliente.ObtenerJugadores())
            {
                string marca = "";
                if (j.Id == cliente.ObtenerIdPropio())
                {
                    marca = " (tu)";
                }
                Console.WriteLine(j.Nombre + marca + " - Casilla " + j.Posicion + " - " + j.Saldo);
            }
        }
    }
}
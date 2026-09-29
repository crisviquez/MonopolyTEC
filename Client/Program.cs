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
            string? ip = Console.ReadLine();
            if (string.IsNullOrEmpty(ip))
            {
                ip = "127.0.0.1";
            }

            Console.Write("Nombre: ");
            string? nombre = Console.ReadLine();
            if (string.IsNullOrEmpty(nombre))
            {
                nombre = "Jugador";
            }

            cliente.OnError += MostrarError;
            cliente.OnEvento += MostrarEvento;
            cliente.OnEstadoActualizado += MostrarTablero;
            cliente.OnOfertaCompra += ManejarOfertaCompra;

            bool conectado = cliente.Conectar(ip, 5000, nombre);
            if (conectado == false)
            {
                Console.WriteLine("No se pudo conectar al servidor.");
                return;
            }

            bool jugando = true;
            while (jugando)
            {
                if (cliente.EstaConectado() == false)
                {
                    Console.WriteLine("Desconectado del servidor.");
                    jugando = false;
                    continue;
                }
                if (cliente.EsMiTurno())
                {
                    Console.WriteLine("");
                    Console.WriteLine("[1] Tirar dado  [2] Comprar  [3] Pagar deuda  [4] Terminar turno  [5] Ver historial  [6] No comprar  [0] Salir");
                    string? opcion = Console.ReadLine();

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
                    else if (opcion == "5")
                    {
                        MostrarHistorial();
                    }
                    else if (opcion == "6")
                    {
                        cliente.NoComprar();
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

        // Se dispara cuando el servidor avisa que hay una propiedad libre para comprar
        static void ManejarOfertaCompra(DatosOfertaCompra datos)
        {
            Console.WriteLine("");
            Console.WriteLine("Casilla " + datos.IdCasilla + ": " + datos.NombrePropiedad + " - Precio: " + datos.Precio);
            Console.WriteLine("Elige [2] Comprar o [6] No comprar");
        }

        static void MostrarHistorial()
        {
            cliente.ConsultarHistorial();
            Thread.Sleep(300); // no hay confirmacion de llegada; se espera un poco a que llegue la respuesta

            Console.WriteLine("");
            Console.WriteLine("=== Historial de transacciones ===");

            foreach (TransaccionInfo t in cliente.ObtenerHistorial())
            {
                Console.WriteLine(t.Id + " | Turno " + t.Turno + " | " + t.Tipo + " | " + t.JugadorOrigen + " -> " + t.JugadorDestino + " | " + t.Monto + " | " + t.Descripcion);
            }

            Console.WriteLine("");
        }

        static void MostrarTablero()
        {
            Console.Clear();
            Console.WriteLine("=== MONOPOLY TEC ===");
            Console.WriteLine("");

            DibujarTablero();
            Console.WriteLine("");

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

        // Dibuja una fila de casillas numeradas con la inicial del jugador que este ahi
        static void DibujarTablero()
        {
            int total = cliente.ObtenerNumeroCasillas();

            for (int i = 0; i < total; i++)
            {
                string casillaTexto = "[" + i;

                foreach (JugadorEstado j in cliente.ObtenerJugadores())
                {
                    if (j.Posicion == i)
                    {
                        string inicial = "?";
                        if (j.Nombre.Length > 0)
                        {
                            inicial = j.Nombre.Substring(0, 1);
                        }
                        casillaTexto = casillaTexto + inicial;
                    }
                }

                casillaTexto = casillaTexto + "]";
                Console.Write(casillaTexto + " ");

                bool finDeFila = false;
                if ((i + 1) % 8 == 0)
                {
                    finDeFila = true;
                }
                if (finDeFila == true)
                {
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }
    }
}
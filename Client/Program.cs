using Network;

namespace Client
{
    class Program
    {
        static Cliente cliente = new Cliente();
        static DispositivoHardware? hardware = null;
        static string nombrePropio = "";

        // Se usan para registrar una tarjeta: el hilo del hardware avisa al hilo principal
        static volatile bool esperandoTarjeta = false;
        static string tarjetaLeida = "";

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
            nombrePropio = nombre;

            Console.Write("Puerto serial del modulo (ej. COM3, ENTER si este PC no lo tiene): ");
            string? nombrePuerto = Console.ReadLine();

            cliente.OnError += MostrarError;
            cliente.OnEvento += MostrarEvento;
            cliente.OnEstadoActualizado += MostrarTablero;
            cliente.OnOfertaCompra += ManejarOfertaCompra;
            cliente.OnHistorial += MostrarHistorial;

            bool conectado = cliente.Conectar(ip, 5000, nombre);
            if (conectado == false)
            {
                Console.WriteLine("No se pudo conectar al servidor.");
                return;
            }

            if (string.IsNullOrEmpty(nombrePuerto) == false)
            {
                DispositivoHardware modulo = new DispositivoHardware();
                modulo.DadosLanzados += ManejarDados;
                modulo.TarjetaLeida += ManejarTarjeta;

                bool abierto = modulo.Abrir(nombrePuerto);
                if (abierto == true)
                {
                    hardware = modulo;
                }
                else
                {
                    Console.WriteLine("No se pudo abrir el puerto " + nombrePuerto);
                }
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
                    Console.WriteLine("[1] Tirar dado  [2] Comprar*  [3] Pagar deuda*  [4] Terminar turno*  [5] Ver historial*  [6] No comprar*  [0] Salir");
                    Console.WriteLine("* requiere acercar tu tarjeta al lector");
                    string? opcion = Console.ReadLine();

                    if (opcion == "1")
                    {
                        Console.WriteLine("Presiona el boton del modulo para lanzar los dados.");
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
                        cliente.ConsultarHistorial();
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
                else if (hardware != null && cliente.PartidaIniciada() == false)
                {
                    // Sala de espera: aqui se registran las tarjetas RFID
                    if (Console.KeyAvailable == true)
                    {
                        string? comando = Console.ReadLine();
                        if (comando == "R" || comando == "r")
                        {
                            RegistrarTarjeta();
                        }
                    }
                    Thread.Sleep(200);
                }
                else
                {
                    Thread.Sleep(200);
                }
            }

            if (hardware != null)
            {
                hardware.Cerrar();
            }
        }

        // Se ejecuta en el hilo del hardware cuando se presiona el boton del modulo
        static void ManejarDados(int dado1, int dado2)
        {
            cliente.TirarDado(dado1, dado2);
        }

        // Se ejecuta en el hilo del hardware cuando se acerca una tarjeta
        static void ManejarTarjeta(string idTarjeta)
        {
            if (esperandoTarjeta == true)
            {
                tarjetaLeida = idTarjeta;
                esperandoTarjeta = false;
            }
            else
            {
                cliente.UsarTarjeta(idTarjeta);
            }
        }

        static void RegistrarTarjeta()
        {
            Console.Write("Nombre del jugador dueno de la tarjeta (ENTER = " + nombrePropio + "): ");
            string? nombreJugador = Console.ReadLine();
            if (string.IsNullOrEmpty(nombreJugador))
            {
                nombreJugador = nombrePropio;
            }

            tarjetaLeida = "";
            esperandoTarjeta = true;
            Console.WriteLine("Acerca la tarjeta al lector (20 segundos)...");

            int espera = 0;
            while (esperandoTarjeta == true && espera < 200)
            {
                Thread.Sleep(100);
                espera = espera + 1;
            }

            if (esperandoTarjeta == true)
            {
                esperandoTarjeta = false;
                Console.WriteLine("No se leyo ninguna tarjeta.");
                return;
            }

            cliente.RegistrarTarjeta(tarjetaLeida, nombreJugador);
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
            Console.WriteLine("Elige [2] Comprar o [6] No comprar (con tu tarjeta)");
        }

        // Se dispara cuando llega el historial, despues de acercar la tarjeta
        static void MostrarHistorial()
        {
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

            bool partidaIniciada = cliente.PartidaIniciada();

            foreach (JugadorEstado j in cliente.ObtenerJugadores())
            {
                string marca = "";
                if (j.Id == cliente.ObtenerIdPropio())
                {
                    marca = " (tu)";
                }

                string tarjeta = "";
                if (partidaIniciada == false)
                {
                    if (j.TieneTarjeta == true)
                    {
                        tarjeta = " [tarjeta OK]";
                    }
                    else
                    {
                        tarjeta = " [SIN TARJETA]";
                    }
                }

                Console.WriteLine(j.Nombre + marca + " - Casilla " + j.Posicion + " - " + j.Saldo + tarjeta);
            }

            if (partidaIniciada == false)
            {
                Console.WriteLine("");
                Console.WriteLine("Todos los jugadores deben registrar su tarjeta RFID para iniciar la partida.");

                if (hardware != null)
                {
                    Console.WriteLine("Modulo conectado. Escribe R y ENTER para registrar una tarjeta RFID.");
                }
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
                if ((i + 1) % 10 == 0)
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
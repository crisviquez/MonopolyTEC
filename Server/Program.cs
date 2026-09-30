using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int puerto = 5000;
            int maxTurnos = Juego.MAX_TURNOS_POR_DEFECTO;

            // Opcional: dotnet run -- 40   (limite de turnos)
            if (args.Length > 0)
            {
                int valor = 0;
                bool esNumero = int.TryParse(args[0], out valor);
                if (esNumero == true && valor > 0)
                {
                    maxTurnos = valor;
                }
            }

            Servidor servidor = new Servidor(puerto, maxTurnos);
            servidor.Iniciar();

            Console.WriteLine("=== SERVIDOR MONOPOLY TEC ===");
            Console.WriteLine("Escuchando en el puerto " + puerto + " (limite de turnos: " + maxTurnos + ")");
            Console.WriteLine("La partida inicia sola con 4 jugadores registrados con tarjeta.");
            Console.WriteLine("ENTER: iniciar la partida (minimo 2 jugadores, todos con tarjeta).");
            Console.WriteLine("Con la partida iniciada, ENTER cierra el servidor.");

            bool cerrar = false;
            while (cerrar == false)
            {
                Console.ReadLine();

                if (servidor.HaIniciado() == false)
                {
                    servidor.IniciarPartida();
                }
                else
                {
                    cerrar = true;
                }
            }

            servidor.Detener();
        }
    }
}
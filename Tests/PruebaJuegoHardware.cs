using System;

namespace Tests
{
    // Prueba dados fisicos y tarjetas RFID contra el Juego, sin red ni Arduino
    public static class PruebaJuegoHardware
    {
        public static void Ejecutar()
        {
            Console.WriteLine("=== Prueba: Dados fisicos y tarjetas RFID ===");

            Juego juego = new Juego(40);
            Jugador ana = juego.AgregarJugador("Ana")!;
            Jugador beto = juego.AgregarJugador("Beto")!;

            Verificar(juego.Iniciar() != "", "No se puede iniciar sin tarjetas");
            Verificar(juego.RegistrarTarjeta("Ana", "AAA111") == "", "Se registra la tarjeta de Ana");
            Verificar(juego.RegistrarTarjeta("Beto", "AAA111") != "", "No se puede repetir una tarjeta");
            Verificar(juego.RegistrarTarjeta("Nadie", "ZZZ999") != "", "Un jugador inexistente se rechaza");
            Verificar(juego.TodosTienenTarjeta() == false, "Falta la tarjeta de Beto");
            Verificar(juego.Iniciar() != "", "No se puede iniciar si falta una tarjeta");
            Verificar(juego.RegistrarTarjeta("Beto", "BBB222") == "", "Se registra la tarjeta de Beto");
            Verificar(juego.TodosTienenTarjeta() == true, "Todos tienen tarjeta");

            Jugador? encontrado = juego.BuscarJugadorPorTarjeta("BBB222");
            Verificar(encontrado != null && encontrado.Id == beto.Id, "La tarjeta BBB222 identifica a Beto");

            Verificar(juego.Iniciar() == "", "La partida inicia con todas las tarjetas");

            Verificar(juego.RegistrarTarjeta("Ana", "CCC333") != "", "No se registran tarjetas con la partida iniciada");
            Verificar(juego.TirarDados(beto, 1, 2) != "", "Beto no puede tirar fuera de turno");
            Verificar(juego.TirarDados(ana, 0, 3) != "", "Un dado fuera de rango se rechaza");
            Verificar(juego.TirarDados(ana, 1, 2) == "", "Ana tira 1 y 2");
            Verificar(juego.ObtenerDado1() == 1 && juego.ObtenerDado2() == 2, "Se guardan los valores de los dados");
            Verificar(ana.Posicion == 3, "Ana avanza 3 casillas");
            Verificar(juego.TirarDados(ana, 1, 1) != "", "No se puede tirar dos veces en el mismo turno");

            Verificar(juego.ComprarPropiedad(ana) == "", "Ana compra la propiedad 3 (Saprissa)");
            Verificar(juego.TerminarTurno(ana) == "", "Ana termina su turno");
            Verificar(juego.TirarDados(beto, 1, 2) == "", "Beto tira 1 y 2 y cae en la propiedad de Ana");

            Jugador? pagador = juego.BuscarJugadorPorTarjeta("BBB222");
            Verificar(pagador != null && juego.PagarDeuda(pagador) == "", "La tarjeta de Beto paga el alquiler");
            Verificar(beto.Saldo == Banco.SALDO_INICIAL - 6, "Beto pago 6 de alquiler");
            Verificar(ana.Saldo == Banco.SALDO_INICIAL - 60 + 6, "Ana recibio el alquiler");

            Console.WriteLine();
        }

        private static void Verificar(bool condicion, string descripcion)
        {
            if (condicion == true)
            {
                Console.WriteLine("[OK] " + descripcion);
            }
            else
            {
                Console.WriteLine("[FALLO] " + descripcion);
            }
        }
    }
}
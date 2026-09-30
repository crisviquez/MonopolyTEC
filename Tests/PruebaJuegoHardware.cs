using System;

namespace Tests
{
    // Prueba dados fisicos y tarjetas RFID contra el Juego, sin red ni Arduino
    public static class PruebaJuegoHardware
    {
        public static void Ejecutar()
        {
            Console.WriteLine("=== Prueba: Dados fisicos y tarjetas RFID ===");

            Juego juego = new Juego(24);
            Jugador ana = juego.AgregarJugador("Ana")!;
            Jugador beto = juego.AgregarJugador("Beto")!;

            Verificar(juego.RegistrarTarjeta("Ana", "AAA111") == "", "Se registra la tarjeta de Ana");
            Verificar(juego.RegistrarTarjeta("Beto", "AAA111") != "", "No se puede repetir una tarjeta");
            Verificar(juego.RegistrarTarjeta("Nadie", "ZZZ999") != "", "Un jugador inexistente se rechaza");
            Verificar(juego.RegistrarTarjeta("Beto", "BBB222") == "", "Se registra la tarjeta de Beto");

            Jugador? encontrado = juego.BuscarJugadorPorTarjeta("BBB222");
            Verificar(encontrado != null && encontrado.Id == beto.Id, "La tarjeta BBB222 identifica a Beto");

            juego.Iniciar();

            Verificar(juego.RegistrarTarjeta("Ana", "CCC333") != "", "No se registran tarjetas con la partida iniciada");
            Verificar(juego.TirarDados(beto, 2, 3) != "", "Beto no puede tirar fuera de turno");
            Verificar(juego.TirarDados(ana, 0, 3) != "", "Un dado fuera de rango se rechaza");
            Verificar(juego.TirarDados(ana, 3, 4) == "", "Ana tira 3 y 4");
            Verificar(juego.ObtenerDado1() == 3 && juego.ObtenerDado2() == 4, "Se guardan los valores de los dados");
            Verificar(ana.Posicion == 7, "Ana avanza 7 casillas");
            Verificar(juego.TirarDados(ana, 1, 1) != "", "No se puede tirar dos veces en el mismo turno");

            Verificar(juego.ComprarPropiedad(ana) == "", "Ana compra la propiedad 7");
            Verificar(juego.TerminarTurno(ana) == "", "Ana termina su turno");
            Verificar(juego.TirarDados(beto, 3, 4) == "", "Beto tira 3 y 4 y cae en la propiedad de Ana");

            Jugador? pagador = juego.BuscarJugadorPorTarjeta("BBB222");
            Verificar(pagador != null && juego.PagarDeuda(pagador) == "", "La tarjeta de Beto paga el alquiler");
            Verificar(beto.Saldo == Banco.SALDO_INICIAL - 17, "Beto pago 17 de alquiler");
            Verificar(ana.Saldo == Banco.SALDO_INICIAL - 170 + 17, "Ana recibio el alquiler");

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

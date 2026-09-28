using System;
using System.IO;
using Monopoly.Estructuras;

namespace Tests
{
    // Prueba Transaccion y HistorialTransacciones sin red
    public static class PruebaTransaccion
    {
        public static void Ejecutar()
        {
            Console.WriteLine("=== Prueba: Transaccion e historial ===");

            HistorialTransacciones historial = new HistorialTransacciones();

            // Jugador 1 compra propiedad al banco
            historial.Agregar(new Transaccion(
                historial.ObtenerSiguienteId(), 1, TipoTransaccion.COMPRA_PROPIEDAD,
                1, "Ana", Transaccion.ID_BANCO, Transaccion.NOMBRE_BANCO, 200, "Ana compro Avenida Central"));

            // Jugador 2 paga alquiler a jugador 1
            historial.Agregar(new Transaccion(
                historial.ObtenerSiguienteId(), 2, TipoTransaccion.PAGO_ALQUILER,
                2, "Beto", 1, "Ana", 50, "Beto paga alquiler en Avenida Central"));

            // Banco premia a jugador 2 por pasar por inicio
            historial.Agregar(new Transaccion(
                historial.ObtenerSiguienteId(), 3, TipoTransaccion.PREMIO_INICIO,
                Transaccion.ID_BANCO, Transaccion.NOMBRE_BANCO, 2, "Beto", 200, "Beto paso por inicio"));

            Verificar(historial.Cantidad == 3, "Se agregaron 3 transacciones");
            Verificar(historial.ObtenerSiguienteId() == 4, "El siguiente Id es 4");

            ListaSimple<Transaccion> antigua = historial.ObtenerDesdeAntigua();
            Verificar(PrimeroDe(antigua)!.Id == 1, "Desde la mas antigua empieza en la #1");

            ListaSimple<Transaccion> reciente = historial.ObtenerDesdeReciente();
            Verificar(PrimeroDe(reciente)!.Id == 3, "Desde la mas reciente empieza en la #3");

            ListaSimple<Transaccion> deAna = historial.BuscarPorJugador(1);
            Verificar(deAna.Cantidad == 2, "Ana participa en 2 transacciones");

            ListaSimple<Transaccion> deBeto = historial.BuscarPorJugador(2);
            Verificar(deBeto.Cantidad == 2, "Beto participa en 2 transacciones");

            ListaSimple<Transaccion> alquileres = historial.BuscarPorTipo(TipoTransaccion.PAGO_ALQUILER);
            Verificar(alquileres.Cantidad == 1, "Hay 1 pago de alquiler");

            ListaSimple<Transaccion> perdidas = historial.BuscarPorTipo(TipoTransaccion.PERDIDA_EVENTO);
            Verificar(perdidas.Cantidad == 0, "No hay perdidas por evento");

            string ruta = Path.Combine(Path.GetTempPath(), "prueba_transacciones.txt");
            bool exportado = historial.ExportarTxt(ruta);
            Verificar(exportado == true, "Se exporto el TXT");
            Verificar(File.Exists(ruta) == true, "El archivo TXT existe");

            string contenido = File.ReadAllText(ruta);
            Verificar(contenido.Contains("PAGO_ALQUILER") == true, "El TXT contiene el tipo PAGO_ALQUILER");

            Console.WriteLine("");
            Console.WriteLine("Historial (mas antigua a mas reciente):");
            historial.ImprimirTodas();
            Console.WriteLine("");
        }

        private static Transaccion? PrimeroDe(ListaSimple<Transaccion> lista)
        {
            foreach (Transaccion t in lista)
            {
                return t;
            }
            return null;
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
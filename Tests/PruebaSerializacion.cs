using System;
using Network;

namespace Tests
{
    // Prueba serializacion/deserializacion sin usar red (rapida, no depende de sockets)
    public static class PruebaSerializacion
    {
        public static void Ejecutar()
        {
            Console.WriteLine("=== Prueba: Serializacion/Deserializacion ===");

            // Mensaje con DatosConectar
            Mensaje original = new Mensaje(TipoMensaje.CONECTAR, new DatosConectar { Nombre = "Cristopher"});
            string json = original.Serializar();
            Console.WriteLine("JSON: " + json);

            Mensaje? recibido = Mensaje.Deserializar(json);
            Verificar(recibido != null && recibido.Tipo == TipoMensaje.CONECTAR, "Tipo CONECTAR se preserva");

            DatosConectar datos = recibido!.LeerDatos<DatosConectar>();
            Verificar(datos.Nombre == "Cristopher", "Nombre se preserva en DatosConectar");

            // Mensaje con DatosResultadoDado
            Mensaje dados = new Mensaje(TipoMensaje.RESULTADO_DADO, new DatosResultadoDado { IdJugador = 1, Dado1 = 4, Dado2 = 6 });
            Mensaje? dadosRecibido = Mensaje.Deserializar(dados.Serializar());
            DatosResultadoDado datosDados = dadosRecibido!.LeerDatos<DatosResultadoDado>();
            Verificar(datosDados.Dado1 == 4 && datosDados.Dado2 == 6, "Valores de dados se preservan");

            // Mensaje TIRAR_DADO con los valores del dado fisico
            Mensaje tirada = new Mensaje(TipoMensaje.TIRAR_DADO, new DatosTirarDado { Dado1 = 2, Dado2 = 5 });
            Mensaje? tiradaRecibida = Mensaje.Deserializar(tirada.Serializar());
            DatosTirarDado datosTirada = tiradaRecibida!.LeerDatos<DatosTirarDado>();
            Verificar(tiradaRecibida.Tipo == TipoMensaje.TIRAR_DADO, "Tipo TIRAR_DADO se preserva");
            Verificar(datosTirada.Dado1 == 2 && datosTirada.Dado2 == 5, "Valores del dado fisico se preservan");

            // Mensaje con la tarjeta RFID
            Mensaje tarjeta = new Mensaje(TipoMensaje.REGISTRAR_TARJETA, new DatosTarjeta { IdTarjeta = "0A1B2C3D4E", NombreJugador = "Ana" });
            Mensaje? tarjetaRecibida = Mensaje.Deserializar(tarjeta.Serializar());
            DatosTarjeta datosTarjeta = tarjetaRecibida!.LeerDatos<DatosTarjeta>();
            Verificar(datosTarjeta.IdTarjeta == "0A1B2C3D4E" && datosTarjeta.NombreJugador == "Ana", "Datos de la tarjeta se preservan");

            // Mensaje sin Datos (null)
            Mensaje sinDatos = new Mensaje(TipoMensaje.PAGAR_DEUDA);
            Mensaje? sinDatosRecibido = Mensaje.Deserializar(sinDatos.Serializar());
            Verificar(sinDatosRecibido != null && sinDatosRecibido.Tipo == TipoMensaje.PAGAR_DEUDA, "Mensaje sin Datos se deserializa bien");

            Console.WriteLine();
        }

        private static void Verificar(bool condicion, string descripcion)
        {
            Console.WriteLine((condicion ? "[OK] " : "[FALLO] ") + descripcion);
        }
    }
}

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
            Mensaje original = new Mensaje(TipoMensaje.CONECTAR, new DatosConectar { Nombre = "Cristopher" });
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

            // Mensaje sin Datos (null)
            Mensaje sinDatos = new Mensaje(TipoMensaje.TIRAR_DADO);
            Mensaje? sinDatosRecibido = Mensaje.Deserializar(sinDatos.Serializar());
            Verificar(sinDatosRecibido != null && sinDatosRecibido.Tipo == TipoMensaje.TIRAR_DADO, "Mensaje sin Datos se deserializa bien");

            Console.WriteLine();
        }

        private static void Verificar(bool condicion, string descripcion)
        {
            Console.WriteLine((condicion ? "[OK] " : "[FALLO] ") + descripcion);
        }
    }
}

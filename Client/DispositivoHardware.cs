using System.IO.Ports;

namespace Client
{
    // Lee del puerto serial las lineas que manda el Arduino: "DADOS:x,y" y "RFID:id"
    public class DispositivoHardware
    {
        private SerialPort? puerto;
        private bool activo;

        public event Action<int, int>? DadosLanzados;
        public event Action<string>? TarjetaLeida;

        public bool Abrir(string nombrePuerto)
        {
            try
            {
                puerto = new SerialPort(nombrePuerto, 9600);
                puerto.ReadTimeout = 500;
                puerto.Open();
                activo = true;

                Thread hilo = new Thread(Escuchar);
                hilo.IsBackground = true;
                hilo.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Cerrar()
        {
            activo = false;

            if (puerto != null)
            {
                try
                {
                    puerto.Close();
                }
                catch (Exception)
                {
                    // ya estaba cerrado
                }
            }
        }

        private void Escuchar()
        {
            while (activo == true)
            {
                string linea = "";

                try
                {
                    linea = puerto!.ReadLine();
                }
                catch (TimeoutException)
                {
                    continue;
                }
                catch (Exception)
                {
                    break;
                }

                ProcesarLinea(linea.Trim());
            }

            activo = false;
        }

        private void ProcesarLinea(string linea)
        {
            if (linea.StartsWith("DADOS:") == true)
            {
                ProcesarDados(linea.Substring(6));
            }
            else if (linea.StartsWith("RFID:") == true)
            {
                string id = linea.Substring(5).Trim();
                if (id != "" && TarjetaLeida != null)
                {
                    TarjetaLeida.Invoke(id);
                }
            }
        }

        // El texto llega como "3,5"
        private void ProcesarDados(string texto)
        {
            int coma = texto.IndexOf(',');
            if (coma < 0)
            {
                return;
            }

            int dado1 = 0;
            int dado2 = 0;
            bool valido1 = int.TryParse(texto.Substring(0, coma), out dado1);
            bool valido2 = int.TryParse(texto.Substring(coma + 1), out dado2);
            if (valido1 == false || valido2 == false)
            {
                return;
            }

            if (DadosLanzados != null)
            {
                DadosLanzados.Invoke(dado1, dado2);
            }
        }
    }
}

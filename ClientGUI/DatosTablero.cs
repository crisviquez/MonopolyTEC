using System.Drawing;

namespace ClientGUI
{
    // Nombres y colores de las 40 casillas (mismo orden que Core/Tablero.cs)
    public static class DatosTablero
    {
        public const int TOTAL_CASILLAS = 40;

        private static string[] nombres = new string[]
        {
            "Salida", "Heredia", "Sorpresa", "Saprissa", "Impuesto", "FIFA",
            "Flamengo", "Sorpresa", "Boca Jr", "Spurs", "Carcel (visita)",
            "Chelsea", "Sorpresa", "LA Dodgers", "New England Patriots", "NFL",
            "Inter Milan", "Sorpresa", "AC Milan", "Golden State Warriors", "Zona segura",
            "Juventus", "Sorpresa", "Chicago Bulls", "New York Yankees", "NBA",
            "Liverpool", "Dallas Cowboys", "Sorpresa", "Lakers LA", "Ve a la carcel",
            "Celtics", "Manchester United", "Sorpresa", "Bayern Munich", "MLB",
            "Sorpresa", "Barcelona", "Impuesto de lujo", "Real Madrid"
        };

        private static string[] grupos = new string[]
        {
            "Especial", "Cafe", "Sorpresa", "Cafe", "Impuesto", "Asociacion",
            "Celeste", "Sorpresa", "Celeste", "Celeste", "Especial",
            "Rosa", "Sorpresa", "Rosa", "Rosa", "Asociacion",
            "Naranja", "Sorpresa", "Naranja", "Naranja", "Especial",
            "Rojo", "Sorpresa", "Rojo", "Rojo", "Asociacion",
            "Amarillo", "Amarillo", "Sorpresa", "Amarillo", "Especial",
            "Verde", "Verde", "Sorpresa", "Verde", "Asociacion",
            "Sorpresa", "Azul", "Impuesto", "Azul"
        };

        public static string ObtenerNombre(int id)
        {
            if (id < 0 || id >= TOTAL_CASILLAS)
            {
                return "Casilla " + id;
            }
            return nombres[id];
        }

        // Version corta para que quepa dentro de la casilla
        public static string ObtenerNombreCorto(int id)
        {
            string nombre = ObtenerNombre(id);

            if (nombre == "New England Patriots")
            {
                return "NE Patriots";
            }
            if (nombre == "Golden State Warriors")
            {
                return "Golden State";
            }
            if (nombre == "Manchester United")
            {
                return "Man. United";
            }
            if (nombre == "New York Yankees")
            {
                return "NY Yankees";
            }
            if (nombre == "Impuesto de lujo")
            {
                return "Impuesto lujo";
            }
            return nombre;
        }

        // Color.Empty significa que la casilla no lleva franja de color
        public static Color ObtenerColor(int id)
        {
            if (id < 0 || id >= TOTAL_CASILLAS)
            {
                return Color.Empty;
            }

            string grupo = grupos[id];
            if (grupo == "Cafe")
            {
                return Color.SaddleBrown;
            }
            if (grupo == "Celeste")
            {
                return Color.DeepSkyBlue;
            }
            if (grupo == "Rosa")
            {
                return Color.HotPink;
            }
            if (grupo == "Naranja")
            {
                return Color.Orange;
            }
            if (grupo == "Rojo")
            {
                return Color.Red;
            }
            if (grupo == "Amarillo")
            {
                return Color.Gold;
            }
            if (grupo == "Verde")
            {
                return Color.ForestGreen;
            }
            if (grupo == "Azul")
            {
                return Color.RoyalBlue;
            }
            if (grupo == "Asociacion")
            {
                return Color.DimGray;
            }
            return Color.Empty;
        }

        public static Color ObtenerFondo(int id)
        {
            if (id < 0 || id >= TOTAL_CASILLAS)
            {
                return Color.White;
            }

            string grupo = grupos[id];
            if (grupo == "Sorpresa")
            {
                return Color.FromArgb(255, 236, 179);
            }
            if (grupo == "Impuesto")
            {
                return Color.FromArgb(255, 205, 210);
            }
            if (grupo == "Especial")
            {
                return Color.FromArgb(207, 216, 220);
            }
            return Color.FromArgb(250, 250, 245);
        }

        private static int IndiceJugador(int idJugador)
        {
            int indice = (idJugador - 1) % 4;
            if (indice < 0)
            {
                indice = 0;
            }
            return indice;
        }

        public static Color ColorJugador(int idJugador)
        {
            int indice = IndiceJugador(idJugador);
            if (indice == 0)
            {
                return Color.Crimson;
            }
            if (indice == 1)
            {
                return Color.RoyalBlue;
            }
            if (indice == 2)
            {
                return Color.SeaGreen;
            }
            return Color.MediumPurple;
        }

        public static string NombreColorJugador(int idJugador)
        {
            int indice = IndiceJugador(idJugador);
            if (indice == 0)
            {
                return "Rojo";
            }
            if (indice == 1)
            {
                return "Azul";
            }
            if (indice == 2)
            {
                return "Verde";
            }
            return "Morado";
        }
    }
}
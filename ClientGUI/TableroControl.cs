using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Monopoly.Estructuras;
using Network;

namespace ClientGUI
{
    // Dibuja el tablero de 11x11 celdas: las 40 casillas van por el borde
    public class TableroControl : Panel
    {
        private ListaSimple<JugadorEstado> jugadores;
        private Image?[] imagenes;
        private int dado1;
        private int dado2;
        private Font fuenteNombre;
        private Font fuenteTitulo;
        private Font fuenteDado;
        private StringFormat formatoCentrado;

        public TableroControl()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(205, 230, 208);

            jugadores = new ListaSimple<JugadorEstado>();
            dado1 = 0;
            dado2 = 0;

            fuenteNombre = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            fuenteTitulo = new Font("Segoe UI", 42f, FontStyle.Bold);
            fuenteDado = new Font("Segoe UI", 32f, FontStyle.Bold);

            formatoCentrado = new StringFormat();
            formatoCentrado.Alignment = StringAlignment.Center;
            formatoCentrado.LineAlignment = StringAlignment.Center;

            imagenes = new Image?[DatosTablero.TOTAL_CASILLAS];
            CargarImagenes();
        }

        public void ActualizarJugadores(ListaSimple<JugadorEstado> lista)
        {
            jugadores = lista;
            Invalidate();
        }

        public void MostrarDados(int valor1, int valor2)
        {
            dado1 = valor1;
            dado2 = valor2;
            Invalidate();
        }

        // Busca Imagenes/casilla_0.png ... casilla_39.png. Si no existe, la casilla se dibuja sin imagen
        private void CargarImagenes()
        {
            for (int i = 0; i < DatosTablero.TOTAL_CASILLAS; i++)
            {
                string ruta = Path.Combine(AppContext.BaseDirectory, "Imagenes", "casilla_" + i + ".png");
                if (File.Exists(ruta) == true)
                {
                    try
                    {
                        imagenes[i] = Image.FromFile(ruta);
                    }
                    catch (Exception)
                    {
                        imagenes[i] = null;
                    }
                }
            }
        }

        private int TamanoCelda()
        {
            int lado = Width;
            if (Height < lado)
            {
                lado = Height;
            }
            return lado / 11;
        }

        // Devuelve (columna, fila) de la casilla. La Salida (0) queda abajo a la derecha
        private Point ObtenerCuadricula(int id)
        {
            if (id <= 10)
            {
                return new Point(10 - id, 10);
            }
            if (id <= 20)
            {
                return new Point(0, 20 - id);
            }
            if (id <= 30)
            {
                return new Point(id - 20, 0);
            }
            return new Point(10, id - 30);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int celda = TamanoCelda();

            for (int id = 0; id < DatosTablero.TOTAL_CASILLAS; id++)
            {
                DibujarCasilla(g, id, celda);
            }

            DibujarCentro(g, celda);
            DibujarFichas(g, celda);
        }

        private void DibujarCasilla(Graphics g, int id, int celda)
        {
            Point p = ObtenerCuadricula(id);
            Rectangle r = new Rectangle(p.X * celda, p.Y * celda, celda, celda);

            using (SolidBrush fondo = new SolidBrush(DatosTablero.ObtenerFondo(id)))
            {
                g.FillRectangle(fondo, r);
            }

            int altoFranja = celda / 5;
            Color franja = DatosTablero.ObtenerColor(id);
            if (franja.IsEmpty == false)
            {
                using (SolidBrush pincelFranja = new SolidBrush(franja))
                {
                    g.FillRectangle(pincelFranja, r.X, r.Y, r.Width, altoFranja);
                }
            }

            g.DrawRectangle(Pens.Black, r);

                        Image? imagen = imagenes[id];
            if (imagen != null)
            {
                // Con imagen no se escribe el nombre: la imagen usa todo el espacio, sin deformarse
                int disponibleAncho = r.Width - 6;
                int disponibleAlto = r.Height - altoFranja - 10;
                int lado = disponibleAncho;
                if (disponibleAlto < lado)
                {
                    lado = disponibleAlto;
                }

                int x = r.X + ((r.Width - lado) / 2);
                int y = r.Y + altoFranja + 2 + ((disponibleAlto - lado) / 2);
                g.DrawImage(imagen, new Rectangle(x, y, lado, lado));
            }
            else
            {
                Rectangle areaTexto = new Rectangle(r.X + 2, r.Y + altoFranja, r.Width - 4, r.Height - altoFranja - 2);
                g.DrawString(DatosTablero.ObtenerNombreCorto(id), fuenteNombre, Brushes.Black, areaTexto, formatoCentrado);
            }

            // Barra del color del jugador que es dueno de la propiedad
            JugadorEstado? dueno = BuscarDueno(id);
            if (dueno != null)
            {
                using (SolidBrush pincelDueno = new SolidBrush(DatosTablero.ColorJugador(dueno.Id)))
                {
                    g.FillRectangle(pincelDueno, r.X + 1, r.Bottom - 7, r.Width - 2, 6);
                }
            }
        }

        private JugadorEstado? BuscarDueno(int idCasilla)
        {
            foreach (JugadorEstado j in jugadores)
            {
                foreach (int idPropiedad in j.Propiedades)
                {
                    if (idPropiedad == idCasilla)
                    {
                        return j;
                    }
                }
            }
            return null;
        }

        private void DibujarCentro(Graphics g, int celda)
        {
            Rectangle centro = new Rectangle(celda, celda, celda * 9, celda * 9);

            Rectangle areaTitulo = new Rectangle(centro.X, centro.Y + celda, centro.Width, celda * 2);
            g.DrawString("MONOPOLY TEC", fuenteTitulo, Brushes.DarkGreen, areaTitulo, formatoCentrado);

            if (dado1 > 0 && dado2 > 0)
            {
                int lado = celda * 2;
                int y = centro.Y + (centro.Height - lado) / 2;
                int x1 = centro.X + (centro.Width / 2) - lado - 10;
                int x2 = centro.X + (centro.Width / 2) + 10;

                DibujarDado(g, new Rectangle(x1, y, lado, lado), dado1);
                DibujarDado(g, new Rectangle(x2, y, lado, lado), dado2);
            }
        }

        private void DibujarDado(Graphics g, Rectangle r, int valor)
        {
            g.FillRectangle(Brushes.White, r);
            g.DrawRectangle(Pens.Black, r);
            g.DrawString(valor.ToString(), fuenteDado, Brushes.Black, r, formatoCentrado);
        }

        // Cada jugador ocupa un lugar fijo dentro de la casilla (cuadricula de 2x2)
        private void DibujarFichas(Graphics g, int celda)
        {
            int diametro = celda / 3;

            foreach (JugadorEstado j in jugadores)
            {
                if (j.EnBancarrota == true)
                {
                    continue;
                }
                if (j.Posicion < 0 || j.Posicion >= DatosTablero.TOTAL_CASILLAS)
                {
                    continue;
                }

                Point p = ObtenerCuadricula(j.Posicion);
                int indice = (j.Id - 1) % 4;
                if (indice < 0)
                {
                    indice = 0;
                }
                int columna = indice % 2;
                int fila = indice / 2;

                int x = (p.X * celda) + ((celda - (2 * diametro)) / 2) + (columna * diametro);
                int y = (p.Y * celda) + celda - (2 * diametro) - 8 + (fila * diametro);

                using (SolidBrush pincel = new SolidBrush(DatosTablero.ColorJugador(j.Id)))
                {
                    g.FillEllipse(pincel, x, y, diametro, diametro);
                }
                g.DrawEllipse(Pens.Black, x, y, diametro, diametro);
            }
        }
    }
}
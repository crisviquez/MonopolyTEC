using System;
using System.Drawing;
using System.Windows.Forms;
using Client;
using Monopoly.Estructuras;
using Network;

namespace ClientGUI
{
    public class VentanaPrincipal : Form
    {
        private const int PUERTO_SERVIDOR = 5000;

        private Cliente cliente;
        private DispositivoHardware? hardware;
        private Random azar;
        private string nombrePropio;
        private string nombreParaTarjeta;
        private string ultimoError;
        private bool esperandoTarjeta;
        private bool partidaTerminada;

        // Pantalla de conexion
        private Panel pConexion = new Panel();
        private TextBox txtIp = new TextBox();
        private TextBox txtNombre = new TextBox();
        private TextBox txtPuerto = new TextBox();
        private Button btnConectar = new Button();
        private Label lblEstadoConexion = new Label();

        // Pantalla de juego
        private Panel pJuego = new Panel();
        private TableroControl tablero = new TableroControl();
        private Label lblTurno = new Label();
        private ListBox lstJugadores = new ListBox();
        private Label lblPropiedades = new Label();
        private Panel pRegistro = new Panel();
        private TextBox txtNombreTarjeta = new TextBox();
        private Button btnRegistrar = new Button();
        private Button btnComprar = new Button();
        private Button btnNoComprar = new Button();
        private Button btnPagar = new Button();
        private Button btnTerminar = new Button();
        private Button btnHistorial = new Button();
        private Button btnSalir = new Button();
        private Button btnDadosSim = new Button();
        private Button btnTarjetaSim = new Button();
        private Label lblAviso = new Label();
        private TextBox txtEventos = new TextBox();
        private Timer timerRegistro = new Timer();

        public VentanaPrincipal()
        {
            cliente = new Cliente();
            hardware = null;
            azar = new Random();
            nombrePropio = "";
            nombreParaTarjeta = "";
            ultimoError = "";
            esperandoTarjeta = false;
            partidaTerminada = false;

            Text = "Monopoly TEC";
            ClientSize = new Size(1120, 670);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            timerRegistro.Interval = 20000;
            timerRegistro.Tick += AlAgotarseRegistro;

            ConstruirPantallaConexion();
            ConstruirPantallaJuego();
            Controls.Add(pConexion);
            Controls.Add(pJuego);
            MostrarPantallaConexion();

            FormClosing += AlCerrarVentana;
        }

        // ---------- Construccion de pantallas ----------

        private void CrearEtiqueta(Control padre, string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.AutoSize = true;
            etiqueta.Font = new Font("Segoe UI", 10f);
            etiqueta.Location = new Point(x, y);
            padre.Controls.Add(etiqueta);
        }

        private void ConfigurarBoton(Button boton, Control padre, string texto, int x, int y, int ancho)
        {
            boton.Text = texto;
            boton.Location = new Point(x, y);
            boton.Size = new Size(ancho, 36);
            boton.Font = new Font("Segoe UI", 10f);
            padre.Controls.Add(boton);
        }

        private void ConstruirPantallaConexion()
        {
            pConexion.Dock = DockStyle.Fill;

            Label titulo = new Label();
            titulo.Text = "MONOPOLY TEC";
            titulo.Font = new Font("Segoe UI", 28f, FontStyle.Bold);
            titulo.AutoSize = true;
            titulo.Location = new Point(390, 90);
            pConexion.Controls.Add(titulo);

            CrearEtiqueta(pConexion, "IP del servidor:", 390, 190);
            txtIp.Text = "127.0.0.1";
            txtIp.Location = new Point(390, 215);
            txtIp.Width = 320;
            txtIp.Font = new Font("Segoe UI", 11f);
            pConexion.Controls.Add(txtIp);

            CrearEtiqueta(pConexion, "Tu nombre:", 390, 260);
            txtNombre.Text = "";
            txtNombre.Location = new Point(390, 285);
            txtNombre.Width = 320;
            txtNombre.Font = new Font("Segoe UI", 11f);
            pConexion.Controls.Add(txtNombre);

            CrearEtiqueta(pConexion, "Puerto serial del modulo (ej. COM3). Vacio si este PC no lo tiene:", 390, 330);
            txtPuerto.Text = "";
            txtPuerto.Location = new Point(390, 355);
            txtPuerto.Width = 320;
            txtPuerto.Font = new Font("Segoe UI", 11f);
            pConexion.Controls.Add(txtPuerto);

            ConfigurarBoton(btnConectar, pConexion, "Conectar", 390, 410, 320);
            btnConectar.Click += BotonConectar_Click;

            lblEstadoConexion.Text = "";
            lblEstadoConexion.ForeColor = Color.DarkRed;
            lblEstadoConexion.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            lblEstadoConexion.AutoSize = false;
            lblEstadoConexion.Location = new Point(390, 460);
            lblEstadoConexion.Size = new Size(400, 60);
            pConexion.Controls.Add(lblEstadoConexion);
        }

        private void ConstruirPantallaJuego()
        {
            pJuego.Dock = DockStyle.Fill;

            tablero.Location = new Point(10, 10);
            tablero.Size = new Size(640, 640);
            pJuego.Controls.Add(tablero);

            lblTurno.Location = new Point(670, 10);
            lblTurno.Size = new Size(440, 30);
            lblTurno.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            pJuego.Controls.Add(lblTurno);

            lstJugadores.Location = new Point(670, 45);
            lstJugadores.Size = new Size(440, 100);
            lstJugadores.Font = new Font("Segoe UI", 10f);
            pJuego.Controls.Add(lstJugadores);

            lblPropiedades.Location = new Point(670, 150);
            lblPropiedades.Size = new Size(440, 60);
            lblPropiedades.Font = new Font("Segoe UI", 9f);
            pJuego.Controls.Add(lblPropiedades);

            // Registro de tarjeta (solo en la sala de espera)
            pRegistro.Location = new Point(670, 215);
            pRegistro.Size = new Size(440, 36);
            pJuego.Controls.Add(pRegistro);

            CrearEtiqueta(pRegistro, "Tarjeta de:", 0, 8);
            txtNombreTarjeta.Location = new Point(75, 6);
            txtNombreTarjeta.Width = 150;
            txtNombreTarjeta.Font = new Font("Segoe UI", 10f);
            pRegistro.Controls.Add(txtNombreTarjeta);
            ConfigurarBoton(btnRegistrar, pRegistro, "Registrar tarjeta", 235, 0, 205);
            btnRegistrar.Click += BotonRegistrar_Click;

            // Acciones (todas menos los dados y salir piden tarjeta)
            ConfigurarBoton(btnComprar, pJuego, "Comprar", 670, 260, 215);
            ConfigurarBoton(btnNoComprar, pJuego, "No comprar", 895, 260, 215);
            ConfigurarBoton(btnPagar, pJuego, "Pagar deuda", 670, 302, 215);
            ConfigurarBoton(btnTerminar, pJuego, "Terminar turno", 895, 302, 215);
            ConfigurarBoton(btnHistorial, pJuego, "Ver historial", 670, 344, 215);
            ConfigurarBoton(btnSalir, pJuego, "Salir", 895, 344, 215);
            btnComprar.Click += BotonComprar_Click;
            btnNoComprar.Click += BotonNoComprar_Click;
            btnPagar.Click += BotonPagar_Click;
            btnTerminar.Click += BotonTerminar_Click;
            btnHistorial.Click += BotonHistorial_Click;
            btnSalir.Click += BotonSalir_Click;

            // Solo aparecen si este PC no tiene el modulo
            ConfigurarBoton(btnDadosSim, pJuego, "Dados (simulado)", 670, 386, 215);
            ConfigurarBoton(btnTarjetaSim, pJuego, "Tarjeta (simulada)", 895, 386, 215);
            btnDadosSim.Click += BotonDadosSim_Click;
            btnTarjetaSim.Click += BotonTarjetaSim_Click;

            lblAviso.Location = new Point(670, 430);
            lblAviso.Size = new Size(440, 50);
            lblAviso.ForeColor = Color.DarkRed;
            lblAviso.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            pJuego.Controls.Add(lblAviso);

            txtEventos.Location = new Point(670, 485);
            txtEventos.Size = new Size(440, 175);
            txtEventos.Multiline = true;
            txtEventos.ReadOnly = true;
            txtEventos.ScrollBars = ScrollBars.Vertical;
            txtEventos.BackColor = Color.White;
            txtEventos.Font = new Font("Segoe UI", 9f);
            pJuego.Controls.Add(txtEventos);
        }

        private void MostrarPantallaConexion()
        {
            pJuego.Visible = false;
            pConexion.Visible = true;
        }

        private void MostrarPantallaJuego()
        {
            bool sinModulo = false;
            if (hardware == null)
            {
                sinModulo = true;
            }
            btnDadosSim.Visible = sinModulo;
            btnTarjetaSim.Visible = sinModulo;

            lblAviso.Text = "";
            txtEventos.Clear();
            tablero.MostrarDados(0, 0);
            tablero.ActualizarJugadores(cliente.ObtenerJugadores());
            ActualizarTodo();

            pConexion.Visible = false;
            pJuego.Visible = true;
        }

        // ---------- Botones ----------

        private void BotonConectar_Click(object? sender, EventArgs e)
        {
            string ip = txtIp.Text.Trim();
            if (ip == "")
            {
                ip = "127.0.0.1";
            }
            string nombre = txtNombre.Text.Trim();
            if (nombre == "")
            {
                nombre = "Jugador";
            }
            string puerto = txtPuerto.Text.Trim();

            nombrePropio = nombre;
            ultimoError = "";
            partidaTerminada = false;
            esperandoTarjeta = false;
            lblEstadoConexion.Text = "";

            // Un Cliente nuevo por cada intento, porque el socket no se puede reutilizar
            cliente = new Cliente();
            cliente.OnError += AlRecibirError;
            cliente.OnEvento += AlRecibirEvento;
            cliente.OnEstadoActualizado += AlActualizarEstado;
            cliente.OnOfertaCompra += AlRecibirOferta;
            cliente.OnHistorial += AlRecibirHistorial;
            cliente.OnResultadoDado += AlRecibirDados;
            cliente.OnFinJuego += AlFinalizarJuego;

            bool conectado = cliente.Conectar(ip, PUERTO_SERVIDOR, nombre);
            if (conectado == false)
            {
                lblEstadoConexion.Text = "No se pudo conectar al servidor.";
                return;
            }

            hardware = null;
            if (puerto != "")
            {
                DispositivoHardware modulo = new DispositivoHardware();
                modulo.DadosLanzados += AlLanzarDados;
                modulo.TarjetaLeida += AlLeerTarjeta;

                bool abierto = modulo.Abrir(puerto);
                if (abierto == true)
                {
                    hardware = modulo;
                }
                else
                {
                    MessageBox.Show("No se pudo abrir el puerto " + puerto + ". Se usara el modo sin modulo.");
                }
            }

            txtNombreTarjeta.Text = nombre;
            MostrarPantallaJuego();
        }

        private void BotonRegistrar_Click(object? sender, EventArgs e)
        {
            string nombre = txtNombreTarjeta.Text.Trim();
            if (nombre == "")
            {
                nombre = nombrePropio;
            }
            nombreParaTarjeta = nombre;

            if (hardware == null)
            {
                // Sin modulo: se registra una tarjeta inventada para poder probar
                cliente.RegistrarTarjeta("SIM" + nombre, nombre);
                return;
            }

            esperandoTarjeta = true;
            lblAviso.Text = "Acerca la tarjeta de " + nombre + " al lector (20 segundos)...";
            timerRegistro.Stop();
            timerRegistro.Start();
        }

        private void AlAgotarseRegistro(object? sender, EventArgs e)
        {
            timerRegistro.Stop();
            if (esperandoTarjeta == true)
            {
                esperandoTarjeta = false;
                lblAviso.Text = "No se leyo ninguna tarjeta.";
            }
        }

        private void BotonComprar_Click(object? sender, EventArgs e)
        {
            cliente.ComprarPropiedad();
        }

        private void BotonNoComprar_Click(object? sender, EventArgs e)
        {
            cliente.NoComprar();
        }

        private void BotonPagar_Click(object? sender, EventArgs e)
        {
            cliente.PagarDeuda();
        }

        private void BotonTerminar_Click(object? sender, EventArgs e)
        {
            cliente.TerminarTurno();
        }

        private void BotonHistorial_Click(object? sender, EventArgs e)
        {
            cliente.ConsultarHistorial();
        }

        private void BotonSalir_Click(object? sender, EventArgs e)
        {
            cliente.Desconectar();
            CerrarHardware();
            lblEstadoConexion.Text = "";
            MostrarPantallaConexion();
        }

        private void BotonDadosSim_Click(object? sender, EventArgs e)
        {
            cliente.TirarDado(azar.Next(1, 7), azar.Next(1, 7));
        }

        private void BotonTarjetaSim_Click(object? sender, EventArgs e)
        {
            cliente.UsarTarjeta("SIM" + nombrePropio);
        }

        private void AlCerrarVentana(object? sender, FormClosingEventArgs e)
        {
            if (cliente.EstaConectado() == true)
            {
                cliente.Desconectar();
            }
            CerrarHardware();
        }

        private void CerrarHardware()
        {
            if (hardware != null)
            {
                hardware.Cerrar();
                hardware = null;
            }
        }

        // ---------- Hardware (llegan desde el hilo del modulo) ----------

        private void AlLanzarDados(int dado1, int dado2)
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action<int, int>(AlLanzarDados), dado1, dado2);
                return;
            }

            cliente.TirarDado(dado1, dado2);
        }

        private void AlLeerTarjeta(string idTarjeta)
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action<string>(AlLeerTarjeta), idTarjeta);
                return;
            }

            if (esperandoTarjeta == true)
            {
                esperandoTarjeta = false;
                timerRegistro.Stop();
                cliente.RegistrarTarjeta(idTarjeta, nombreParaTarjeta);
                lblAviso.Text = "Tarjeta enviada para " + nombreParaTarjeta;
            }
            else
            {
                cliente.UsarTarjeta(idTarjeta);
            }
        }

        // ---------- Eventos del cliente (llegan desde el hilo de red) ----------

        private void AlRecibirError(string mensaje)
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action<string>(AlRecibirError), mensaje);
                return;
            }

            // Si ya no hay conexion volvemos a la pantalla inicial
            if (cliente.EstaConectado() == false)
            {
                string texto = mensaje;
                if (ultimoError != "")
                {
                    texto = ultimoError + " | " + mensaje;
                }
                CerrarHardware();
                lblEstadoConexion.Text = texto;
                MostrarPantallaConexion();
                return;
            }

            ultimoError = mensaje;
            lblAviso.Text = mensaje;
            AgregarLog("[ERROR] " + mensaje);
        }

        private void AlRecibirEvento(string texto)
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action<string>(AlRecibirEvento), texto);
                return;
            }

            if (texto.StartsWith("Acerca tu tarjeta") == true)
            {
                lblAviso.Text = texto;
            }
            AgregarLog(texto);
        }

        private void AlActualizarEstado()
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action(AlActualizarEstado));
                return;
            }

            tablero.ActualizarJugadores(cliente.ObtenerJugadores());
            ActualizarTodo();
        }

        private void AlRecibirOferta(DatosOfertaCompra datos)
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action<DatosOfertaCompra>(AlRecibirOferta), datos);
                return;
            }

            lblAviso.Text = "Casilla " + datos.IdCasilla + ": " + datos.NombrePropiedad
                + " - Precio " + datos.Precio + ". Elige Comprar o No comprar.";
        }

        private void AlRecibirDados(DatosResultadoDado datos)
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action<DatosResultadoDado>(AlRecibirDados), datos);
                return;
            }

            tablero.MostrarDados(datos.Dado1, datos.Dado2);
        }

        private void AlFinalizarJuego(int idGanador)
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action<int>(AlFinalizarJuego), idGanador);
                return;
            }

            partidaTerminada = true;
            ActualizarTodo();
            MessageBox.Show("La partida termino. Gana " + NombreDe(idGanador));
        }

        private void AlRecibirHistorial()
        {
            if (IsDisposed == true)
            {
                return;
            }
            if (InvokeRequired == true)
            {
                BeginInvoke(new Action(AlRecibirHistorial));
                return;
            }

            string texto = "";
            foreach (TransaccionInfo t in cliente.ObtenerHistorial())
            {
                texto = texto + t.Id + " | Turno " + t.Turno + " | " + t.Tipo + " | "
                    + t.JugadorOrigen + " -> " + t.JugadorDestino + " | " + t.Monto
                    + " | " + t.Descripcion + Environment.NewLine;
            }

            TextBox caja = new TextBox();
            caja.Multiline = true;
            caja.ReadOnly = true;
            caja.WordWrap = false;
            caja.ScrollBars = ScrollBars.Both;
            caja.Dock = DockStyle.Fill;
            caja.Font = new Font("Consolas", 9f);
            caja.Text = texto;

            Form ventana = new Form();
            ventana.Text = "Historial de transacciones";
            ventana.Size = new Size(900, 500);
            ventana.StartPosition = FormStartPosition.CenterParent;
            ventana.Controls.Add(caja);
            caja.SelectionLength = 0;
            ventana.Show(this);
        }

        // ---------- Actualizacion de controles ----------

        private void AgregarLog(string texto)
        {
            txtEventos.AppendText(texto + Environment.NewLine);
        }

        private string NombreDe(int idJugador)
        {
            foreach (JugadorEstado j in cliente.ObtenerJugadores())
            {
                if (j.Id == idJugador)
                {
                    return j.Nombre;
                }
            }
            return "jugador " + idJugador;
        }

        private void ActualizarTodo()
        {
            ActualizarTurno();
            ActualizarListaJugadores();
            ActualizarPropiedades();
            ActualizarBotones();
        }

        private void ActualizarTurno()
        {
            if (cliente.PartidaIniciada() == false)
            {
                lblTurno.Text = "Sala de espera";
            }
            else if (partidaTerminada == true)
            {
                lblTurno.Text = "Partida terminada";
            }
            else if (cliente.EsMiTurno() == true)
            {
                lblTurno.Text = "TU TURNO";
            }
            else
            {
                lblTurno.Text = "Turno de " + NombreDe(cliente.ObtenerTurnoActualId());
            }
        }

        private void ActualizarListaJugadores()
        {
            bool iniciada = cliente.PartidaIniciada();
            int turno = cliente.ObtenerTurnoActualId();

            lstJugadores.Items.Clear();
            foreach (JugadorEstado j in cliente.ObtenerJugadores())
            {
                string linea = "";
                if (iniciada == true && j.Id == turno)
                {
                    linea = "> ";
                }

                linea = linea + "[" + DatosTablero.NombreColorJugador(j.Id) + "] " + j.Nombre;
                if (j.Id == cliente.ObtenerIdPropio())
                {
                    linea = linea + " (tu)";
                }
                linea = linea + "   ₡" + j.Saldo + "   casilla " + j.Posicion;

                if (j.EnBancarrota == true)
                {
                    linea = linea + "   [ELIMINADO]";
                }
                if (iniciada == false)
                {
                    if (j.TieneTarjeta == true)
                    {
                        linea = linea + "   [tarjeta OK]";
                    }
                    else
                    {
                        linea = linea + "   [SIN TARJETA]";
                    }
                }

                lstJugadores.Items.Add(linea);
            }
        }

        private void ActualizarPropiedades()
        {
            string texto = "Mis propiedades: ";
            int cantidad = 0;

            foreach (JugadorEstado j in cliente.ObtenerJugadores())
            {
                if (j.Id == cliente.ObtenerIdPropio())
                {
                    foreach (int idPropiedad in j.Propiedades)
                    {
                        if (cantidad > 0)
                        {
                            texto = texto + ", ";
                        }
                        texto = texto + DatosTablero.ObtenerNombre(idPropiedad);
                        cantidad = cantidad + 1;
                    }
                }
            }

            if (cantidad == 0)
            {
                texto = texto + "ninguna";
            }
            lblPropiedades.Text = texto;
        }

        private void ActualizarBotones()
        {
            bool iniciada = cliente.PartidaIniciada();

            bool puedeActuar = false;
            if (cliente.EsMiTurno() == true && partidaTerminada == false)
            {
                puedeActuar = true;
            }

            btnComprar.Enabled = puedeActuar;
            btnNoComprar.Enabled = puedeActuar;
            btnPagar.Enabled = puedeActuar;
            btnTerminar.Enabled = puedeActuar;
            btnDadosSim.Enabled = puedeActuar;
            btnHistorial.Enabled = iniciada;
            btnTarjetaSim.Enabled = iniciada;

            // El registro de tarjetas solo existe antes de iniciar
            pRegistro.Visible = !iniciada;
        }
    }
}
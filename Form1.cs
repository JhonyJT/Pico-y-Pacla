using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PICO_PLACAS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

      
        private void fechaSalir(object sender, EventArgs e)
        {
         if (txtFecha.Text == "") {
             txtFecha.Text = "AAAA-MM-DD";

             txtFecha.ForeColor = Color.Silver;
            }
        }

        private void fechaEntrar(object sender, EventArgs e)
        {
         if (txtFecha.Text == "AAAA-MM-DD") {
             txtFecha.Text = "";

             txtFecha.ForeColor = Color.Black;
            }
        }

        private void horaEnter(object sender, EventArgs e)
        {
            if (txtHora.Text == "HH:MM")
            {
                txtHora.Text = "";

                txtHora.ForeColor = Color.Black;
            }
        }

        private void horaSalir(object sender, EventArgs e)
        {
            if (txtHora.Text == "")
            {
                txtHora.Text = "HH:MM";

                txtHora.ForeColor = Color.Silver;
            }
        }

        private void placaEntrar(object sender, EventArgs e)
        {
            if (txtPlaca.Text == "AAA - 0123")
            {
                txtPlaca.Text = "";

                txtPlaca.ForeColor = Color.Black;
            }
        }

        private void placaSalir(object sender, EventArgs e)
        {
            if (txtPlaca.Text == "")
            {
                txtPlaca.Text = "AAA - 0123";

                txtPlaca.ForeColor = Color.Silver;
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=LAPTOP-DKTPKMQ6;Database=PicoPlacaDB;Trusted_Connection=True;";

            // Obtener los valores ingresados por el usuario
            string fechaInput = txtFecha.Text;
            string horaInput = txtHora.Text;
            string placa = txtPlaca.Text;

            DateTime fecha;
            TimeSpan hora;

            // Validar las entradas
            if (!DateTime.TryParseExact(fechaInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                MessageBox.Show("Fecha no válida.");
                return;
            }

            if (!TimeSpan.TryParse(horaInput, out hora))
            {
                MessageBox.Show("Hora no válida.");
                return;
            }

            if (placa.Length == 0 || !char.IsDigit(placa[placa.Length - 1]))
            {
                MessageBox.Show("Número de placa no válido.");
                return;
            }

            char ultimoDigito = placa[placa.Length - 1];
            int digito = int.Parse(ultimoDigito.ToString());

            // Reglas de Pico y Placa en Quito
            var restricciones = new (DayOfWeek dia, int[] digitos)[]
            {
                (DayOfWeek.Monday, new int[] { 1, 2 }),
                (DayOfWeek.Tuesday, new int[] { 3, 4 }),
                (DayOfWeek.Wednesday, new int[] { 5, 6 }),
                (DayOfWeek.Thursday, new int[] { 7, 8 }),
                (DayOfWeek.Friday, new int[] { 9, 0 })
            };

            TimeSpan inicioManana = new TimeSpan(7, 0, 0);
            TimeSpan finManana = new TimeSpan(9, 30, 0);
            TimeSpan inicioTarde = new TimeSpan(16, 0, 0);
            TimeSpan finTarde = new TimeSpan(19, 30, 0);

            bool puedeCircular = true;
            string mensaje = "El vehículo PUEDE circular en la fecha y hora especificadas.";

            foreach (var restriccion in restricciones)
            {
                if (fecha.DayOfWeek == restriccion.dia && Array.Exists(restriccion.digitos, d => d == digito))
                {
                    if ((hora >= inicioManana && hora <= finManana) || (hora >= inicioTarde && hora <= finTarde))
                    {
                        puedeCircular = false;
                        mensaje = "El vehículo NO puede circular en la fecha y hora especificadas.";
                        break;
                    }
                }
            }

            // Mostrar el resultado en la interfaz
            lblResultado.Text = mensaje;

            // Guardar la información en la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Consultas (FechaConsulta, FechaIngresada, HoraIngresada, NumeroPlaca, PuedeCircular, Mensaje) " +
                               "VALUES (@FechaConsulta, @FechaIngresada, @HoraIngresada, @NumeroPlaca, @PuedeCircular, @Mensaje)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FechaConsulta", DateTime.Now);
                    command.Parameters.AddWithValue("@FechaIngresada", fecha);
                    command.Parameters.AddWithValue("@HoraIngresada", hora);
                    command.Parameters.AddWithValue("@NumeroPlaca", placa);
                    command.Parameters.AddWithValue("@PuedeCircular", puedeCircular);
                    command.Parameters.AddWithValue("@Mensaje", mensaje);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}

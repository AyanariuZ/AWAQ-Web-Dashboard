using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace AWAQ_Web
{
    public class MainPageModel : PageModel
    {
        public string CurrentView { get; set; } = "_DashboardPartial.cshtml";
        public string ContentTitle { get; set; } = "Dashboard";

        public List<Colaborador> Collaborators { get; set; } = new List<Colaborador>();

        [BindProperty]
        public string Nombre { get; set; }
        [BindProperty]
        public string Apellidos { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string TipoUsuario { get; set; }
        [BindProperty]
        public string Password { get; set; }
        [BindProperty]
        public string PasswordConfirm { get; set; }

        public void OnGetDashboard()
        {
            CurrentView = "_DashboardPartial.cshtml";
            ContentTitle = "Dashboard";
        }

        public void OnGetCollaborators()
        {
            LoadCollaborators();
            CurrentView = "_CollaboratorsPartial.cshtml";
            ContentTitle = "Colaboradores";
        }

        public void OnGetAddCollaborator()
        {
            CurrentView = "_AddCollaboratorPartial.cshtml";
            ContentTitle = "Agregar Colaborador";
        }

        public IActionResult OnPostRegisterCollaborator()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                string nombreCompleto = $"{Nombre} {Apellidos}";
                string connString = "Server=127.0.0.1;Port=3306;Database=Awak;Uid=root;password=xUgcn5c1;";
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT INTO usuarios (nombre, email, tipo_usuario, password) VALUES (@nombre, @email, @tipo_usuario, @password)", conn);
                    cmd.Parameters.AddWithValue("@nombre", nombreCompleto);
                    cmd.Parameters.AddWithValue("@email", Email);
                    cmd.Parameters.AddWithValue("@tipo_usuario", TipoUsuario);
                    cmd.Parameters.AddWithValue("@password", Password);
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Colaborador registrado exitosamente.";
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al registrar el colaborador: {ex.Message}");
                return Page();
            }
        }

        private void LoadCollaborators()
        {
            string connString = "Server=127.0.0.1;Port=3306;Database=Awak;Uid=root;password=xUgcn5c1;";
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                var cmd = new MySqlCommand(@"SELECT u.usuario_id, u.tipo_usuario, SUBSTRING_INDEX(u.nombre, ' ', 1) AS nombre, u.fecha_ingreso, 
                                             COALESCE(a.intentos, 0) AS intentos, COALESCE(a.puntaje_global, 0) AS puntaje_global, e.estado
                                             FROM Usuarios u
                                             LEFT JOIN AvancePorUsuario a ON u.usuario_id = a.usuario_id
                                             LEFT JOIN Juegos j ON a.juego_id = j.juego_id
                                             LEFT JOIN EstadosDeJuego e ON j.juego_id = e.juego_id", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Collaborators.Add(new Colaborador
                        {
                            Id = reader.GetInt32("usuario_id"),
                            TipoUsuario = reader.GetString("tipo_usuario"),
                            Nombre = reader.GetString("nombre"),
                            FechaIngreso = reader.GetDateTime("fecha_ingreso"),
                            Intentos = reader.GetInt32("intentos"),
                            Calificacion = reader.GetFloat("puntaje_global"),
                            Estatus = reader.IsDBNull(reader.GetOrdinal("estado")) ? "Indefinido" : reader.GetString("estado")
                        });
                    }
                }
            }
        }

        public string IsActive(string viewName)
        {
            return CurrentView == $"_{viewName}Partial.cshtml" ? "active" : "";
        }
    }

    public class Colaborador
    {
        public int Id { get; set; }
        public string TipoUsuario { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int Intentos { get; set; }
        public float Calificacion { get; set; }
        public string Estatus { get; set; }
    }
}

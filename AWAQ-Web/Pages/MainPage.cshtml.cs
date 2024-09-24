using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using static AWAQ_Web.MainPageModel;
using AWAQ_Web.Pages;

namespace AWAQ_Web
{
    public class MainPageModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public string CurrentView { get; set; } = "_DashboardPartial.cshtml";
        public string ContentTitle { get; set; } = "Dashboard";

        public List<Colaborador> Collaborators { get; set; } = new List<Colaborador>();
        public List<Colaborador> data { get; set; } = new List<Colaborador>();

        public PaginatedList Pager = new PaginatedList();

        public int starpage = 1;
        public Personal info { get; set; } = new Personal();
        public List<Experiencia> exp { get; set; } = new List<Experiencia>();
        public string direccion { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Nombre es requerido")]
        public string Nombre { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Apellidos son requeridos")]
        public string Apellidos { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Email es requerido")]
        public string Email { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Tipo de usuario es requerido")]
        public string TipoUsuario { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Contraseña es requerida")]
        public string Password { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Validación de contraseña es requerida")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string PasswordConfirm { get; set; }

        public MainPageModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool ValidateSession()
        {

            string username = HttpContext.Session.GetString("username");
            if (username == null)
            {
                Console.WriteLine("Nulo");

                return true;

            }
            return false;

        }

        public IActionResult OnGetDashboard()
        {

            if (ValidateSession())
            {
                TempData["ErrorMessage"] = "La sesión ha expirado. Por favor, vuelva a iniciar sesión.";
                return RedirectToPage("/Index");
            }
            CurrentView = "_DashboardPartial.cshtml";
            ContentTitle = "Dashboard";
            return Page();
        }

        public IActionResult OnGetCollaborators(int pg)
        {
            if (ValidateSession())
            {
                return RedirectToPage("/Index");
            }
            LoadCollaborators(pg);
            CurrentView = "_CollaboratorsPartial.cshtml";
            ContentTitle = "Colaboradores";
            return Page();


        }

        public IActionResult OnGetAddCollaborator()
        {
            if (ValidateSession())
            {
                return RedirectToPage("/Index");
            }
            CurrentView = "_AddCollaboratorPartial.cshtml";
            ContentTitle = "Agregar Colaborador";
            return Page();
        }
        public IActionResult OnGetSignOut()
        {

            return RedirectToPage("/Index");
        }

        public IActionResult OnGetProfile() {
            if (ValidateSession())
            {
                return RedirectToPage("/Index");
            }
            CurrentView = "_Profile.cshtml";
            ContentTitle = "Perfil";
            LoadPersonalInfo();
            return Page();
        }

        public void LoadPersonalInfo()
        {

            string connString = _configuration.GetConnectionString("myDb1");
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                var cmd = new MySqlCommand("call ObtenerInformacionPersonal(" + HttpContext.Session.GetInt32("userid").ToString() + ");", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        info = new Personal
                        {
                            personal_id = reader.GetInt32("personal_id"),
                            user_id = reader.GetInt32("usuario_id"),
                            nacimiento = reader.GetDateTime("fecha_nacimiento"),
                            genero = reader.GetString("genero"),
                            calle = reader.GetString("direccion_calle"),
                            ciudad = reader.GetString("direccion_ciudad"),
                            cp = reader.GetString("direccion_codigo_postal"),
                            estado = reader.GetString("direccion_estado"),
                            pais = reader.GetString("pais"),
                            telefono = reader.GetString("telefono"),
                            estado_civil = reader.GetString("estado_civil")
                        };

                    }
                }
            }

            direccion = info.calle + ", " + info.ciudad + ", " + info.estado + ", " + info.cp;
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                var cmd2 = new MySqlCommand("call ObtenerExperienciaLaboral(" + HttpContext.Session.GetInt32("userid").ToString() + ");", conn);
                using (var reader = cmd2.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exp.Add( new Experiencia 
                        {
                            empresa = reader.GetString("empresa"),
                            puesto = reader.GetString("puesto"),
                            fecha_inicio = reader.GetDateTime("fecha_inicio"),
                            fecha_fin = reader.GetDateTime("fecha_fin"),
                            descripcion = reader.GetString("descripcion")
                        });

                    }
                }
            }
        }


        public void LoadCollaborators(int pg)
        {

            string connString = _configuration.GetConnectionString("myDb1");
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                var cmd = new MySqlCommand(@"SELECT u.usuario_id, u.tipo_usuario, u.nombre AS nombre, u.fecha_ingreso, u.estado
                                             
                                             FROM Usuarios u
                                             ", conn);

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
                            Estatus = reader.IsDBNull(reader.GetOrdinal("estado")) ? "Indefinido" : reader.GetString("estado")
                        });
                    }
                }
            }
            const int pageSize = 9;
            if (pg < 1)
                pg = 1;

            int recsCount = Collaborators.Count();
            Pager = new PaginatedList(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            data = Collaborators.Skip(recSkip).Take(Pager.PageSize).ToList();
        }

        public IActionResult OnPostRegisterCollaborator()
        {

            ContentTitle = "Agregar Colaborador";

            if (!ModelState.IsValid)
            {
                CurrentView = "_AddCollaboratorPartial.cshtml";
                return Page();
            }
            else
            {
                try
                {
                    string nombreCompleto = $"{Nombre} {Apellidos}";
                    string connString = _configuration.GetConnectionString("myDb1");
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
                    return Page();



                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al registrar el colaborador: {ex.Message}");
                    return Page();
                }

            }

        }
    }
        
       



        


    
}
public class Personal
{
    public int personal_id { get; set; }
    public int user_id { get; set; }
    public DateTime nacimiento { get; set; }
    public string genero { get; set; }
    public string calle { get; set; }
    public string ciudad { get; set; }
    public string estado { get; set; }
    public string cp { get; set; }
    public string pais { get; set; }
    public string telefono { get; set; }
    public string estado_civil { get; set; }
}

public class Experiencia
{
    public string empresa { get; set; }
    public string puesto { get; set; }

    public DateTime fecha_inicio { get; set; }
    public DateTime fecha_fin { get; set; }
    public string descripcion { get; set; }



}
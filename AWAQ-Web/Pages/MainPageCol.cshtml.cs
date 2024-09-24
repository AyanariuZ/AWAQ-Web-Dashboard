using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AWAQ_Web.Pages
{
    public class MainPageColModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        public string CurrentView1 { get; set; } = "_DashboardCol.cshtml";
        public string ContentTitle1 { get; set; } = "Panel de Control";

        public Personal info { get; set; } = new Personal();
        public List<Experiencia> exp { get; set; } = new List<Experiencia>();
        public List<TemaEstudio> TemasEstudio { get; set; } = new List<TemaEstudio>();
        public string direccion { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Sugerencias es requerido")]
        public string Sugerencias { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "¿Qué se debería quitar? es requerido")]
        public string Quitar { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Preguntas es requerido")]
        public string Preguntas { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Nombre es requerido")]
        public string Nombre { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Correo electrónico no es válido")]
        public string Email { get; set; }

        public MainPageColModel(IConfiguration configuration, IEmailSender emailSender)
        {
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public bool ValidateSession()
        {
            string username = HttpContext.Session.GetString("username");
            if (username == null)
            {
                return true;
            }
            return false;
        }

        public IActionResult OnGetSignOut()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        public IActionResult OnGetDashboard()
        {
            if (ValidateSession())
            {
                TempData["ErrorMessage"] = "La sesión ha expirado. Por favor, vuelva a iniciar sesión.";
                return RedirectToPage("/Index");
            }
            CurrentView1 = "_DashboardCol.cshtml";
            ContentTitle1 = "Dashboard";
            return Page();
        }

        public IActionResult OnGetProfile()
        {
            if (ValidateSession())
            {
                return RedirectToPage("/Index");
            }
            CurrentView1 = "_ProfileCol.cshtml";
            ContentTitle1 = "Perfil";
            LoadPersonalInfo();
            return Page();
        }

        public IActionResult OnGetGuiaTemas()
        {
            if (ValidateSession())
            {
                return RedirectToPage("/Index");
            }
            CurrentView1 = "_GuiaTemasPartial";
            ContentTitle1 = "Guía de Temas para Estudio";
            return Page();
        }

        public IActionResult OnGetInstrucciones()
        {
            if (ValidateSession())
            {
                return RedirectToPage("/Index");
            }
            CurrentView1 = "_InstruccionesPartial";
            ContentTitle1 = "Instrucciones de Juego";
            return Page();
        }

        public IActionResult OnGetFeedback()
        {
            if (ValidateSession())
            {
                return RedirectToPage("/Index");
            }
            CurrentView1 = "_FeedbackPartial";
            ContentTitle1 = "Formulario de Retroalimentación";
            return Page();
        }

        public async Task<IActionResult> OnPostFeedbackAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentView1 = "_FeedbackPartial";
                ContentTitle1 = "Formulario Feedback";
                return Page();
            }

            try
            {
                string connString = _configuration.GetConnectionString("myDb1");
                using (var conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT INTO Feedback (sugerencias, quitar, preguntas, nombre, email) VALUES (@sugerencias, @quitar, @preguntas, @nombre, @correo)", conn);
                    cmd.Parameters.AddWithValue("@sugerencias", Sugerencias);
                    cmd.Parameters.AddWithValue("@quitar", Quitar);
                    cmd.Parameters.AddWithValue("@preguntas", Preguntas);
                    cmd.Parameters.AddWithValue("@nombre", Nombre);
                    cmd.Parameters.AddWithValue("@correo", Email);
                    cmd.ExecuteNonQuery();
                }

                var message = $@"
                    <p>Nombre: {Nombre}</p>
                    <p>Correo: {Email}</p>
                    <p>Sugerencias: {Sugerencias}</p>
                    <p>¿Qué se debería quitar?: {Quitar}</p>
                    <p>Preguntas: {Preguntas}</p>";

                await _emailSender.SendEmailAsync("skulltula.mn35@outlook.com", "Nuevo Feedback Recibido", message);

                TempData["SuccessMessage"] = "Formulario enviado exitosamente.";

                // Limpiar campos después de enviar el formulario
                ModelState.Clear();
                Sugerencias = Quitar = Preguntas = Nombre = Email = string.Empty;

                return RedirectToPage("MainPageCol", new { handler = "Feedback" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al enviar el feedback: {ex.Message}");
                CurrentView1 = "_FeedbackPartial";
                ContentTitle1 = "Formulario Feedback";
                return Page();
            }
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
                        exp.Add(new Experiencia
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

        public void LoadTemasEstudio()
        {
            string connString = _configuration.GetConnectionString("myDb1");
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT titulo, descripcion FROM TemasEstudio WHERE estado = 'activo';", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TemasEstudio.Add(new TemaEstudio
                        {
                            Titulo = reader.GetString("titulo"),
                            Descripcion = reader.GetString("descripcion")
                        });
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

    public class Feedback
    {
        public string Sugerencias { get; set; }
        public string Quitar { get; set; }
        public string Preguntas { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
    }

    public class TemaEstudio
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography.X509Certificates;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace AWAQ_Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Correo electrónico requerido")]
        public string email { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Contraseña requerida")]
        public string password { get; set; }
        public string ErrorMessage { get; set; }
        private int id { get; set; }

        public string puesto { get; set; }
        public bool valid { get; set; }
        public IList<Usuario> Usuarios { get; set; }

        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool ValidateUser()
        {
            string username;
            string connString = _configuration.GetConnectionString("myDb1");
            MySqlConnection conn = new MySqlConnection(connString);
            conn.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT email,password,nombre,usuario_id,tipo_usuario FROM usuarios";

            Usuario usuario1 = new Usuario();

            Usuarios = new List<Usuario>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    usuario1 = new Usuario();
                    usuario1.email = reader["email"].ToString();
                    usuario1.password = reader["password"].ToString();
                    usuario1.nombre = reader["nombre"].ToString();
                    usuario1.usuario_id = (int)reader["usuario_id"];
                    usuario1.tipo_usuario = reader["tipo_usuario"].ToString();
                    Usuarios.Add(usuario1);
                }
            }
            conn.Dispose();
            foreach (var usuario in Usuarios)
            {
                username = usuario.nombre;
                id = usuario.usuario_id;
                puesto = usuario.tipo_usuario;
                Console.WriteLine(username);


                if (email == usuario.email && password == usuario.password)
                {
                    
                    HttpContext.Session.SetString("username", username);
                    HttpContext.Session.SetInt32("userid", id);
                    HttpContext.Session.SetString("puesto", usuario.tipo_usuario);
                    HttpContext.Session.SetString("email", usuario.email);

                    return true;
                }
            }
            
            return false;
        }
        public IActionResult OnPost()
        {


            if (ValidateUser())
            {

                HttpContext.Session.SetString("usuario", "username");
                if(puesto == "admin")
                {
                    return RedirectToPage("/MainPage");
                }
                else
                {
                    return RedirectToPage("/MainPageCol");
                }
                
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Tu correo o contraseña no son correctas.");
                return Page();
            }
            
            
        }
        public void OnGet()
        {
            if (TempData.ContainsKey("ErrorMessage"))
            {
                ErrorMessage = TempData["ErrorMessage"].ToString();
            }
        }
        
}
}


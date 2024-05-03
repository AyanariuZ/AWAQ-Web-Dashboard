using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AWAQ_Web.Pages
{
    public class RegistrooModel : PageModel
    {
        [BindProperty]
        public string nombre { get; set; }

        [BindProperty]
        public string apellidos { get; set; }

        [BindProperty]
        public string email { get; set; }

        [BindProperty]
        public string tipo_usuario { get; set; }

        [BindProperty]
        public string password { get; set; }
        [BindProperty]
        public string password2 { get; set; }

        public string nombreyap { get; set; }


        public IActionResult OnPost()
        {

            try
            {
                nombreyap = nombre + ' ' + apellidos;
                string connString = "Server=127.0.0.1;Port=3306;Database=awak;Uid=root;password=0906alex;";
                MySqlConnection conn = new MySqlConnection(connString);
                conn.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO awak.usuarios (nombre, email, tipo_usuario, password) VALUES(@nombre,@email,@tipo_usuario,@password);";
                cmd.Parameters.AddWithValue("@nombre", nombreyap);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@tipo_usuario", tipo_usuario);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.ExecuteNonQuery();
                conn.Dispose();
            }

            catch (System.Exception)
            {
                Console.WriteLine("Error en la conexión a la base de datos");
              
            }
            

            return Page();
        }
    }
}

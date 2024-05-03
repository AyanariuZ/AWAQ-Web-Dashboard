using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography.X509Certificates;
using MySql.Data.MySqlClient;

namespace AWAQ_Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string email { get; set; }
        [BindProperty]
        public string password { get; set; }

        public bool valid { get; set; }
        public IList<Usuario> Usuarios { get; set; }
        public IActionResult OnPost()
        {
            string connString = "Server=127.0.0.1;Port=3306;Database=awak;Uid=root;password=0906alex;";
            MySqlConnection conn = new MySqlConnection(connString);
            conn.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT email,password FROM usuarios WHERE tipo_usuario = 'ADMIN' AND estado='activo'";

            Usuario usuario1 = new Usuario();

            Usuarios = new List<Usuario>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    usuario1 = new Usuario();
                    usuario1.email = reader["email"].ToString();
                    usuario1.password = reader["password"].ToString();
                    Usuarios.Add(usuario1);
                }
            }
            conn.Dispose();

            foreach (var usuario in Usuarios)
            {
                if (email == usuario.email && password == usuario.password)
                {
                    return RedirectToPage("/pruebalogin");
                }
            }
            return Page();
        }
    }
}

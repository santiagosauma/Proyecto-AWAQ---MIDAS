using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace midas.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "El campo de correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar una dirección de correo válida.")]
        public string? Mail { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "El campo de contraseña es obligatorio.")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.", MinimumLength = 6)]
        public string? Password { get; set; }

        public string? Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = "Entrada inválida. Por favor, inténtalo de nuevo.";
                return Page();
            }

            string connectionString = "server=localhost;user=root;database=wildwatch;port=3306;password=";
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT ID_Usuario, Is_Admin FROM Usuario WHERE email = @Mail AND clave = SHA2(@Password, 256)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Mail", Mail);
                        command.Parameters.AddWithValue("@Password", Password);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                var userId = reader["ID_Usuario"].ToString();
                                byte[] isAdminBytes = (byte[])reader["Is_Admin"];
                                bool isAdmin = isAdminBytes[0] == 49;  // 49 is ASCII for '1'

                                if (isAdmin)
                                {
                                    return RedirectToPage("/Admin/General");
                                }
                                else
                                {
                                    return RedirectToPage("/User", new { id = userId });
                                }
                            }
                            else
                            {
                                Message = "Correo electrónico o contraseña incorrectos.";
                                return Page();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Message = $"Error al conectar con la base de datos: {ex.Message}";
                return Page();
            }
        }
    }
}

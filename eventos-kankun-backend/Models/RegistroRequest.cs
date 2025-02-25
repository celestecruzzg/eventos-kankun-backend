public class RegistroRequest
{
    public string Nombre { get; set; }
    public string ApPaterno { get; set; }
    public string ApMaterno { get; set; }
    public string Email { get; set; }
    public string Contrasena { get; set; }
    public string TipoUsuario { get; set; } // "Participante" o "Administrador"
}
public class ValidarCodigoRequest
{
    public string Email { get; set; }
    public string Codigo { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Contrasena { get; set; }
}
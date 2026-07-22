namespace BackendApi.Models;

public class Proveedor
{
    public int Id { get; set; }
    public string NombreComercial { get; set; } = string.Empty;
    public string Rubro { get; set; } = string.Empty;
    public string InsumoPrincipal { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
}
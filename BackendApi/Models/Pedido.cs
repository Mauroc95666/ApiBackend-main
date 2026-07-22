namespace BackendApi.Models;

public class Pedido
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double Total { get; set; }
    public string Estado { get; set; } = "Pendiente";
}

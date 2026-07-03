using ClosedXML.Excel;
using Dynamics_Leads.Application.Export;

namespace Dynamics_Leads.Infrastructure.Export;

/// <summary>Implementación de <see cref="IExcelExporter"/> con ClosedXML.</summary>
public sealed class ClosedXmlExcelExporter : IExcelExporter
{
    private static readonly string[] ColumnasReservadas = ["leadId", "formulario", "fechaCreacion"];

    public byte[] ExportarLeads(IReadOnlyList<IDictionary<string, object?>> leads)
    {
        // Columnas: reservadas primero, luego los campos dinámicos en orden de aparición.
        var columnas = new List<string>(ColumnasReservadas);
        var vistas = new HashSet<string>(columnas, StringComparer.Ordinal);
        foreach (var fila in leads)
        {
            foreach (var clave in fila.Keys)
            {
                if (vistas.Add(clave))
                {
                    columnas.Add(clave);
                }
            }
        }

        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Leads");

        // Encabezados.
        for (var c = 0; c < columnas.Count; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = columnas[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
        }

        // Filas.
        for (var r = 0; r < leads.Count; r++)
        {
            var fila = leads[r];
            for (var c = 0; c < columnas.Count; c++)
            {
                if (!fila.TryGetValue(columnas[c], out var valor) || valor is null)
                {
                    continue;
                }

                var cell = ws.Cell(r + 2, c + 1);
                switch (valor)
                {
                    case DateTime dt:
                        cell.Value = dt;
                        cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                        break;
                    case bool b:
                        cell.Value = b;
                        break;
                    case byte or short or int or long or float or double or decimal:
                        cell.Value = Convert.ToDouble(valor);
                        break;
                    default:
                        cell.Value = valor.ToString();
                        break;
                }
            }
        }

        ws.Row(1).SetAutoFilter();
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}

namespace SocietiesManagementSystem.Helpers;

public static class CsvExportHelper
{
    public static void ExportDataGridView(DataGridView grid, string path)
    {
        using var w = new StreamWriter(path);
        var cols = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();
        w.WriteLine(string.Join(",", cols.Select(c => Quote(c.HeaderText))));
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.IsNewRow) continue;
            w.WriteLine(string.Join(",", cols.Select(c =>
                Quote(row.Cells[c.Index].FormattedValue?.ToString() ?? ""))));
        }
    }

    static string Quote(string s) => $"\"{s.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}

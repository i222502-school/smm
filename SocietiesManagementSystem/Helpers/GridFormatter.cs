using System.Data;

namespace SocietiesManagementSystem.Helpers;

public static class GridFormatter
{
    public static void Bind(DataGridView grid, DataTable? table)
    {
        grid.DataSource = null;
        grid.AutoGenerateColumns = true;
        grid.DataSource = table;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
    }

    public static void BindEditable(DataGridView grid, DataTable? table)
    {
        grid.DataSource = null;
        grid.AutoGenerateColumns = true;
        grid.DataSource = table;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        grid.ReadOnly = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    }
}

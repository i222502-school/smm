using System.Data;
using SocietiesManagementSystem.Data;
using SocietiesManagementSystem.Helpers;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Forms;

/// <summary>Approved member workspace: roster maintenance (view), tasks, announcements.</summary>
public class MemberSocietyForm : Form
{
    readonly SessionUser _user;
    readonly int _societyId;

    readonly DataGridView _roster = new() { Dock = DockStyle.Fill };
    readonly DataGridView _tasks = new() { Dock = DockStyle.Fill };
    readonly DataGridView _announce = new() { Dock = DockStyle.Fill };

    public MemberSocietyForm(SessionUser user, int societyId)
    {
        _user = user;
        _societyId = societyId;

        if (!SocietyRepository.IsApprovedMemberOf(user.UserId, societyId))
        {
            MessageBox.Show("You are not an approved member of this society.");
            Close();
            return;
        }

        Text = "Society member workspace";
        Width = 920;
        Height = 640;
        StartPosition = FormStartPosition.CenterParent;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var t1 = new TabPage("Member list");
        t1.Controls.Add(_roster);
        var t2 = new TabPage("My tasks in this society");
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 8, Top = 10, Width = 130 };
        cmb.Items.AddRange(new object[] { "Pending", "InProgress", "Completed" });
        cmb.SelectedIndex = 0;
        var btn = new Button { Text = "Update selected task status", Left = 160, Top = 8 };
        btn.Click += (_, _) =>
        {
            if (_tasks.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            var tid = (int)rv.Row["TaskID"];
            TaskRepository.UpdateTaskStatus(tid, _user.UserId, cmb.SelectedItem!.ToString()!);
            ReloadTasks();
        };
        panel.Controls.AddRange(new Control[] { cmb, btn });
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        host.Controls.Add(_tasks, 0, 0);
        host.Controls.Add(panel, 0, 1);
        t2.Controls.Add(host);
        var t3 = new TabPage("Announcements");
        t3.Controls.Add(_announce);

        tabs.TabPages.Add(t1);
        tabs.TabPages.Add(t2);
        tabs.TabPages.Add(t3);
        Controls.Add(tabs);

        ReloadRoster();
        ReloadTasks();
        ReloadAnnounce();
    }

    void ReloadRoster() => GridFormatter.Bind(_roster, SocietyRepository.GetMemberRoster(_societyId));

    void ReloadTasks()
    {
        var all = TaskRepository.GetTasksForSociety(_societyId);
        var filtered = all.Clone();
        foreach (DataRow row in all.Rows)
        {
            if ((int)row["AssignedToUserID"] == _user.UserId)
                filtered.ImportRow(row);
        }

        GridFormatter.Bind(_tasks, filtered);
        if (_tasks.Columns.Contains("AssignedToUserID"))
            _tasks.Columns["AssignedToUserID"]!.Visible = false;
    }

    void ReloadAnnounce() => GridFormatter.Bind(_announce, AnnouncementRepository.GetForSociety(_societyId));
}

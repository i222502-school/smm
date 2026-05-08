using System.Data;
using SocietiesManagementSystem.Data;
using SocietiesManagementSystem.Helpers;
using SocietiesManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Forms;

public class SocietyLeadershipForm : Form
{
    readonly SessionUser _user;
    readonly int _societyId;

    readonly TextBox _txtName = new() { Width = 320 };
    readonly TextBox _txtDesc = new() { Width = 520, Height = 100, Multiline = true };
    readonly DataGridView _pending = new() { Dock = DockStyle.Fill };
    readonly DataGridView _roster = new() { Dock = DockStyle.Fill };
    readonly DataGridView _events = new() { Dock = DockStyle.Fill };
    readonly DataGridView _tasks = new() { Dock = DockStyle.Fill };
    readonly DataGridView _announce = new() { Dock = DockStyle.Fill };

    readonly TextBox _evTitle = new() { Width = 220 };
    readonly TextBox _evVenue = new() { Width = 180 };
    readonly TextBox _evDesc = new() { Width = 520, Height = 60, Multiline = true };
    readonly DateTimePicker _evStart = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", ShowUpDown = false, Width = 180 };
    readonly DateTimePicker _evEnd = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", ShowUpDown = false, Width = 180 };
    readonly NumericUpDown _evMax = new() { Minimum = 0, Maximum = 100000, Width = 80 };

    readonly ComboBox _taskAssignee = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly TextBox _taskTitle = new() { Width = 240 };
    readonly TextBox _taskDesc = new() { Width = 400, Height = 50, Multiline = true };
    readonly DateTimePicker _taskDue = new() { ShowCheckBox = true, Checked = false, Width = 180 };

    readonly TextBox _anTitle = new() { Width = 260 };
    readonly TextBox _anBody = new() { Width = 520, Height = 70, Multiline = true };

    public SocietyLeadershipForm(SessionUser user, int societyId)
    {
        _user = user;
        _societyId = societyId;

        if (!SocietyRepository.IsHeadOf(user.UserId, societyId))
        {
            MessageBox.Show("You are not the approved head for this society.");
            Close();
            return;
        }

        Text = "Society leadership";
        Width = 980;
        Height = 720;
        StartPosition = FormStartPosition.CenterParent;

        var tabs = new TabControl { Dock = DockStyle.Fill };

        tabs.TabPages.Add(BuildProfileTab());
        tabs.TabPages.Add(BuildMembershipTab());
        tabs.TabPages.Add(BuildMembersTab());
        tabs.TabPages.Add(BuildEventsTab());
        tabs.TabPages.Add(BuildTasksTab());
        tabs.TabPages.Add(BuildAnnounceTab());
        tabs.TabPages.Add(BuildReportsTab());

        Controls.Add(tabs);

        _taskAssignee.DisplayMember = "Display";

        LoadProfile();
        ReloadGrids();
        LoadAssignees();
    }

    void LoadProfile()
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var cmd = new SqlCommand(
            "SELECT Name, Description FROM dbo.Societies WHERE SocietyID=@id", cn);
        cmd.Parameters.AddWithValue("@id", _societyId);
        using var r = cmd.ExecuteReader();
        if (r.Read())
        {
            _txtName.Text = r.GetString(0);
            _txtDesc.Text = r.GetString(1);
        }
    }

    TabPage BuildProfileTab()
    {
        var tab = new TabPage("Profile");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2 };
        layout.Controls.Add(new Label { Text = "Name", AutoSize = true }, 0, 0);
        layout.Controls.Add(_txtName, 1, 0);
        layout.Controls.Add(new Label { Text = "Description", AutoSize = true }, 0, 1);
        layout.Controls.Add(_txtDesc, 1, 1);
        var btn = new Button { Text = "Save profile", AutoSize = true };
        btn.Click += (_, _) =>
        {
            SocietyRepository.UpdateSocietyProfile(_societyId, _txtName.Text, _txtDesc.Text, _user.UserId);
            MessageBox.Show("Saved.");
        };
        layout.Controls.Add(btn, 1, 2);
        tab.Controls.Add(layout);
        return tab;
    }

    TabPage BuildMembershipTab()
    {
        var tab = new TabPage("Approve membership requests");
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var approve = new Button { Text = "Approve", Dock = DockStyle.Right, Width = 100 };
        var reject = new Button { Text = "Reject", Dock = DockStyle.Right, Width = 100 };
        approve.Click += (_, _) => ResolvePending(true);
        reject.Click += (_, _) => ResolvePending(false);
        panel.Controls.Add(reject);
        panel.Controls.Add(approve);
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        host.Controls.Add(_pending, 0, 0);
        host.Controls.Add(panel, 0, 1);
        tab.Controls.Add(host);
        return tab;
    }

    void ResolvePending(bool approve)
    {
        if (_pending.CurrentRow?.DataBoundItem is not DataRowView rv) return;
        var mid = (int)rv.Row["MembershipID"];
        SocietyRepository.ResolveMembership(mid, approve, _user.UserId);
        ReloadGrids();
        LoadAssignees();
    }

    TabPage BuildMembersTab()
    {
        var tab = new TabPage("Member roster");
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var remove = new Button { Text = "Remove selected member", Dock = DockStyle.Right, Width = 200 };
        remove.Click += (_, _) =>
        {
            if (_roster.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            if (rv.Row["RoleInSociety"].ToString() == "Head")
            {
                MessageBox.Show("Cannot remove society head.");
                return;
            }
            var memberUserId = (int)rv.Row["UserID"];
            if (MessageBox.Show("Remove this member?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            SocietyRepository.RemoveMemberFromSociety(_societyId, memberUserId, _user.UserId);
            ReloadGrids();
            LoadAssignees();
        };
        panel.Controls.Add(remove);
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        host.Controls.Add(_roster, 0, 0);
        host.Controls.Add(panel, 0, 1);
        tab.Controls.Add(host);
        return tab;
    }

    TabPage BuildEventsTab()
    {
        var tab = new TabPage("Events");
        var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 220, ColumnCount = 2, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Title", AutoSize = true }, 0, 0);
        top.Controls.Add(_evTitle, 1, 0);
        top.Controls.Add(new Label { Text = "Venue", AutoSize = true }, 0, 1);
        top.Controls.Add(_evVenue, 1, 1);
        top.Controls.Add(new Label { Text = "Description", AutoSize = true }, 0, 2);
        top.Controls.Add(_evDesc, 1, 2);
        top.Controls.Add(new Label { Text = "Start", AutoSize = true }, 0, 3);
        top.Controls.Add(_evStart, 1, 3);
        top.Controls.Add(new Label { Text = "End", AutoSize = true }, 0, 4);
        top.Controls.Add(_evEnd, 1, 4);
        top.Controls.Add(new Label { Text = "Max participants (0 = unlimited)", AutoSize = true }, 0, 5);
        top.Controls.Add(_evMax, 1, 5);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
        var addDraft = new Button { Text = "Add as draft" };
        var submit = new Button { Text = "Add & submit for admin approval" };
        var saveEdit = new Button { Text = "Update selected" };
        var cancelEv = new Button { Text = "Cancel selected event" };

        addDraft.Click += (_, _) => AddEvent(false);
        submit.Click += (_, _) => AddEvent(true);
        saveEdit.Click += (_, _) => UpdateSelectedEvent();
        cancelEv.Click += (_, _) => CancelSelectedEvent();

        buttons.Controls.AddRange(new Control[] { addDraft, submit, saveEdit, cancelEv });

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 230));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(buttons, 0, 1);
        layout.Controls.Add(_events, 0, 2);
        _events.Dock = DockStyle.Fill;
        tab.Controls.Add(layout);
        return tab;
    }

    void AddEvent(bool submitForApproval)
    {
        int? max = _evMax.Value == 0 ? null : (int)_evMax.Value;
        EventRepository.CreateEvent(_societyId, _user.UserId, _evTitle.Text, _evDesc.Text, _evVenue.Text,
            _evStart.Value, _evEnd.Value, max, submitForApproval);
        ReloadGrids();
        MessageBox.Show(submitForApproval ? "Submitted for admin approval." : "Draft saved.");
    }

    void UpdateSelectedEvent()
    {
        if (_events.CurrentRow?.DataBoundItem is not DataRowView rv) return;
        var eid = (int)rv.Row["EventID"];
        var status = rv.Row["Status"].ToString()!;
        int? max = _evMax.Value == 0 ? null : (int)_evMax.Value;
        EventRepository.UpdateEvent(eid, _societyId, _evTitle.Text, _evDesc.Text, _evVenue.Text,
            _evStart.Value, _evEnd.Value, max, status);
        ReloadGrids();
        MessageBox.Show("Event updated.");
    }

    void CancelSelectedEvent()
    {
        if (_events.CurrentRow?.DataBoundItem is not DataRowView rv) return;
        var eid = (int)rv.Row["EventID"];
        EventRepository.CancelEvent(eid, _societyId, _user.UserId);
        ReloadGrids();
    }

    TabPage BuildTasksTab()
    {
        var tab = new TabPage("Tasks");
        var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 140, ColumnCount = 2, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Assign to", AutoSize = true }, 0, 0);
        top.Controls.Add(_taskAssignee, 1, 0);
        top.Controls.Add(new Label { Text = "Title", AutoSize = true }, 0, 1);
        top.Controls.Add(_taskTitle, 1, 1);
        top.Controls.Add(new Label { Text = "Description", AutoSize = true }, 0, 2);
        top.Controls.Add(_taskDesc, 1, 2);
        top.Controls.Add(new Label { Text = "Due", AutoSize = true }, 0, 3);
        top.Controls.Add(_taskDue, 1, 3);

        var btn = new Button { Text = "Assign task", Dock = DockStyle.Top, Height = 32 };
        btn.Click += (_, _) =>
        {
            if (_taskAssignee.SelectedItem is not MemberPick mp) return;
            DateTime? due = _taskDue.Checked ? _taskDue.Value : null;
            TaskRepository.AddTask(_societyId, _user.UserId, mp.UserId, _taskTitle.Text, _taskDesc.Text, due);
            ReloadGrids();
            MessageBox.Show("Task assigned.");
        };

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(btn, 0, 1);
        layout.Controls.Add(_tasks, 0, 2);
        _tasks.Dock = DockStyle.Fill;
        tab.Controls.Add(layout);
        return tab;
    }

    sealed record MemberPick(int UserId, string Display);

    void LoadAssignees()
    {
        _taskAssignee.Items.Clear();
        var dt = SocietyRepository.GetMemberRoster(_societyId);
        foreach (DataRow row in dt.Rows)
        {
            if (row["Status"].ToString() != "Approved") continue;
            var uid = (int)row["UserID"];
            if (row["RoleInSociety"].ToString() == "Head" || row["RoleInSociety"].ToString() == "Member")
                _taskAssignee.Items.Add(new MemberPick(uid, $"{row["FullName"]} ({row["Username"]})"));
        }
        if (_taskAssignee.Items.Count > 0) _taskAssignee.SelectedIndex = 0;
    }

    TabPage BuildAnnounceTab()
    {
        var tab = new TabPage("Announcements");
        var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 140, ColumnCount = 2, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Title", AutoSize = true }, 0, 0);
        top.Controls.Add(_anTitle, 1, 0);
        top.Controls.Add(new Label { Text = "Body", AutoSize = true }, 0, 1);
        top.Controls.Add(_anBody, 1, 1);
        var btn = new Button { Text = "Post to society", Height = 28 };
        btn.Click += (_, _) =>
        {
            AnnouncementRepository.Post(_societyId, _user.UserId, _anTitle.Text, _anBody.Text);
            _anTitle.Clear();
            _anBody.Clear();
            ReloadGrids();
        };
        top.Controls.Add(btn, 1, 2);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_announce, 0, 1);
        _announce.Dock = DockStyle.Fill;
        tab.Controls.Add(layout);
        return tab;
    }

    TabPage BuildReportsTab()
    {
        var tab = new TabPage("Reports");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44 };
        var members = new Button { Text = "Members report" };
        var events = new Button { Text = "Events report" };
        var grid = new DataGridView { Dock = DockStyle.Fill };
        members.Click += (_, _) => GridFormatter.Bind(grid, SocietyRepository.GetMemberRoster(_societyId));
        events.Click += (_, _) => GridFormatter.Bind(grid, EventRepository.GetSocietyEventsReport(_societyId));
        var export = new Button { Text = "Export grid to CSV..." };
        export.Click += (_, _) =>
        {
            using var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "report.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
                CsvExportHelper.ExportDataGridView(grid, sfd.FileName);
        };
        panel.Controls.AddRange(new Control[] { members, events, export });
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(panel, 0, 0);
        layout.Controls.Add(grid, 0, 1);
        grid.Dock = DockStyle.Fill;
        tab.Controls.Add(layout);
        return tab;
    }

    void ReloadGrids()
    {
        GridFormatter.Bind(_pending, SocietyRepository.GetPendingMembershipsForSociety(_societyId));
        GridFormatter.Bind(_roster, SocietyRepository.GetMemberRoster(_societyId));
        GridFormatter.Bind(_events, EventRepository.GetEventsForSociety(_societyId));
        GridFormatter.Bind(_tasks, TaskRepository.GetTasksForSociety(_societyId));
        HideColumn(_tasks, "AssignedToUserID");
        GridFormatter.Bind(_announce, AnnouncementRepository.GetForSociety(_societyId));
    }

    static void HideColumn(DataGridView g, string name)
    {
        if (g.Columns.Contains(name))
            g.Columns[name]!.Visible = false;
    }
}

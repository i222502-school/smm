using SocietiesManagementSystem.Data;
using SocietiesManagementSystem.Helpers;

namespace SocietiesManagementSystem.Forms;

public class AdminDashboardForm : Form
{
    readonly TabControl _tabs = new() { Dock = DockStyle.Fill };

    readonly DataGridView _users = new() { Dock = DockStyle.Fill };
    readonly TextBox _uEmail = new() { Width = 260 };
    readonly TextBox _uName = new() { Width = 260 };
    readonly CheckBox _uActive = new() { Text = "Active", AutoSize = true };
    int? _selectedUserId;

    readonly DataGridView _societies = new() { Dock = DockStyle.Fill };

    readonly DataGridView _pendingEvents = new() { Dock = DockStyle.Fill };

    readonly DataGridView _activity = new() { Dock = DockStyle.Fill };

    readonly DataGridView _report = new() { Dock = DockStyle.Fill };
    readonly TextBox _bcTitle = new() { Width = 280 };
    readonly TextBox _bcBody = new() { Width = 560, Height = 80, Multiline = true };

    public AdminDashboardForm()
    {
        Text = $"Administration — {AppSession.CurrentUser!.Username}";
        Width = 1100;
        Height = 760;
        StartPosition = FormStartPosition.CenterScreen;

        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("File");
        fileMenu.DropDownItems.Add("Log out", null, (_, _) => Close());
        menu.Items.Add(fileMenu);
        MainMenuStrip = menu;
        var strip = new ToolStrip();
        var refresh = new ToolStripButton("Refresh all");
        refresh.Click += (_, _) => ReloadAll();
        strip.Items.Add(refresh);

        Controls.Add(_tabs);
        Controls.Add(strip);
        Controls.Add(menu);
        strip.Dock = DockStyle.Top;
        menu.Dock = DockStyle.Top;

        _tabs.TabPages.Add(BuildUsersTab());
        _tabs.TabPages.Add(BuildSocietiesTab());
        _tabs.TabPages.Add(BuildEventsTab());
        _tabs.TabPages.Add(BuildActivityTab());
        _tabs.TabPages.Add(BuildReportsTab());

        ReloadAll();
    }

    TabPage BuildUsersTab()
    {
        var tab = new TabPage("Students & accounts");
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };
        split.Panel1.Controls.Add(_users);
        _users.SelectionChanged += (_, _) =>
        {
            if (_users.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            _selectedUserId = (int)rv.Row["UserID"];
            _uEmail.Text = rv.Row["Email"].ToString()!;
            _uName.Text = rv.Row["FullName"].ToString()!;
            _uActive.Checked = (bool)rv.Row["IsActive"];
        };

        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
        panel.Controls.Add(new Label { Text = "Email", AutoSize = true }, 0, 0);
        panel.Controls.Add(_uEmail, 1, 0);
        panel.Controls.Add(new Label { Text = "Full name", AutoSize = true }, 0, 1);
        panel.Controls.Add(_uName, 1, 1);
        panel.Controls.Add(_uActive, 1, 2);
        var save = new Button { Text = "Save changes", AutoSize = true };
        save.Click += (_, _) =>
        {
            if (_selectedUserId is null) return;
            var uid = _selectedUserId.Value;
            try
            {
                UserAdminRepository.UpdateUser(AppSession.CurrentUser!.UserId, uid, _uEmail.Text.Trim(), _uName.Text.Trim(), _uActive.Checked);
                MessageBox.Show("User updated.");
                ReloadUsers();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        panel.Controls.Add(save, 1, 3);

        split.Panel2.Controls.Add(panel);
        tab.Controls.Add(split);
        return tab;
    }

    TabPage BuildSocietiesTab()
    {
        var tab = new TabPage("Societies oversight");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48 };
        void SetStatus(string status)
        {
            if (_societies.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            var sid = (int)rv.Row["SocietyID"];
            SocietyRepository.AdminSetSocietyStatus(sid, status, AppSession.CurrentUser!.UserId);
            ReloadSocieties();
        }

        var approve = new Button { Text = "Approve selected" };
        var suspend = new Button { Text = "Suspend selected" };
        var reject = new Button { Text = "Reject selected" };
        var delete = new Button { Text = "Delete selected society" };
        approve.Click += (_, _) => SetStatus("Approved");
        suspend.Click += (_, _) => SetStatus("Suspended");
        reject.Click += (_, _) => SetStatus("Rejected");
        delete.Click += (_, _) =>
        {
            if (_societies.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            if (MessageBox.Show("Delete society and related data?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            SocietyRepository.AdminDeleteSociety((int)rv.Row["SocietyID"], AppSession.CurrentUser!.UserId);
            ReloadSocieties();
        };
        panel.Controls.AddRange(new Control[] { approve, suspend, reject, delete });

        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        host.Controls.Add(_societies, 0, 0);
        host.Controls.Add(panel, 0, 1);
        tab.Controls.Add(host);
        return tab;
    }

    TabPage BuildEventsTab()
    {
        var tab = new TabPage("Pending event approvals");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44 };
        var approve = new Button { Text = "Approve selected event" };
        var reject = new Button { Text = "Reject (cancel) selected event" };
        approve.Click += (_, _) =>
        {
            if (_pendingEvents.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            EventRepository.AdminApproveEvent((int)rv.Row["EventID"], AppSession.CurrentUser!.UserId, true);
            ReloadPendingEvents();
        };
        reject.Click += (_, _) =>
        {
            if (_pendingEvents.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            EventRepository.AdminApproveEvent((int)rv.Row["EventID"], AppSession.CurrentUser!.UserId, false);
            ReloadPendingEvents();
        };
        panel.Controls.AddRange(new Control[] { approve, reject });
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        host.Controls.Add(_pendingEvents, 0, 0);
        host.Controls.Add(panel, 0, 1);
        tab.Controls.Add(host);
        return tab;
    }

    TabPage BuildActivityTab()
    {
        var tab = new TabPage("Activity monitoring");
        tab.Controls.Add(_activity);
        return tab;
    }

    TabPage BuildReportsTab()
    {
        var tab = new TabPage("University-wide reports & broadcast");
        var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 180, ColumnCount = 2, Padding = new Padding(8) };
        top.Controls.Add(new Label { Text = "Broadcast title (all students)", AutoSize = true }, 0, 0);
        top.Controls.Add(_bcTitle, 1, 0);
        top.Controls.Add(new Label { Text = "Broadcast body", AutoSize = true }, 0, 1);
        top.Controls.Add(_bcBody, 1, 1);
        var send = new Button { Text = "Post university announcement" };
        send.Click += (_, _) =>
        {
            AnnouncementRepository.Post(null, AppSession.CurrentUser!.UserId, _bcTitle.Text, _bcBody.Text);
            _bcTitle.Clear();
            _bcBody.Clear();
            MessageBox.Show("Posted.");
        };
        top.Controls.Add(send, 1, 2);

        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
        var load = new Button { Text = "Load summary report" };
        var export = new Button { Text = "Export report to CSV..." };
        load.Click += (_, _) => GridFormatter.Bind(_report, EventRepository.GetUniversityReport());
        export.Click += (_, _) =>
        {
            using var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "university_report.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
                CsvExportHelper.ExportDataGridView(_report, sfd.FileName);
        };
        panel.Controls.AddRange(new Control[] { load, export });

        var mid = new Panel { Dock = DockStyle.Fill };
        mid.Controls.Add(_report);
        mid.Controls.Add(panel);
        mid.Controls.Add(top);
        _report.Dock = DockStyle.Fill;
        panel.Dock = DockStyle.Top;
        top.Dock = DockStyle.Top;
        tab.Controls.Add(mid);
        return tab;
    }

    void ReloadAll()
    {
        ReloadUsers();
        ReloadSocieties();
        ReloadPendingEvents();
        ReloadActivity();
        GridFormatter.Bind(_report, EventRepository.GetUniversityReport());
    }

    void ReloadUsers() => GridFormatter.Bind(_users, UserAdminRepository.GetAllUsers());
    void ReloadSocieties() => GridFormatter.Bind(_societies, SocietyRepository.GetSocietiesForAdmin());
    void ReloadPendingEvents() => GridFormatter.Bind(_pendingEvents, EventRepository.GetPendingEventsForAdmin());
    void ReloadActivity() => GridFormatter.Bind(_activity, ActivityLogQueryRepository.GetRecent());
}

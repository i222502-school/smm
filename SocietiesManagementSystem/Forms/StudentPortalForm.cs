using System.Data;
using SocietiesManagementSystem.Data;
using SocietiesManagementSystem.Helpers;

namespace SocietiesManagementSystem.Forms;

public class StudentPortalForm : Form
{
    readonly TabControl _tabs = new() { Dock = DockStyle.Fill };

    readonly DataGridView _gridBrowse = new() { Dock = DockStyle.Fill };
    readonly DataGridView _gridMemberships = new() { Dock = DockStyle.Fill };
    readonly DataGridView _gridEvents = new() { Dock = DockStyle.Fill };
    readonly DataGridView _gridTickets = new() { Dock = DockStyle.Fill };
    readonly DataGridView _gridAnnounce = new() { Dock = DockStyle.Fill };
    readonly DataGridView _gridMyTasks = new() { Dock = DockStyle.Fill };

    readonly TextBox _txtSocName = new() { Width = 260 };
    readonly TextBox _txtSocDesc = new() { Width = 460, Multiline = true, Height = 80 };

    public StudentPortalForm()
    {
        Text = $"Student portal — {AppSession.CurrentUser!.FullName}";
        Width = 1024;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("File");
        file.DropDownItems.Add("Log out", null, (_, _) => Close());
        menu.Items.Add(file);
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

        BuildBrowseTab();
        BuildRequestTab();
        BuildMembershipsTab();
        BuildEventsTab();
        BuildTicketsTab();
        BuildAnnounceTab();
        BuildMyTasksTab();

        ReloadAll();
    }

    void BuildBrowseTab()
    {
        var tab = new TabPage("Browse societies");
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var btn = new Button { Text = "Apply for membership", Dock = DockStyle.Right, Width = 180 };
        btn.Click += (_, _) =>
        {
            var u = AppSession.CurrentUser!;
            if (_gridBrowse.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            var id = (int)rv.Row["SocietyID"];
            try
            {
                SocietyRepository.ApplyMembership(u.UserId, id, "Member");
                MessageBox.Show("Application submitted (pending head approval).");
                ReloadMemberships();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        panel.Controls.Add(btn);
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        host.Controls.Add(_gridBrowse, 0, 0);
        host.Controls.Add(panel, 0, 1);
        tab.Controls.Add(host);
        _tabs.TabPages.Add(tab);
    }

    void BuildRequestTab()
    {
        var tab = new TabPage("Request new society");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.Controls.Add(new Label { Text = "Society name", AutoSize = true }, 0, 0);
        layout.Controls.Add(_txtSocName, 1, 0);
        layout.Controls.Add(new Label { Text = "Description", AutoSize = true }, 0, 1);
        layout.Controls.Add(_txtSocDesc, 1, 1);
        var btn = new Button { Text = "Submit request (pending admin approval)", AutoSize = true };
        btn.Click += (_, _) =>
        {
            var u = AppSession.CurrentUser!;
            try
            {
                SocietyRepository.CreateSocietyRequest(u.UserId, _txtSocName.Text, _txtSocDesc.Text);
                MessageBox.Show("Society request submitted.");
                _txtSocName.Clear();
                _txtSocDesc.Clear();
                ReloadBrowse();
                ReloadMemberships();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        layout.Controls.Add(btn, 1, 2);
        tab.Controls.Add(layout);
        _tabs.TabPages.Add(tab);
    }

    void BuildMembershipsTab()
    {
        var tab = new TabPage("My memberships");
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var btn = new Button { Text = "Open society workspace", Dock = DockStyle.Right, Width = 220 };
        btn.Click += (_, _) => OpenWorkspace();
        panel.Controls.Add(btn);
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        host.Controls.Add(_gridMemberships, 0, 0);
        host.Controls.Add(panel, 0, 1);
        tab.Controls.Add(host);
        _tabs.TabPages.Add(tab);
    }

    void OpenWorkspace()
    {
        var u = AppSession.CurrentUser!;
        if (_gridMemberships.CurrentRow?.DataBoundItem is not DataRowView rv) return;
        var sid = (int)rv.Row["SocietyID"];
        var role = rv.Row["RoleInSociety"].ToString();
        var status = rv.Row["MembershipStatus"].ToString();
        if (status != "Approved")
        {
            MessageBox.Show("Membership is not approved yet.");
            return;
        }

        Form f = role == "Head"
            ? new SocietyLeadershipForm(u, sid)
            : new MemberSocietyForm(u, sid);
        f.ShowDialog();
        ReloadAll();
    }

    void BuildEventsTab()
    {
        var tab = new TabPage("Upcoming events");
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var btn = new Button { Text = "Register for selected event", Dock = DockStyle.Right, Width = 220 };
        btn.Click += (_, _) =>
        {
            var u = AppSession.CurrentUser!;
            if (_gridEvents.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            var eid = (int)rv.Row["EventID"];
            var (ok, msg) = EventRepository.RegisterStudentForEvent(u.UserId, eid);
            MessageBox.Show(msg);
            if (ok) ReloadTickets();
        };
        panel.Controls.Add(btn);
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        host.Controls.Add(_gridEvents, 0, 0);
        host.Controls.Add(panel, 0, 1);
        tab.Controls.Add(host);
        _tabs.TabPages.Add(tab);
    }

    void BuildTicketsTab()
    {
        var tab = new TabPage("My tickets / passes");
        tab.Controls.Add(_gridTickets);
        _tabs.TabPages.Add(tab);
    }

    void BuildAnnounceTab()
    {
        var tab = new TabPage("Announcements");
        tab.Controls.Add(_gridAnnounce);
        _tabs.TabPages.Add(tab);
    }

    void BuildMyTasksTab()
    {
        var tab = new TabPage("My assigned tasks");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8) };
        var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        cmb.Items.AddRange(new object[] { "Pending", "InProgress", "Completed" });
        cmb.SelectedIndex = 0;
        var btn = new Button { Text = "Update status", AutoSize = true };
        btn.Click += (_, _) =>
        {
            var u = AppSession.CurrentUser!;
            if (_gridMyTasks.CurrentRow?.DataBoundItem is not DataRowView rv) return;
            var tid = (int)rv.Row["TaskID"];
            TaskRepository.UpdateTaskStatus(tid, u.UserId, cmb.SelectedItem!.ToString()!);
            ReloadMyTasks();
        };
        panel.Controls.AddRange(new Control[] { new Label { Text = "New status:", AutoSize = true }, cmb, btn });
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        host.Controls.Add(_gridMyTasks, 0, 0);
        host.Controls.Add(panel, 0, 1);
        tab.Controls.Add(host);
        _tabs.TabPages.Add(tab);
    }

    void ReloadAll()
    {
        ReloadBrowse();
        ReloadMemberships();
        ReloadEvents();
        ReloadTickets();
        ReloadAnnounce();
        ReloadMyTasks();
    }

    void ReloadBrowse() => GridFormatter.Bind(_gridBrowse, SocietyRepository.GetApprovedSocietiesForBrowse());
    void ReloadMemberships() => GridFormatter.Bind(_gridMemberships, SocietyRepository.GetMemberSocieties(AppSession.CurrentUser!.UserId));
    void ReloadEvents() => GridFormatter.Bind(_gridEvents, EventRepository.GetUpcomingApprovedEvents());
    void ReloadTickets() => GridFormatter.Bind(_gridTickets, EventRepository.GetTicketsForUser(AppSession.CurrentUser!.UserId));
    void ReloadAnnounce() => GridFormatter.Bind(_gridAnnounce, AnnouncementRepository.GetForStudentHome());
    void ReloadMyTasks() => GridFormatter.Bind(_gridMyTasks, TaskRepository.GetMyTasks(AppSession.CurrentUser!.UserId));
}

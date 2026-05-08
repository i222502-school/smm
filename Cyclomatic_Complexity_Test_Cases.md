# Cyclomatic Complexity and Test Case Inputs

Source: `lizard_report.txt`

| Function | Cyclomatic Complexity (CCN) | Test Case Inputs (Parameters + Explanation) |
|---|---:|---|
| `GridFormatter::Bind@7-16` | 1 | grid=DataGridView with sample rows (grid input source); table=DataTable sample (bindable table input) |
| `GridFormatter::BindEditable@18-26` | 1 | grid=DataGridView with sample rows (grid input source); table=DataTable sample (bindable table input) |
| `CsvExportHelper::ExportDataGridView@5-16` | 4 | grid=DataGridView with sample rows (grid input source); path='output.csv' (writable csv path) |
| `CsvExportHelper::Quote@18-18` | 1 | s='sample' (valid non-empty text) |
| `ActivityLogRepository::Log@7-21` | 5 | cn=open SqlConnection (active DB connection); userId=10 (valid optional id; null in alternate test); actionType='Create' (log action type); entityType='Event' (entity type for activity log); entityId=10 (valid optional id; null in alternate test); details='Created event #1' (human-readable log details) |
| `SqlConnectionFactory::CreateOpenConnection@8-17` | 3 | - |
| `UserAdminRepository::GetAllUsers@8-19` | 1 | - |
| `UserAdminRepository::UpdateUser@21-34` | 1 | adminUserId=1 (existing user id); userId=1 (existing user id); email='student1@example.com' (valid email format); fullName='Ali Khan' (displayable full name); isActive=true (tests positive/allowed branch) |
| `AnnouncementRepository::GetForStudentHome@8-25` | 1 | - |
| `AnnouncementRepository::GetForSociety@27-42` | 1 | societyId=1 (existing society id) |
| `AnnouncementRepository::Post@44-58` | 3 | societyId=10 (valid optional id; null in alternate test); postedByUserId=1 (existing user id); title='Orientation Event' (non-empty title); body='Announcement body text' (message content) |
| `ActivityLogQueryRepository::GetRecent@8-24` | 1 | top=100 (retrieve latest 100 rows) |
| `TaskRepository::GetTasksForSociety@8-25` | 1 | societyId=1 (existing society id) |
| `TaskRepository::GetMyTasks@27-41` | 1 | userId=1 (existing user id) |
| `TaskRepository::AddTask@43-60` | 2 | societyId=1 (existing society id); assignedByUserId=1 (existing user id); assignedToUserId=1 (existing user id); title='Orientation Event' (non-empty title); description='Detailed description text' (non-empty details); dueDate=2026-05-15 17:00 (valid date-time input) |
| `TaskRepository::UpdateTaskStatus@62-74` | 1 | taskId=1 (existing task id); userId=1 (existing user id); status='Approved' (valid workflow status) |
| `EventRepository::GetUpcomingApprovedEvents@8-24` | 1 | - |
| `EventRepository::GetEventsForSociety@26-39` | 1 | societyId=1 (existing society id) |
| `EventRepository::GetPendingEventsForAdmin@41-55` | 1 | - |
| `EventRepository::CreateEvent@57-80` | 3 | societyId=1 (existing society id); creatorUserId=1 (existing user id); title='Orientation Event' (non-empty title); description='Detailed description text' (non-empty details); venue='Main Hall' (valid venue text); start=2026-05-10 10:00 (event start time); end=2026-05-10 12:00 (event end time after start); maxParticipants=100 (capacity limit; null in alternate test for unlimited); submitForApproval=true (tests positive/allowed branch) |
| `EventRepository::UpdateEvent@82-102` | 2 | eventId=1 (existing event id); societyId=1 (existing society id); title='Orientation Event' (non-empty title); description='Detailed description text' (non-empty details); venue='Main Hall' (valid venue text); start=2026-05-10 10:00 (event start time); end=2026-05-10 12:00 (event end time after start); maxParticipants=100 (capacity limit; null in alternate test for unlimited); status='Approved' (valid workflow status) |
| `EventRepository::CancelEvent@104-115` | 1 | eventId=1 (existing event id); societyId=1 (existing society id); userId=1 (existing user id) |
| `EventRepository::AdminApproveEvent@117-134` | 3 | eventId=1 (existing event id); adminUserId=1 (existing user id); approve=true (tests positive/allowed branch) |
| `EventRepository::RegisterStudentForEvent@136-177` | 10 | userId=1 (existing user id); eventId=1 (existing event id) |
| `EventRepository::GetTicketsForUser@179-195` | 1 | userId=1 (existing user id) |
| `EventRepository::GetUniversityReport@197-215` | 1 | - |
| `EventRepository::GetSocietyEventsReport@217-230` | 1 | societyId=1 (existing society id) |
| `SocietyRepository::GetApprovedSocietiesForBrowse@8-19` | 1 | - |
| `SocietyRepository::GetSocietiesForAdmin@21-35` | 1 | - |
| `SocietyRepository::CreateSocietyRequest@38-78` | 2 | creatorUserId=1 (existing user id); name='sample' (valid non-empty text); description='Detailed description text' (non-empty details) |
| `SocietyRepository::UpdateSocietyProfile@80-92` | 1 | societyId=1 (existing society id); name='sample' (valid non-empty text); description='Detailed description text' (non-empty details); actingUserId=1 (existing user id) |
| `SocietyRepository::AdminSetSocietyStatus@94-110` | 1 | societyId=1 (existing society id); status='Approved' (valid workflow status); adminUserId=1 (existing user id) |
| `SocietyRepository::AdminDeleteSociety@112-120` | 1 | societyId=1 (existing society id); adminUserId=1 (existing user id) |
| `SocietyRepository::IsHeadOf@122-133` | 1 | userId=1 (existing user id); societyId=1 (existing society id) |
| `SocietyRepository::IsApprovedMemberOf@135-146` | 1 | userId=1 (existing user id); societyId=1 (existing society id) |
| `SocietyRepository::GetHeadSocieties@148-164` | 1 | userId=1 (existing user id) |
| `SocietyRepository::GetMemberSocieties@166-181` | 1 | userId=1 (existing user id) |
| `SocietyRepository::ApplyMembership@183-201` | 1 | userId=1 (existing user id); societyId=1 (existing society id); roleMember='Member' (allowed role value) |
| `SocietyRepository::GetPendingMembershipsForSociety@203-218` | 1 | societyId=1 (existing society id) |
| `SocietyRepository::GetMemberRoster@220-235` | 1 | societyId=1 (existing society id) |
| `SocietyRepository::ResolveMembership@237-251` | 2 | membershipId=1 (existing membership id); approve=true (tests positive/allowed branch); headUserId=1 (existing user id) |
| `SocietyRepository::RemoveMemberFromSociety@254-267` | 2 | societyId=1 (existing society id); memberUserId=1 (existing user id); headUserId=1 (existing user id) |
| `PasswordHasher::HashPassword@12-17` | 1 | password='Pass123!' (meets min-length rule); salt=550e8400-e29b-41d4-a716-446655440000 (stable GUID test salt) |
| `PasswordHasher::Verify@19-23` | 1 | password='Pass123!' (meets min-length rule); salt=550e8400-e29b-41d4-a716-446655440000 (stable GUID test salt); storedHash=[32-byte hash] (persisted hash bytes for verification) |
| `MemberSocietyForm::new@13-13` | 1 | - |
| `MemberSocietyForm::new@14-14` | 1 | - |
| `MemberSocietyForm::new@15-15` | 1 | - |
| `MemberSocietyForm::MemberSocietyForm@17-68` | 4 | user=SessionUser(UserId=1, UserType='Student') (authenticated context); societyId=1 (existing society id) |
| `MemberSocietyForm::ReloadRoster@70-70` | 1 | - |
| `MemberSocietyForm::ReloadTasks@72-85` | 3 | - |
| `MemberSocietyForm::ReloadAnnounce@87-87` | 1 | - |
| `StudentPortalForm::new@8-8` | 1 | - |
| `StudentPortalForm::new@10-10` | 1 | - |
| `StudentPortalForm::new@11-11` | 1 | - |
| `StudentPortalForm::new@12-12` | 1 | - |
| `StudentPortalForm::new@13-13` | 1 | - |
| `StudentPortalForm::new@14-14` | 1 | - |
| `StudentPortalForm::new@15-15` | 1 | - |
| `StudentPortalForm::new@17-17` | 1 | - |
| `StudentPortalForm::new@18-18` | 1 | - |
| `StudentPortalForm::StudentPortalForm@20-54` | 1 | - |
| `StudentPortalForm::BuildBrowseTab@56-82` | 4 | - |
| `StudentPortalForm::BuildRequestTab@84-113` | 2 | - |
| `StudentPortalForm::BuildMembershipsTab@115-129` | 1 | - |
| `StudentPortalForm::OpenWorkspace@131-149` | 5 | - |
| `StudentPortalForm::BuildEventsTab@151-173` | 4 | - |
| `StudentPortalForm::BuildTicketsTab@175-180` | 1 | - |
| `StudentPortalForm::BuildAnnounceTab@182-187` | 1 | - |
| `StudentPortalForm::BuildMyTasksTab@189-213` | 3 | - |
| `StudentPortalForm::ReloadAll@215-223` | 1 | - |
| `StudentPortalForm::ReloadBrowse@225-225` | 1 | - |
| `StudentPortalForm::ReloadMemberships@226-226` | 1 | - |
| `StudentPortalForm::ReloadEvents@227-227` | 1 | - |
| `StudentPortalForm::ReloadTickets@228-228` | 1 | - |
| `StudentPortalForm::ReloadAnnounce@229-229` | 1 | - |
| `StudentPortalForm::ReloadMyTasks@230-230` | 1 | - |
| `LoginForm::new@7-7` | 1 | - |
| `LoginForm::new@8-8` | 1 | - |
| `LoginForm::new@9-9` | 1 | - |
| `LoginForm::new@10-10` | 1 | - |
| `LoginForm::new@11-11` | 1 | - |
| `LoginForm::LoginForm@13-32` | 1 | - |
| `LoginForm::TryLogin@34-59` | 3 | - |
| `SocietyLeadershipForm::new@13-13` | 1 | - |
| `SocietyLeadershipForm::new@14-14` | 1 | - |
| `SocietyLeadershipForm::new@15-15` | 1 | - |
| `SocietyLeadershipForm::new@16-16` | 1 | - |
| `SocietyLeadershipForm::new@17-17` | 1 | - |
| `SocietyLeadershipForm::new@18-18` | 1 | - |
| `SocietyLeadershipForm::new@19-19` | 1 | - |
| `SocietyLeadershipForm::new@21-21` | 1 | - |
| `SocietyLeadershipForm::new@22-22` | 1 | - |
| `SocietyLeadershipForm::new@23-23` | 1 | - |
| `SocietyLeadershipForm::new@24-24` | 1 | - |
| `SocietyLeadershipForm::new@25-25` | 1 | - |
| `SocietyLeadershipForm::new@26-26` | 1 | - |
| `SocietyLeadershipForm::new@28-28` | 1 | - |
| `SocietyLeadershipForm::new@29-29` | 1 | - |
| `SocietyLeadershipForm::new@30-30` | 1 | - |
| `SocietyLeadershipForm::new@31-31` | 1 | - |
| `SocietyLeadershipForm::new@33-33` | 1 | - |
| `SocietyLeadershipForm::new@34-34` | 1 | - |
| `SocietyLeadershipForm::SocietyLeadershipForm@36-70` | 2 | user=SessionUser(UserId=1, UserType='Student') (authenticated context); societyId=1 (existing society id) |
| `SocietyLeadershipForm::LoadProfile@72-84` | 2 | - |
| `SocietyLeadershipForm::BuildProfileTab@86-103` | 1 | - |
| `SocietyLeadershipForm::BuildMembershipTab@105-122` | 1 | - |
| `SocietyLeadershipForm::ResolvePending@124-131` | 3 | approve=true (tests positive/allowed branch) |
| `SocietyLeadershipForm::BuildMembersTab@133-160` | 5 | - |
| `SocietyLeadershipForm::BuildEventsTab@162-202` | 1 | - |
| `SocietyLeadershipForm::AddEvent@204-211` | 4 | submitForApproval=true (tests positive/allowed branch) |
| `SocietyLeadershipForm::UpdateSelectedEvent@213-223` | 5 | - |
| `SocietyLeadershipForm::CancelSelectedEvent@225-231` | 3 | - |
| `SocietyLeadershipForm::BuildTasksTab@233-266` | 4 | - |
| `MemberPick::BuildAnnounceTab@284-310` | 1 | - |
| `MemberPick::BuildReportsTab@312-337` | 2 | - |
| `MemberPick::ReloadGrids@339-347` | 1 | - |
| `MemberPick::HideColumn@349-353` | 2 | g=DataGridView with sample rows (grid input source); name='sample' (valid non-empty text) |
| `AdminDashboardForm::new@8-8` | 1 | - |
| `AdminDashboardForm::new@10-10` | 1 | - |
| `AdminDashboardForm::new@11-11` | 1 | - |
| `AdminDashboardForm::new@12-12` | 1 | - |
| `AdminDashboardForm::new@13-13` | 1 | - |
| `AdminDashboardForm::new@16-16` | 1 | - |
| `AdminDashboardForm::new@18-18` | 1 | - |
| `AdminDashboardForm::new@20-20` | 1 | - |
| `AdminDashboardForm::new@22-22` | 1 | - |
| `AdminDashboardForm::new@23-23` | 1 | - |
| `AdminDashboardForm::new@24-24` | 1 | - |
| `AdminDashboardForm::AdminDashboardForm@26-56` | 1 | - |
| `AdminDashboardForm::BuildUsersTab@58-96` | 5 | - |
| `AdminDashboardForm::BuildSocietiesTab@98-133` | 6 | - |
| `AdminDashboardForm::BuildEventsTab@135-161` | 5 | - |
| `AdminDashboardForm::BuildActivityTab@163-168` | 1 | - |
| `AdminDashboardForm::BuildReportsTab@170-209` | 2 | - |
| `AdminDashboardForm::ReloadAll@211-218` | 1 | - |
| `AdminDashboardForm::ReloadUsers@220-220` | 1 | - |
| `AdminDashboardForm::ReloadSocieties@221-221` | 1 | - |
| `AdminDashboardForm::ReloadPendingEvents@222-222` | 1 | - |
| `AdminDashboardForm::ReloadActivity@223-223` | 1 | - |
| `RegisterForm::new@7-7` | 1 | - |
| `RegisterForm::new@8-8` | 1 | - |
| `RegisterForm::new@9-9` | 1 | - |
| `RegisterForm::new@10-10` | 1 | - |
| `RegisterForm::new@11-11` | 1 | - |
| `RegisterForm::new@12-12` | 1 | - |
| `RegisterForm::new@13-13` | 1 | - |
| `RegisterForm::RegisterForm@15-33` | 3 | - |
| `AuthService::Login@10-37` | 4 | username='student1' (valid username); password='Pass123!' (meets min-length rule) |
| `AuthService::RegisterStudent@39-69` | 6 | username='student1' (valid username); password='Pass123!' (meets min-length rule); email='student1@example.com' (valid email format); fullName='Ali Khan' (displayable full name) |
| `Program::Main@8-12` | 1 | - |
| `AppSession::SignOut@9-9` | 1 | - |

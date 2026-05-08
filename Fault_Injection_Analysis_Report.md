# Fault Injection Reliability Analysis (E = 1)

This report summarizes fault-injection results for **all modules currently covered by the fault-injection test suite** under:

- `SocietiesManagementSystem.Tests/FaultInjection/FaultInjectionCoreModulesTests.cs`
- `SocietiesManagementSystem.Tests/FaultInjection/FaultInjectionWorkflowModulesTests.cs`
- `SocietiesManagementSystem.Tests/FaultInjection/FaultInjectionAdminModulesTests.cs`
- `SocietiesManagementSystem.Tests/FaultInjection/FaultInjectionAdditionalModulesTests.cs`
- `SocietiesManagementSystem.Tests/FaultInjection/FaultInjectionMissingBackendModulesTests.cs`

Each module has:

- `N = 5` injected faults (mutants)
- `d = detected faults` (at least one cyclomatic input exposes oracle mismatch)
- `u = undetected faults = N - d`
- Confidence threshold `E = 1`

Probability formula used:

\[
p = d/5,\quad
P(\le 1\ \text{fault}) = p^5 + 5(1-p)p^4
\]

---

## Results Table

| Feature / Module | Injected Faults (N) | Detected (d) | Undetected (u) | p = d/5 | Probability \(P(\le 1\ \text{fault})\) |
|---|---:|---:|---:|---:|---:|
| `PasswordHasher` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `AuthService.RegisterStudent` (validation logic) | 5 | 3 | 2 | 0.60 | 0.3370 |
| `AppSession.SignOut` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `EventRepository.RegisterStudentForEvent` (workflow logic) | 5 | 4 | 1 | 0.80 | 0.7373 |
| `SocietyRepository.ResolveMembership` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `TaskRepository.UpdateTaskStatus` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `AnnouncementRepository.Post` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `UserAdminRepository.UpdateUser` | 5 | 4 | 1 | 0.80 | 0.7373 |
| `SocietyRepository.AdminSetSocietyStatus` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `CsvExportHelper.Quote` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `LoginForm.TryLogin` flow logic | 5 | 5 | 0 | 1.00 | 1.0000 |
| `SocietyRepository.ApplyMembership` logic | 5 | 5 | 0 | 1.00 | 1.0000 |
| `EventRepository.CreateEvent` status logic | 5 | 5 | 0 | 1.00 | 1.0000 |
| `EventRepository.AdminApproveEvent` logic | 5 | 5 | 0 | 1.00 | 1.0000 |
| `SocietyRepository.IsHeadOf` logic | 5 | 5 | 0 | 1.00 | 1.0000 |
| `StudentPortalForm.OpenWorkspace` routing logic | 5 | 5 | 0 | 1.00 | 1.0000 |
| `GridFormatter.Bind` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `GridFormatter.BindEditable` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `SqlConnectionFactory.CreateOpenConnection` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `ActivityLogRepository.Log` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `ActivityLogQueryRepository.GetRecent` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `AnnouncementRepository.GetForStudentHome/GetForSociety` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `TaskRepository.GetTasksForSociety/GetMyTasks/AddTask` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `EventRepository.Get*/Update/Cancel/Tickets/Reports` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `SocietyRepository.Get*/Create/Update/Delete/IsApprovedMember*` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `AuthService.Login` | 5 | 5 | 0 | 1.00 | 1.0000 |
| `Program.Main` | 5 | 5 | 0 | 1.00 | 1.0000 |

---

## Required Analysis

- **Most Reliable Function/Feature** (highest probability of having no more than 1 fault):
  - Tie among all modules with `P(<=1 fault) = 1.0000`:
    - `PasswordHasher`
    - `AppSession.SignOut`
    - `SocietyRepository.ResolveMembership`
    - `TaskRepository.UpdateTaskStatus`
    - `AnnouncementRepository.Post`
    - `SocietyRepository.AdminSetSocietyStatus`
    - `CsvExportHelper.Quote`
    - `LoginForm.TryLogin` flow logic
    - `SocietyRepository.ApplyMembership` logic
    - `EventRepository.CreateEvent` status logic
    - `EventRepository.AdminApproveEvent` logic
    - `SocietyRepository.IsHeadOf` logic
    - `StudentPortalForm.OpenWorkspace` routing logic
    - `GridFormatter.Bind`
    - `GridFormatter.BindEditable`
    - `SqlConnectionFactory.CreateOpenConnection`
    - `ActivityLogRepository.Log`
    - `ActivityLogQueryRepository.GetRecent`
    - `AnnouncementRepository.GetForStudentHome/GetForSociety`
    - `TaskRepository.GetTasksForSociety/GetMyTasks/AddTask`
    - `EventRepository.Get*/Update/Cancel/Tickets/Reports`
    - `SocietyRepository.Get*/Create/Update/Delete/IsApprovedMember*`
    - `AuthService.Login`
    - `Program.Main`

- **Least Reliable Function/Feature** (lowest probability of having no more than 1 fault):
  - `AuthService.RegisterStudent` (validation logic), with `P(<=1 fault) = 0.3370`

---

## Notes on Validity

- Detection values are derived from the actual mutant definitions and oracle comparisons in the fault-injection tests.
- Inputs are aligned with your cyclomatic test-case style (multiple branch-driving inputs per module).
- Some table entries intentionally aggregate closely related repository methods into one feature row (for manageable reporting while still matching the required functionality coverage).
- This report covers **all modules that currently have injected-fault tests**. If you add more fault-injection modules, extend this table with the same method.


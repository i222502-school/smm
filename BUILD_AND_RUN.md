# Build from GitHub Actions and run the app

## Does GitHub Actions give you a binary?

**Yes.** The workflow publishes a **Release** build into a folder and uploads it as a ZIP artifact. Inside that ZIP you get:

- **`SocietiesManagementSystem.exe`** — the main program you double‑click (or run from a terminal).
- **`.dll` files**, **`SocietiesManagementSystem.dll.config`** (from `App.config`), and other runtime files the app needs.

That is a normal **framework-dependent** publish: the `.exe` is real, but the machine must have the **.NET 8 Desktop Runtime for Windows** installed (see below).

---

## Where you get the compiled project on GitHub

1. Open your repository on **GitHub**.
2. Click the **Actions** tab (top navigation).
3. In the left sidebar, click **“Build WinForms app”** (the workflow name from `.github/workflows/build-winforms.yml`).
4. Click the **latest successful run** (green checkmark).
5. Scroll to the bottom of the run summary page to the **Artifacts** section.
6. Download **`SocietiesManagementSystem-win-x64`** — this is a **ZIP** containing the published folder.

**Notes:**

- If nothing appears under Artifacts, the workflow did not finish successfully — open the failed job and read the logs.
- Artifacts are kept for a limited time (repository/org retention rules apply; often on the order of weeks unless changed).

You can also start a run manually: **Actions → Build WinForms app → Run workflow** (requires `workflow_dispatch`, which is already enabled).

---

## Exact steps after download (on Windows)

1. **Download** the artifact ZIP from the workflow run (steps above).
2. **Extract** the ZIP to a folder, e.g. `C:\Apps\SocietiesManagementSystem\`.
3. **Install prerequisites:**
   - **.NET 8 Desktop Runtime (Windows x64)**  
     Download from Microsoft’s [.NET 8 download page](https://dotnet.microsoft.com/download/dotnet/8.0) — choose **Desktop Runtime** → **x64**.
   - **SQL Server** (LocalDB, Express, or full instance) with the database created from the script in this repo.
4. **Create the database** (once per machine/server):
   - Open **`Database/SocietiesDB.sql`** in **SQL Server Management Studio** (or use `sqlcmd`) connected to your SQL Server instance.
   - Execute the script. It creates database **`SocietiesManagement`** and seed users.
5. **Point the app at your SQL Server:**
   - In the extracted folder, edit **`SocietiesManagementSystem.dll.config`** (same settings as `App.config` in source).
   - Under `<connectionStrings>`, set **`Data Source=...`** (and use **SQL authentication** if needed: `User Id=...;Password=...` instead of Integrated Security).
6. **Run** **`SocietiesManagementSystem.exe`** from the extracted folder.
7. **Log in** with seeded accounts (from the SQL script), e.g. **`admin` / `Admin@123`** or **`student1` / `Student@123`**, unless you changed them.

---

## If you want a single folder with no .NET install on the target PC

The current workflow uses the default **framework-dependent** publish. To produce a **self-contained** build (larger ZIP, includes the runtime), change the publish step in `.github/workflows/build-winforms.yml` to include:

```yaml
-r win-x64 --self-contained true
```

Then rebuild on Actions and download the new artifact the same way.

---

## Summary

| Question | Answer |
|----------|--------|
| Binary? | Yes — **`SocietiesManagementSystem.exe`** plus DLLs/config in the artifact ZIP. |
| Where on GitHub? | **Repository → Actions → workflow run → Artifacts → `SocietiesManagementSystem-win-x64`**. |
| Where to run? | **Windows** only (WinForms). |
| Extra install? | **.NET 8 Desktop Runtime** (unless you switch to self-contained publish) and **SQL Server** with the schema loaded. |

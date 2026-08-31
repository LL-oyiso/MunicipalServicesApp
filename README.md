# Municipal Services App

A C# Windows Forms (.NET Framework 4.8) application that lets residents report municipal
issues and service requests. This phase implements the **Report Issues** task in full;
**Local Events and Announcements** and **Service Request Status** are visible on the main
menu but intentionally disabled, ready for later phases.

## Prerequisites

- Windows 10/11.
- One of:
  - **Visual Studio 2022 (or later)** with the ".NET desktop development" workload installed, or
  - The **.NET SDK** (8.0+) available on `PATH` (`dotnet --version` to check), which can build/run
    .NET Framework 4.8 projects from the command line.
- **.NET Framework 4.8 Developer/Targeting Pack** (already installed if Visual Studio's desktop
  development workload is installed).

No third-party NuGet packages are required - the project only uses the standard
`System.Windows.Forms` libraries that ship with .NET Framework.

## Project structure

```
MunicipalServicesApp/
  MunicipalServicesApp.slnx          - solution file
  README.md                          - this file
  MunicipalServicesApp/              - project folder
    Program.cs                       - application entry point
    MainMenuForm.cs / .Designer.cs   - startup screen (3 tasks, 2 disabled)
    ReportIssuesForm.cs / .Designer.cs - the Report Issues screen
    Models/
      Issue.cs                       - data model for a single reported issue
    Data/
      IssueRepository.cs             - in-memory List<Issue> store for all reported issues
    Common/
      AppTheme.cs                    - shared colours/fonts/button styles used by every form
```

## How to compile and run

### Option A: Visual Studio

1. Open `MunicipalServicesApp.slnx` (double-click it, or use File > Open > Project/Solution
   in Visual Studio).
2. Press **F5** (or **Ctrl+F5** to run without debugging). Visual Studio will restore/build
   automatically and launch the app.

### Option B: Command line (.NET SDK)

From the `MunicipalServicesApp` folder (the one containing `MunicipalServicesApp.slnx`):

```bash
dotnet build MunicipalServicesApp\MunicipalServicesApp.csproj
dotnet run --project MunicipalServicesApp\MunicipalServicesApp.csproj
```

Or simply run the built executable directly after building:

```bash
MunicipalServicesApp\MunicipalServicesApp\bin\Debug\MunicipalServicesApp.exe
```

## How to use the app

1. **Main Menu** opens on startup with three options:
   - **Report Issues** - enabled, described below.
   - **Local Events and Announcements** - disabled (greyed out); hover over it to see a
     tooltip explaining it's coming in a future update.
   - **Service Request Status** - disabled for the same reason.

2. Click **Report Issues** to open the reporting form. Fill in:
   - **Location** - a text description of where the issue is (address, suburb, or landmark).
   - **Category** - choose one from the dropdown (Sanitation, Roads, Utilities, Electricity,
     Water, Other).
   - **Description** - describe the issue in detail in the text box.
   - **Attachments (optional)** - click **Attach Image/Document...** to open a file picker;
     you can select multiple images or documents at once. Attached files appear in the list
     below. Select one or more entries in the list and click **Remove Selected** to undo an
     attachment.

3. As you fill in each field, watch the **progress bar and message** just above the buttons -
   it fills up in 25% steps (Location, Category, Description, Attachment) and the message
   changes to encourage you along, for example "You're halfway there - now describe what's
   happening."

4. Click **Submit** once all required fields (Location, Category, Description - marked with
   `*`) are filled in.
   - If something required is missing, a message box tells you exactly what to fix.
   - On success, a confirmation message box shows how many issues you've reported this
     session, and the form resets so you can report another issue straight away.

5. Click **Back to Main Menu** at any time to return to the main menu without submitting.

## Notes

- **Data storage**: reported issues are stored in memory only (`IssueRepository`, backed by a
  `List<Issue>`), for the duration of the running application. There is no database or file
  persistence yet - this will be introduced as later tasks (Service Request Status) are
  implemented.
- **Why Local Events and Service Request Status are disabled**: the brief for this phase only
  requires the Report Issues task to be implemented; the other two are deliberately left as
  visibly disabled placeholders rather than removed, so the full intended navigation structure
  of the app is already in place for future work.
- **Engagement strategy**: the dynamic progress bar and encouraging messages on the Report
  Issues screen are the concrete implementation of the "Participatory Co-Design and Community
  Consultation" engagement strategy chosen in the Task 1 research - giving residents visible,
  immediate feedback that their input is being captured, and a session-report-count on
  submission to reinforce that their participation has real impact.

## Troubleshooting

- **"The .exe is locked / access denied" when building**: close any previously running
  instance of the app (check the taskbar, or Visual Studio's "Stop Debugging" button) before
  rebuilding.
- **Missing .NET Framework 4.8 error when opening in Visual Studio**: install the ".NET
  Framework 4.8 targeting pack" via the Visual Studio Installer (Individual Components tab).
- **Attach dialog shows no files**: the file filter defaults to common image/document types
  (`.jpg`, `.png`, `.pdf`, `.docx`, etc.); switch the dialog's file type filter to "All files"
  to see other file types.

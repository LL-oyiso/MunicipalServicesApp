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
    ReportIssuesForm.cs / .Designer.cs - the Report Issues wizard
    Models/
      Issue.cs                       - data model for a single reported issue
    Data/
      IssueRepository.cs             - in-memory List<Issue> store for all reported issues
    Common/
      AppTheme.cs                    - shared colours/fonts/button styles used by every form
      StepIndicatorControl.cs        - custom-drawn wizard step indicator (progress chips)
```

The wizard's Designer file holds only the persistent chrome - the header bar with the step
indicator, the content area and the footer navigation. The seven step panels themselves are
built in code in `ReportIssuesForm.cs`, because the Summary and Confirmation screens have to
be generated from whatever the resident actually entered.

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

2. Click **Report Issues** to open the reporting wizard. Instead of one long form, the
   report is captured over six short steps, with a seventh confirmation screen at the end.
   The step indicator across the top of the window shows which step you are on, which ones
   are already done (ticked) and which are still ahead.

3. Work through the steps using the **Next** and **Back** buttons in the footer. Fields
   marked with `*` are required, and each step is checked before you can move on - if
   something is missing, a message box explains what to fix and puts the cursor in the
   field that needs attention.

   1. **Category** - choose one option from the dropdown (Sanitation, Roads, Utilities,
      Electricity, Water, Other). Pick *Other* if nothing fits and explain it in the
      description.
   2. **Description** - describe what you can see and how long it has been like that. A live
      counter under the box tracks your character count and turns green once you have passed
      the ten-character minimum.
   3. **Location** - the street address or landmark where the issue is, plus an optional
      **Additional location information** box for directions such as "behind the community
      hall".
   4. **Attachment** - optional. Click **Attach Files** to open a file picker; you can select
      several images or documents at once. Attached files are listed by name, and duplicates
      are ignored. Select one or more entries and click **Remove** to take them off the
      report. You can continue without attaching anything.
   5. **Contact** - your first name and surname, and how you would like to be kept updated.
      Choosing **Mobile (SMS)** makes the mobile number required and checks it is ten digits;
      choosing **Email** makes the email address required and sanity-checks its format. The
      `*` markers move between the two fields as you switch, so only what is needed is asked
      for.
   6. **Summary** - a review screen with one card per section showing exactly what will be
      submitted. Each card has an **Edit** link that jumps straight back to that step with
      everything you have already entered still in place. When you return to the summary it
      rebuilds itself from your latest answers.

4. Click **Submit** on the summary step to log the report.

5. The **Confirmation** screen shows your **reference number** (for example `REQ-2026-0001`)
   in large text, along with a recap of the report and how many reports you have logged this
   session. From here you can either click **New Report** to clear everything and start a
   fresh report at step 1, or **Close** to return to the main menu.

6. Clicking **Cancel** in the footer at any point returns you to the main menu. If you have
   already entered something, it asks you to confirm first so a report is never discarded by
   accident.

## Notes

- **Data storage**: reported issues are stored in memory only (`IssueRepository`, backed by a
  `List<Issue>`), for the duration of the running application. There is no database or file
  persistence yet - this will be introduced as later tasks (Service Request Status) are
  implemented.
- **Why Local Events and Service Request Status are disabled**: the brief for this phase only
  requires the Report Issues task to be implemented; the other two are deliberately left as
  visibly disabled placeholders rather than removed, so the full intended navigation structure
  of the app is already in place for future work.
- **Engagement strategy**: the step indicator in the wizard header is the concrete
  implementation of the "Participatory Co-Design and Community Consultation" engagement
  strategy chosen in the Task 1 research. It is a custom control drawn with GDI+
  (`Common/StepIndicatorControl.cs`) that ticks off each completed step, so residents always
  know how far through the process they are and how much is left rather than facing an
  intimidating wall of fields. It works alongside the encouraging per-step message in the
  footer, the live description counter, the summary screen that lets residents check and
  correct their own submission before committing to it, and the reference number and
  session report count on the confirmation screen - all reinforcing that their input is being
  captured and that their participation has real impact.
- **Why a wizard**: the reporting flow is modelled on the City of Cape Town's real
  "New Service Request" journey, which splits a service request across short, focused steps.
  Breaking the form up this way lowers the effort of any single screen and lets each step
  carry its own guidance banner.

## Troubleshooting

- **"The .exe is locked / access denied" when building**: close any previously running
  instance of the app (check the taskbar, or Visual Studio's "Stop Debugging" button) before
  rebuilding.
- **Missing .NET Framework 4.8 error when opening in Visual Studio**: install the ".NET
  Framework 4.8 targeting pack" via the Visual Studio Installer (Individual Components tab).
- **Attach dialog shows no files**: the file filter defaults to common image/document types
  (`.jpg`, `.png`, `.pdf`, `.docx`, etc.); switch the dialog's file type filter to "All files"
  to see other file types.
- **Next button seems to do nothing**: the current step has a required field that is still
  empty or invalid - a message box appears explaining which one, and the cursor is placed in
  that field.

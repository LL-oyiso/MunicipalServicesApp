# Municipal Services App

A C# Windows Forms (.NET Framework 4.8) application that lets residents report municipal
issues, browse local events and announcements, and (in a later phase) check on service
requests. This phase implements the **Report Issues** and **Local Events and Announcements**
tasks in full; **Service Request Status** is visible on the main menu but intentionally
disabled, ready for a later phase. A bonus **Community Resources** section is also included -
see the note below, it is an unmarked extra and not part of the graded submission.

> **Community Resources is a bonus, non-marked extra.** It is not required by, and is not
> scored against, the marking rubric for this submission. It was added purely to make the app
> more useful to a resident - see the "Community Resources (bonus)" section further down for
> what it does.

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
    MainMenuForm.cs / .Designer.cs   - startup screen (4 tasks, 1 disabled, 1 bonus)
    ReportIssuesForm.cs / .Designer.cs - the Report Issues wizard
    LocalEventsForm.cs / .Designer.cs  - the Local Events and Announcements screen
    CommunityResourcesForm.cs / .Designer.cs - bonus section (not marked), see below
    Models/
      Issue.cs                       - data model for a single reported issue
      Event.cs                       - data model for a single local event
      Announcement.cs                - data model for a single municipal announcement
      EmergencyContact.cs            - bonus: a single emergency service contact
      WardCouncilMember.cs           - bonus: a single ward councillor/caretaker
      CommunityProgram.cs            - bonus: a single community support programme
    Data/
      IssueRepository.cs             - in-memory List<Issue> store for all reported issues
      EventsRepository.cs            - in-memory event/announcement store (dictionaries,
                                        sorted dictionary, hash sets, queue - seeded with
                                        sample data)
      RecommendationEngine.cs        - per-session search history (stack) and category
                                        frequency ranking used to recommend events
      CommunityResourcesRepository.cs - bonus: in-memory store for Emergency Services,
                                        Know Your Ward and Community Programs
    Common/
      AppTheme.cs                    - shared colours/fonts/button styles used by every form
      StepIndicatorControl.cs        - custom-drawn wizard step indicator (progress chips)
      MenuCardControl.cs             - custom-drawn main menu task card
      PriorityQueue.cs               - generic binary min-heap (no built-in equivalent in
                                        .NET Framework 4.8), used to rank announcements and
                                        recommended categories
      EventCardControl.cs            - custom-drawn event card (plain and "recommended" variants)
      AnnouncementCardControl.cs     - custom-drawn announcement card (urgent/resident-submitted variants)
      NotificationBadgeControl.cs    - small red "unread count" badge used on the Announcements tab
      ContactCardControl.cs          - bonus: generic card (title/badge/detail lines) shared by
                                        all three Community Resources sections
      FlowLayoutHelper.cs            - shared FlowLayoutPanel wrap-height fix used by every
                                        scrollable card gallery in the app
```

The wizard's Designer file holds only the persistent chrome - the header bar with the step
indicator, the content area and the footer navigation. The seven step panels themselves are
built in code in `ReportIssuesForm.cs`, because the Summary and Confirmation screens have to
be generated from whatever the resident actually entered. `LocalEventsForm` follows the same
pattern: its Designer file only holds the header bar and an empty content panel, while the
tab strip, filter bar, galleries, submission form and pending-review queue are all built in
code in `LocalEventsForm.cs`, since they react to live search/filter state.

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

1. **Main Menu** opens on startup with four options:
   - **Report Issues** - enabled, described below.
   - **Local Events and Announcements** - enabled, described further down.
   - **Community Resources** - enabled, tagged **"Bonus"**; a non-marked extra described
     further down.
   - **Service Request Status** - disabled (greyed out); hover over it to see a tooltip
     explaining it's coming in a future update.

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
      Choosing **Mobile (SMS)** makes the mobile number required; choosing **Email** makes the
      email address required. The `*` markers move between the two fields as you switch, so
      only what is needed is asked for. Whichever fields you do fill in are format-checked,
      including the one you didn't pick, so a typo in the spare field is still caught:
      - Names must be letters only (spaces, hyphens and apostrophes are allowed).
      - An email address needs a single `@`, a local part, and a dotted domain ending in a
        letters-only top-level domain - so `name@example.com` and `name@example.co.za` pass,
        while `name@example` or `name@example.` are rejected.
      - Two-letter endings have to stay valid for South African addresses like `.co.za`, which
        would otherwise let a mistyped `gmail.co` through. The major free providers (Gmail,
        Outlook, Hotmail, Yahoo, iCloud, Live and AOL) only ever use `.com`, so those are
        checked by name and the error suggests the correct ending.
      - A mobile number must be exactly 10 digits starting with `0`, and the digit after that
        leading `0` cannot also be `0`, since no South African area or network code begins
        `00`. That rules out placeholder entries such as `0000000000`.
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

7. Click **Local Events and Announcements** on the main menu to open that screen (it opens
   maximized - there's a lot to show). It has two tabs:

   - **Upcoming Events**
     - A filter bar lets you narrow events by **Category**, a **From/To date** range (the
       pickers default to the earliest/latest seeded event dates, so the initial view shows
       everything), and a free-text **Keyword** search across titles, descriptions and
       locations. **Search** applies the filters; **Clear filters** resets them.
     - Below that, the **Upcoming Events** gallery shows every event matching the current
       filters as a card (date tile, title, category badge, location and description).
     - Once you've searched at least once, a **Recommended For You** section appears beneath
       the gallery: events ranked by how often you've searched each category this session,
       always excluding whatever's already visible in the gallery above (so it never just
       repeats what you can already see). Each recommended card is tagged "RECOMMENDED" with
       a short reason (e.g. "Because you searched Community 2 time(s)"). A **Previous
       Search** button steps backwards through your last few searches (one at a time),
       re-applying each one's filters.

   - **Announcements** - a notification badge on this tab shows how many published
     announcements you haven't viewed yet; it clears once you switch to the tab.
     - The **Municipal Announcements** gallery lists every published announcement, with
       **URGENT** ones shown first (red border) and a "Resident request" tag on announcements
       that started as a resident submission rather than an official notice.
     - **Submit an Announcement Request** lets you suggest a title, category and message for
       the municipality to review. Submitted requests don't appear on the public board
       immediately - they go into **Pending Review** below.
     - **Pending Review** lists everything awaiting review; **Publish Next** publishes the
       oldest pending request (first-in-first-out) onto the public Announcements gallery
       above.

8. **Community Resources (bonus, not marked)** - click the **"Bonus"**-tagged card on the
   main menu to open this screen. It has three tabs:

   - **Emergency Services** - filter by **Service Type** (Fire Brigade, Ambulance, Police,
     Mental Health) and/or an **Area** keyword, then **Search**. Each card shows the
     contact's area, phone number, address and hours. Province-wide numbers (e.g. the
     national ambulance line `10177`) are included alongside area-specific contacts.
   - **Know Your Ward** - pick a ward from the dropdown (or leave it on "All Wards") to see
     that ward's councillor(s) and community caretaker(s), with their phone, email and
     office hours.
   - **Community Programs** - filter by category (**Computer Literacy**, **CV Guidelines**,
     **Interview Preparation**) to see available sessions, their schedule, location and
     contact info.

## Notes

- **Data storage**: reported issues, events and announcements are all stored in memory only
  (`IssueRepository` and `EventsRepository`), for the duration of the running application.
  There is no database or file persistence yet - this will be introduced as a later task
  (Service Request Status) is implemented.
- **Why Service Request Status is disabled**: the brief for this phase requires Report Issues
  and Local Events and Announcements; Service Request Status is deliberately left as a
  visibly disabled placeholder rather than removed, so the full intended navigation structure
  of the app is already in place for future work.
- **Data structures used by Local Events and Announcements**: each piece of the feature maps
  onto a different core data structure. `EventsRepository` indexes events by `Dictionary`
  (lookup by ID), `Dictionary<string, List<Event>>` (by category) and `SortedDictionary`
  (by date, for fast chronological range queries), with `HashSet`s tracking the distinct
  categories/dates seen; pending announcement requests are held in a `Queue` so they're
  published in the order they were submitted; a generic `PriorityQueue<T>` (hand-written, since
  .NET Framework 4.8 has no built-in one) ranks urgent-first/newest-first announcements and
  most-searched-first categories; and `RecommendationEngine` keeps a `Stack` of the resident's
  search history for the session, so "Previous Search" can step backwards through it.
- **Community Resources is a bonus feature**: it is not required by, and is not scored
  against, the marking rubric for Local Events and Announcements. It was added purely as a
  non-marked extra, reusing the same in-memory-only pattern and visual style as the rest of
  the app, and all of its seed data (names, phone numbers, addresses) is fictional placeholder
  content except for the handful of genuinely public national service numbers it lists (e.g.
  `10177`, `112`, the SADAG helpline).
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

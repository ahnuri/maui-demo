# Hanna Lab — Naming & Localization Standards

This document defines the conventions every contributor must follow for folders,
files, variables, and user-facing strings in this codebase. It is the
canonical reference; if a rule here conflicts with what the IDE auto-formats,
the rule here wins.

Pair with `Core/Theme/THEME.md` for the design-system tokens (colors / fonts /
spacing / radii / icon sizes / button heights).

---

## 1. Strings & localization

### 1.1 Single source of truth

**Every user-visible English string lives in `Core/Localization/TranslationStore.cs`.**

There is no second copy in `Resources/`, no `.resx` file, no per-page string
class. Adding a literal English string anywhere else is a code-review blocker.

### 1.2 Key naming convention

```
<Section>_<Element>[_<Variant>]
```

| Slot     | Allowed values                                                                                                                                                                                                                                       |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Section  | `AppShell`, `Shell`, `Flyout`, `Toolbar`, `PageToolbar`, `Home`, `Device`, `DeviceInfo`, `Measure`, `Halo`, `Photometer`, `Multimeter`, `LogHistory`, `Settings`, `Theme`, `Cloud`, `Profile`, `Register`, `Page_Language`, `Instrument`, `Common`, `Alert` |
| Element  | `Title`, `Subtitle`, `Header`, `Hint`, `Subtitle`, `Section`, `Button`, `Label`, `Placeholder`, `Message`, `Error`, `Empty`, `Status`, `Action`, `Confirm`, `Format`                                                                                  |
| Variant  | Disambiguates multi-state strings: `_Active` / `_Paused`, `_Lot` / `_Lod`, `_StrongAcidic` / `_Alkaline`, `_Single` / `_Many`                                                                                                                          |

#### Examples

```text
AppShell_Title_Home
Flyout_Login
Halo_Calibration_Title
Photometer_Tank_PickerSearchPlaceholder
Multimeter_LogRecall_DownloadCompleteTitle
Common_Cancel
LogHistory_DeleteMultiFormat
```

Anti-patterns:

```text
SignInBtn                   ← no section, abbreviation
HaloCalibrationTitle        ← missing underscores
Halo_calibration_title      ← lowercase
strong-acidic               ← kebab-case
HALO_CALIBRATION_TITLE      ← screaming-snake
```

### 1.3 Format strings

`string.Format` placeholders go inside the value, not the key. Multiple variants
are separate keys (no plural ICU syntax — keep it simple):

```text
Home_ConnectedOne     = "1 instrument connected"
Home_ConnectedMany    = "{0} instruments connected"
LogHistory_DeleteSingleFormat = "Delete \"{0}\" from Hanna Lab? This cannot be undone."
```

Resolve via `Loc.T("Home_ConnectedMany", count)`.

### 1.4 The `Common_*` section

Re-usable button / dialog primitives — always prefer these over adding a
section-specific Cancel / OK / Save / Delete key:

```text
Common_OK, Common_Cancel, Common_Done, Common_Save, Common_Delete,
Common_Edit, Common_Share, Common_Sync, Common_Continue, Common_Back,
Common_Yes, Common_No, Common_Loading, Common_Empty, Common_NA,
Common_Search, Common_SelectAll, Common_Clear
```

### 1.5 Adding a new string

1. Pick a `Section` + `Element` (extend the table above if no fit).
2. Add the English value in `En()` only. Falling-back to English is automatic
   for the six satellite languages.
3. Use the new key from a `LocalizedViewModelBase` subclass via
   `Loc.T("Key")` or `Loc.T("Key", arg0, arg1)`.
4. For pages without a ViewModel base, resolve through
   `Application.Current.Services.GetRequiredService<LocalizationService>()`
   and call `.T("Key")`.
5. **Never** add literal English text to a `.xaml` `Text="…"` attribute or a
   C# property assignment unless it is one of: a debug-only diagnostic
   message, an asset filename, a route ID, or a glyph character.

### 1.6 Reading strings in XAML

XAML cannot call `Loc.T` directly. Bind to a ViewModel string property whose
value is computed in `ApplyLocalization()`. Example pattern from `HomeViewModel`:

```csharp
public string CapabilityLiveStream { get; private set; } = string.Empty;

protected override void ApplyLocalization()
{
    CapabilityLiveStream = Loc.T("Home_Capability_LiveStream");
    OnPropertyChanged(nameof(CapabilityLiveStream));
}
```

```xml
<Label Text="{Binding CapabilityLiveStream}" />
```

### 1.7 Instrument-display strings

Display names, subtitles, opening messages, and navigation titles for every
instrument family flow through `Core/Devices/InstrumentRegistry.cs`. The
registry stores translation **keys** only; resolve them with the localized
accessors:

```csharp
InstrumentRegistry.GetDisplayName(kind, loc)
InstrumentRegistry.GetSubtitle(kind, loc)
InstrumentRegistry.GetOpeningMessage(kind, loc)
InstrumentRegistry.GetMeasureNavigationTitle(kind, loc)
```

---

## 2. Folder structure

### 2.1 Top-level layout

```
HannaUIDemo/
├── Core/                          ← cross-cutting infrastructure
│   ├── Auth/                      ← user / session services
│   ├── Constants/                 ← AppConstants (legacy; prefer Tokens)
│   ├── Converters/                ← IValueConverter implementations
│   ├── Demo/                      ← in-memory demo catalogs (BLE stand-ins)
│   ├── Devices/                   ← instrument families + icon resolver
│   ├── Helpers/                   ← static helper classes
│   ├── Localization/              ← LocalizationService + TranslationStore
│   ├── Mvvm/                      ← base ViewModels, ObservableObject infra
│   └── Theme/                     ← design tokens + colors + theme service
├── Features/                      ← per-screen feature verticals
│   ├── <Feature>/                 ← one folder per feature
│   └── Instruments/<Family>/      ← per-instrument feature sub-modules
├── Resources/                     ← images, fonts, raw assets, styles
├── Platforms/                     ← MAUI per-platform glue
└── Properties/                    ← .NET project metadata
```

### 2.2 Rules

* All folders are **PascalCase**. No camelCase, snake_case, kebab-case, or
  lowercase folders.
* Use **plural nouns** for folders that contain a collection of peers
  (`Converters/`, `Helpers/`, `Devices/`, `Instruments/`). Use **singular** for
  features (`Home/`, `Help/`, `Settings/`). When in doubt about
  singular/plural, follow the existing neighbour pattern.
* Co-locate XAML and code-behind: put `.xaml`, `.xaml.cs`, and the matching
  `*ViewModel.cs` in the same folder. Do **not** create a separate `Views/`
  subfolder for one feature (the empty `./Views/` directory at the repo root
  is a leftover and should be deleted).
* `Features/Instruments/<Family>/` holds family-specific UI and modules.
  `Features/Instruments/Logs/` holds the cross-instrument log-history shell
  (consider renaming to `LogHistory/` to remove the per-family / cross-family
  name collision).

### 2.3 Single domain, single folder

`Core/Devices/` and `Features/Device/` were grown organically and are
inconsistent (plural vs singular). Treat `Features/Devices/` as the target
name in any future refactor; the current `Features/Device/` is retained only
to avoid an immediate breaking rename.

---

## 3. File names

### 3.1 Suffix conventions

| Suffix          | Meaning                                       |
| --------------- | --------------------------------------------- |
| `*Page`         | `ContentPage` subclass — full-screen page     |
| `*View`         | `ContentView` subclass — reusable section     |
| `*ViewModel`    | MVVM ViewModel                                |
| `*Service`      | Long-lived application service                |
| `*Module`       | Pluggable feature module (instrument modules) |
| `*Registry`     | Static lookup catalog                         |
| `*Catalog`      | In-memory demo data                           |
| `*Resolver`     | Maps inputs → resources                       |
| `*Host`         | UI / nav host abstraction                     |
| `*Drawable`     | `IDrawable` implementation                    |
| `*Visuals`      | Static icon / accent helpers                  |
| `*Helper`       | Loose static helper (prefer no Helper suffix) |
| `*Extensions`   | C# extension method class                     |
| `*Converter`    | `IValueConverter` implementation              |
| `*Presenter`    | UI overlay / sheet controller                 |
| `*Navigator`    | Navigates between pages                       |
| `*Contributor`  | Contributes data to a shared list/catalog     |

### 3.2 One public type per file

* Every file contains **one** public type whose name matches the file name.
* File-private helper types (records, enums, drawables tightly coupled to the
  primary type) may live in the same file when they are <30 lines and
  unlikely to be reused.
* If a file already exceeds ~500 lines, split helper types into siblings.

### 3.3 PascalCase, no exceptions

Every `.cs` and `.xaml` file uses PascalCase. No `kebab-case`, no
`snake_case`. Exception: `*.xaml.cs` (the MAUI tooling enforces it).

### 3.4 Pages: prefer XAML

New pages should use `MyPage.xaml` + `MyPage.xaml.cs` rather than C#-only
construction. The half-dozen code-only pages in the repo today
(`Halo2SettingsPage.cs`, `TankPickerPage.cs`, etc.) are retained for
historical reasons but new pages must follow the XAML+code-behind pattern.

---

## 4. Variable & member naming

### 4.1 C# identifier conventions

| Kind                                    | Convention         | Example                                  |
| --------------------------------------- | ------------------ | ---------------------------------------- |
| Public / protected property             | `PascalCase`       | `DeviceName`, `IsLoggedIn`               |
| Public / protected method               | `PascalCase`       | `ApplyTheme`, `SetSelectedRoute`         |
| Private method                          | `PascalCase`       | `RefreshUserInfo`, `BuildOverlay`        |
| Constant (`const`, `static readonly`)   | `PascalCase`       | `PreferenceKey`, `DefaultBufferCount`    |
| Private instance field                  | `_camelCase`       | `_viewModel`, `_localization`            |
| `[ObservableProperty]` backing field    | `_camelCase`       | `_isLoading`, `_displayName`             |
|                                         | **must include explicit `private` modifier** |                  |
| Local variable                          | `camelCase`        | `index`, `currentValue`, `pageContent`   |
| Method parameter                        | `camelCase`        | `kind`, `loc`, `value`                   |
| Type parameter                          | `T` or `TName`     | `T`, `TItem`, `TViewModel`               |
| Interface                               | `I` + `PascalCase` | `IInstrumentMeasureModule`, `IDrawable`  |
| Enum value                              | `PascalCase`       | `InstrumentKind.Halo2`, `FlyoutNavAction.SignOut` |
| Record positional field                 | `PascalCase`       | `record Foo(int Count, string Name)` — **not** `count`, `name` |

### 4.2 `[ObservableProperty]` is always `private`

```csharp
[ObservableProperty] private bool _isLoading;        // ✓
[ObservableProperty] private string _email = "";     // ✓

[ObservableProperty] bool _isLoading;                // ✗ — visibility missing
[ObservableProperty] string Email;                   // ✗ — wrong casing, no backing field
```

### 4.3 No name shadowing

Avoid local / field names that shadow the surrounding type:

```csharp
// ✗ shadows the `Color` type
static (string Label, Color Color) GetPhStatus(double ph) => …;

// ✓
static (string Label, Color Swatch) GetPhStatus(double ph) => …;
```

### 4.4 Cryptic abbreviations are not allowed

* `Pfx` → `PreferencePrefix`
* `Pwd` → `Password`
* `Cfg` → `Configuration`
* `Hdr` → `Header`

Single-letter locals are tolerated only for loop indices (`i`, `j`), LINQ
expressions (`x`, `r`), and standard idioms (`e` in event handlers,
`sender` always written out).

### 4.5 No magic numbers in views

Always reach for `Tokens.*` (spacing, radius, font size, icon size, button
height, stroke width). New magic numbers in `.xaml` or `.xaml.cs` are
code-review blockers. See `Core/Theme/THEME.md` for the full token catalog.

---

## 5. Namespaces

### 5.1 Folder ≠ namespace (sometimes)

The codebase has a documented divergence: the `Theme` namespace is
`HannaUIDemo.Theme` (a global using in `GlobalUsings.cs`) even though the
files live under `Core/Theme/`. This is intentional — the namespace is
"flat" to keep XAML markup `xmlns:theme="clr-namespace:HannaUIDemo.Theme"`
short.

Within `Core/`, **always check** existing files in the folder before
choosing a namespace. The current canonical mapping is:

| Folder                  | Namespace                                |
| ----------------------- | ---------------------------------------- |
| `Core/Auth`             | `HannaUIDemo.Core.Auth`                  |
| `Core/Constants`        | `HannaUIDemo.Core.Constants`             |
| `Core/Converters`       | `HannaUIDemo.Core.Converters`            |
| `Core/Demo`             | `HannaUIDemo.Core.Demo`                  |
| `Core/Devices`          | `HannaUIDemo.Core.Devices`               |
| `Core/Helpers`          | `HannaUIDemo.Core.Helpers`               |
| `Core/Localization`     | `HannaUIDemo.Core.Localization`          |
| `Core/Mvvm`             | `HannaUIDemo.Core.Mvvm`                  |
| `Core/Theme`            | `HannaUIDemo.Theme` (intentionally flat) |
| `Features/<Feature>`    | `HannaUIDemo.Features.<Feature>`         |
| `Features/Instruments/<Family>` | `HannaUIDemo.Features.Instruments.<Family>` |

### 5.2 No `Theme` files outside `HannaUIDemo.Theme`

All design-system files (Tokens, Colors, SemanticResources, ThemeService,
AppThemeOption, ShellChrome) belong to `HannaUIDemo.Theme`. The previous
split (some files in `HannaUIDemo.Core.Theme`) has been collapsed into the
single namespace.

---

## 6. XAML conventions

* Element attributes appear in **logical groups**:
  1. layout (`Grid.Row`, `Grid.Column`, `Padding`, `Margin`)
  2. sizing (`HeightRequest`, `WidthRequest`)
  3. appearance (`BackgroundColor`, `Stroke`, `StrokeThickness`)
  4. content (`Text`, `Source`, `CornerRadius`)
  5. behavior (`Command`, `IsVisible`, `IsEnabled`)
* Use `{StaticResource …}` for design tokens (sizes, radii, fonts).
* Use `{DynamicResource …}` for theme-aware colors (so light/dark switches
  re-evaluate live).
* Bindings: prefer compiled bindings — set `x:DataType` on every root.

---

## 7. Migration cheat sheet

When porting an existing screen to these standards:

1. **Strings** — extract every literal English string into `TranslationStore.En()`
   using a `<Section>_<Element>` key, then bind through the ViewModel.
2. **Variables** — rename any `[ObservableProperty]` field missing `private`,
   rename cryptic abbreviations (`Pfx` → `PreferencePrefix`).
3. **Magic numbers** — replace `FontSize="13"`, `Padding="16"`, `CornerRadius="10"`
   with the equivalent `{StaticResource FontSizeBody}`, `{StaticResource SpacingLg}`,
   `{StaticResource RadiusMd}`.
4. **Icons** — replace asset names with `DeviceIconResolver.*Icon` references.
5. **Navigation titles** — never set `Title="…"` literally; always use
   `NavToolbar.Configure(page, "Translation_Key")` or
   `InstrumentRegistry.GetMeasureNavigationTitle(...)`.
6. **Theme colors** — replace `Color.FromArgb("#...")` literals with
   `ThemeColors.*` references or `{DynamicResource …}` bindings.

---

## 8. Code review checklist

- [ ] No new literal English strings outside `TranslationStore.cs`.
- [ ] All `[ObservableProperty]` declarations include `private`.
- [ ] All new files match a documented suffix convention.
- [ ] All new public types match their file name.
- [ ] No new magic numbers; use `Tokens.*` / `ThemeColors.*`.
- [ ] All translation keys follow `<Section>_<Element>[_<Variant>]`.
- [ ] All folders are PascalCase and follow plural-vs-singular conventions.
- [ ] Namespace matches the table in §5.1.
- [ ] No name shadowing (locals shadowing types, fields shadowing properties).
- [ ] Cryptic abbreviations expanded.

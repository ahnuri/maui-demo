# Hanna Lab — Design System

Single source of truth for every visual constant used in this app.
**Never put a magic number into a view.** Add a token here first.

```
Core/Theme/
├── Tokens.cs          ← C# entry point (font sizes, spacing, radii, icons, buttons)
├── ThemeColors.cs     ← Theme-aware colors (light/dark)
└── SemanticResources  ← Pushes ThemeColors into Application.Resources at runtime

Resources/Styles/
├── Colors.xaml        ← Brand-fixed colors (Primary, Success, White, Black, Gray ramp)
├── Tokens.xaml        ← Mirror of Tokens.cs as <x:Double>/<sys:String> resources
└── Styles.xaml        ← Implicit element styles (Button, Label, Entry, etc.)
```

## How to consume tokens

### C# (code-built UI)

```csharp
using HannaUIDemo.Theme;

var card = new Border
{
    Padding = Tokens.Spacing.Md,
    StrokeThickness = Tokens.Stroke.Thin,
    StrokeShape = new RoundRectangle { CornerRadius = Tokens.Radius.Lg },
    Stroke = ThemeColors.Divider,
    BackgroundColor = ThemeColors.Surface,
    Content = new Label
    {
        FontFamily = Tokens.FontFamily.Regular,
        FontSize = Tokens.FontSize.Body,
        TextColor = ThemeColors.OnSurface,
        Text = "Hello"
    }
};
```

### XAML

```xml
<Border
    Padding="{StaticResource SpacingMd}"
    StrokeThickness="{StaticResource StrokeThin}"
    BackgroundColor="{DynamicResource Surface}"
    Stroke="{DynamicResource Divider}">
    <Border.StrokeShape>
        <RoundRectangle CornerRadius="{StaticResource RadiusLg}" />
    </Border.StrokeShape>
    <Label
        FontSize="{StaticResource FontSizeBody}"
        TextColor="{DynamicResource OnSurface}"
        Text="Hello" />
</Border>
```

**Why `StaticResource` for numbers and `DynamicResource` for colors:**
- Numbers are constants — set once at app start, never change.
- Colors are theme-aware — re-pushed into `Application.Resources` when the user toggles dark mode, so XAML must observe them dynamically.

## Token reference (single-line summary)

| Group           | Scale step                                                | XAML key prefix  | C# accessor           |
|-----------------|-----------------------------------------------------------|------------------|-----------------------|
| Font family     | `Regular` / `Semibold`                                    | `FontFamily*`    | `Tokens.FontFamily.*` |
| Font size       | `Caption` 11 / `Small` 12 / `Body` 13 / `BodyLarge` 14 / `Subhead` 15 / `SubheadLarge` 16 / `Title` 17 / `SectionTitle` 18 / `LargeTitle` 20 / `Hero` 22 / `Display` 26 / `MeasureHero` 46 | `FontSize*`      | `Tokens.FontSize.*`   |
| Spacing         | `Xs` 4 / `Xxs` 6 / `Sm` 8 / `SmPlus` 10 / `Md` 12 / `MdPlus` 14 / `Lg` 16 / `Xl` 20 / `Xxl` 24 / `Xxxl` 28 | `Spacing*`       | `Tokens.Spacing.*`    |
| Radius          | `Xs` 6 / `Sm` 8 / `Md` 10 / `Lg` 12 / `Xl` 14 / `Xxl` 16 / `Xxxl` 18 / `Pill` 22 / `Hero` 28 | `Radius*`        | `Tokens.Radius.*`     |
| Stroke          | `Hairline` 0.5 / `Thin` 1 / `ThinPlus` 1.2 / `Thick` 1.5 / `Heavy` 2 | `Stroke*`        | `Tokens.Stroke.*`     |
| Icon image size | `Xs` 16 / `Sm` 18 / `SmPlus` 20 / `Md` 22 / `MdPlus` 24 / `Lg` 26 / `Xl` 28 / `Xxl` 32 / `Hero` 40 / `HeroLarge` 54 | `IconSize*`      | `Tokens.IconSize.*`   |
| Icon button     | `Sm` 36 / `SmPlus` 38 / `Md` 40 / `Lg` 44                 | `IconButton*`    | `Tokens.IconButton.*` |
| Avatar          | `Sm` 40 / `Md` 44 / `Lg` 52 / `Xl` 56                     | `Avatar*`        | `Tokens.Avatar.*`     |
| Button height   | `Sm` 40 / `Md` 48 / `Lg` 54                               | `ButtonHeight*`  | `Tokens.ButtonHeight.*` |

## Color tokens

Colors are not in `Tokens.cs` because they need to flip between light and dark
theme. They live in two places that are kept in sync:

- **`Core/Theme/ThemeColors.cs`** — C# entry point. All getters return the
  right color for the active theme (uses `Application.Current.RequestedTheme`).
- **`Core/Theme/SemanticResources.cs`** — pushes each `ThemeColors.*` getter
  into `Application.Resources` so XAML can reference them via
  `{DynamicResource OnSurface}`, `{DynamicResource Surface}`, etc.

Brand-fixed colors that do not change between themes live in
`Resources/Styles/Colors.xaml` and are referenced with `{StaticResource Primary}`,
`{StaticResource Success}`, etc.

### Available DynamicResource color keys

Surface / text:
`PageBackground`, `Surface`, `SurfaceSecondary`, `OnSurface`,
`OnSurfaceVariant`, `OnSurfaceMuted`, `Divider`

Primary brand tints:
`PrimarySubtleFill`, `PrimarySubtleBanner`, `PrimarySubtleStroke`

Halo / lab measure surfaces:
`LabCanvas`, `LabCard`, `LabCardElevated`, `LabRowStripe`, `LabBorder`,
`LabMuted`, `LabPrimaryText`, `LabSecondaryText`, `LabAccentCyan`,
`LabAccentOrange`, `LabEmerald`, `LabEmeraldMuted`, `LabIconButtonFill`,
`LabGradientStop`, `LabGradientEnd`, `LabGraphPlotFill`, `LabModeChipActive`,
`LabChipDisabled`, `LabTableHeaderBackground`, `LabTableHeaderText`

Semantic / status:
`LabWarning`, `LabWarningMuted`, `LabDanger`, `LabDangerSoft`, `LabDangerMuted`,
`LabSuccess`

Home hero banner:
`LabHeroGradientStart`, `LabHeroGradientMid`, `LabHeroGradientEnd`,
`LabHeroText`, `LabHeroMuted`, `LabHeroTileBackground`, `LabHeroTileStroke`,
`LabHeroBadgeFill`, `LabHeroBadgeStroke`, `LabHeroDeviceFrame`,
`LabHeroDeviceStroke`

Flyout:
`FlyoutBackground`, `FlyoutProfileCard`, `FlyoutActiveRow`, `FlyoutMenuGroup`,
`FlyoutLogoBadge`, `FlyoutIconBadge`

## Migration cheat sheet

Anywhere you see a literal number in a view, swap it for the matching token:

| Was                              | Use                                                |
|----------------------------------|----------------------------------------------------|
| `FontSize="14"`                  | `FontSize="{StaticResource FontSizeBodyLarge}"`    |
| `Padding="12"`                   | `Padding="{StaticResource SpacingMd}"`             |
| `CornerRadius="10"`              | `CornerRadius="{StaticResource RadiusMd}"`         |
| `StrokeThickness="1"`            | `StrokeThickness="{StaticResource StrokeThin}"`    |
| `WidthRequest="22" HeightRequest="22"` | `WidthRequest="{StaticResource IconSizeMd}" HeightRequest="{StaticResource IconSizeMd}"` |
| `Color.FromArgb("#EF4444")`      | `ThemeColors.LabDanger` (C#) / `{DynamicResource LabDanger}` (XAML) |

## Worked example

`Features/Instruments/Logs/LogHistoryDeviceLogsView.xaml` is the canonical
worked example: every spacing, padding, font size, radius, stroke thickness,
and icon size has been migrated to tokens. Use it as the template when you
clean up the other views.

## See also

- `Core/Constants/AppConstants.cs` — Brand colors plus named layout aliases
  (e.g. `RadiusCard` → `Tokens.Radius.Xxxl`). Kept for backwards compatibility.
- `Resources/Styles/Styles.xaml` — Implicit styles applied to every `Button`,
  `Label`, `Entry`, etc. Reference tokens via `{StaticResource …}` here too.

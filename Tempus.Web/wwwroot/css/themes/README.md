# Tempus Custom Radzen Themes

This directory contains the Tempus custom theme definitions for Radzen Blazor components.

## Theme Files

### Main Themes
- **tempus.scss** - Light mode theme with Tempus blue/teal branding
- **tempus-dark.scss** - Dark mode theme with brightened colors for dark backgrounds

### Base Themes
- **tempus-base.scss** - Base light mode theme (foundational variant)
- **tempus-dark-base.scss** - Base dark mode theme (foundational variant)

**Note**: Base themes are foundational variants that can be used for creating custom theme extensions. They have the `$base: true` flag set, which enables different component styling options in Radzen's theme system.

### SCSS Base Files
- **_variables.scss** - Theme metadata flags (material, fluent, standard, theme-dark, base)
- **_mixins.scss** - SCSS mixins for utilities, colors, and effects
- **_fonts.scss** - Fonts stub (fonts loaded in App.razor head instead)
- **_components.scss** - Components stub (outputs CSS variables only)

## Color Palette

### Light Mode (tempus.scss)
- **Primary**: #0EA5E9 (Blue) - Main brand color
- **Secondary**: #14B8A6 (Teal) - Accent brand color
- **Base Grayscale**: Slate palette (#f8fafc to #1e293b)

### Dark Mode (tempus-dark.scss)
- **Primary**: #38bdf8 (Lighter Blue) - Brightened for dark backgrounds
- **Secondary**: #2dd4bf (Lighter Teal) - Brightened for dark backgrounds
- **Base Grayscale**: Inverted slate palette (#0f172a to #f8fafc)

## Usage

### Option 1: Compile SCSS (Ready to Use!)

The required Radzen base files have been included, so you can compile the themes immediately:

```bash
# Navigate to themes directory
cd wwwroot/css/themes

# Compile main themes
sass tempus.scss tempus.css --no-source-map
sass tempus-dark.scss tempus-dark.css --no-source-map

# Compile base themes (optional)
sass tempus-base.scss tempus-base.css --no-source-map
sass tempus-dark-base.scss tempus-dark-base.css --no-source-map
```

**Note**: If you see deprecation warnings about `@import` and global built-in functions, these are harmless. The themes will compile successfully.

#### Compiled CSS Files

After compilation, you'll have CSS files that define Tempus-branded CSS variables. These files are already compiled and available:
- `tempus.css` - Light mode CSS variables
- `tempus-dark.css` - Dark mode CSS variables
- `tempus-base.css` - Base light mode CSS variables
- `tempus-dark-base.css` - Base dark mode CSS variables

To use the compiled theme in your application:

```razor
<!-- Add to App.razor <head> section -->
<link rel="stylesheet" href="css/themes/tempus.css" />
<link rel="stylesheet" href="css/themes/tempus-dark.css" />
```

Then use RadzenTheme with the software theme (which these CSS files will override):

```razor
<RadzenTheme Theme="software" @rendermode="RenderMode.InteractiveServer" />
```

### Option 2: CSS Variable Overrides (Recommended - Current Approach)

Instead of compiling custom themes, the current implementation uses CSS variable overrides to customize the `software` theme with Tempus colors. This approach:

- Works directly with Radzen's built-in theme system
- Supports RadzenAppearanceToggle for light/dark mode switching
- Requires no SCSS compilation
- See `simplified.css` for the CSS variable definitions

Current setup in App.razor:
```razor
<RadzenTheme Theme="software" @rendermode="RenderMode.InteractiveServer" />
```

## Important Notes

### Component Styles

The `_components.scss` file is a **stub** that only outputs CSS variables. It does NOT include the full Radzen component library styles because:

1. **Radzen.Blazor Package** - Component styles are already provided by the Radzen.Blazor NuGet package
2. **Complexity** - The full component library includes hundreds of SCSS files
3. **Not Needed** - Since you're using Radzen components directly, their styles are already loaded

The compiled CSS files define **CSS variables only**, which work with Radzen's existing component styles to apply the Tempus color scheme.

If you need to download the full Radzen component SCSS files for deep customization, see: https://github.com/radzenhq/radzen-blazor/tree/master/Radzen.Blazor/themes/components

### Font Loading

Fonts are loaded directly in the HTML `<head>` section (App.razor) rather than through CSS:

```razor
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200" rel="stylesheet" />
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet" />
<link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
```

**Why this approach:**
1. **Avoids Sass Processing** - Sass @import url() can cause issues with external URLs
2. **No 404 Errors** - Fonts load directly from Google's CDN, no local font files needed
3. **Better Performance** - Browser can start downloading fonts immediately during HTML parse
4. **Standard Practice** - HTML link tags are the recommended way to load Google Fonts

The `_fonts.scss` file is a stub that prevents SCSS compilation errors while keeping fonts in HTML where they belong.

## Theme Structure

All theme files follow the Radzen theme structure:

1. **Theme Metadata**
   - `$theme-name`: Theme identifier
   - `$theme-dark`: Boolean flag (dark mode only)

2. **Color Variables**
   - Base colors (white, black)
   - Semantic colors (primary, secondary, info, success, warning, danger)
   - 24 series colors for data visualization
   - Base grayscale palette (50-900)
   - Color variants (light, lighter, dark, darker)

3. **Theme Constants**
   - Border width and radius
   - Typography (font size, line height, font family)
   - Outline properties
   - Background colors

4. **Imports**
   - `variables`: Radzen base variables
   - `mixins`: SCSS mixins for color manipulation
   - `fonts`: Font definitions
   - `components`: Component-specific styles

## Customization

To customize the Tempus theme colors:

1. Edit the color variables in the SCSS files
2. Recompile to CSS
3. Or update the CSS variable overrides in your custom CSS files

## Future Enhancements

Potential improvements:

- Add base theme variants (e.g., tempus-base.scss)
- Add WCAG compliant variants (e.g., tempus-wcag.scss)
- Create an automated build process for SCSS compilation
- Add theme preview documentation with color swatches

## References

- [Radzen Blazor Themes](https://github.com/radzenhq/radzen-blazor/tree/master/Radzen.Blazor/themes)
- [Radzen Theme Documentation](https://www.radzen.com/documentation/blazor/themes/)
- [Tempus Brand Guidelines](../../../docs/BRANDING.md)

# Tempus Custom Radzen Themes

This directory contains the Tempus custom theme definitions for Radzen Blazor components.

## Theme Files

- **tempus.scss** - Light mode theme with Tempus blue/teal branding
- **tempus-dark.scss** - Dark mode theme with brightened colors for dark backgrounds

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

### Option 1: Compile SCSS (Requires SASS Compiler)

To use these custom themes, you need to compile them to CSS:

```bash
# Install SASS if not already installed
npm install -g sass

# Compile the themes
sass tempus.scss tempus.css
sass tempus-dark.scss tempus-dark.css
```

Then reference the compiled CSS in your application:

```razor
<RadzenTheme Theme="tempus" @rendermode="RenderMode.InteractiveServer" />
```

**Note**: The SCSS files import Radzen's base `variables`, `mixins`, `fonts`, and `components` files which are part of the Radzen.Blazor package. You may need to copy these from the Radzen.Blazor NuGet package location or download them from the Radzen GitHub repository.

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

## Theme Structure

Both theme files follow the Radzen theme structure:

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

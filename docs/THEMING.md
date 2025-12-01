# Tempus Custom Theme Colors

Tempus uses custom color overrides on top of Radzen Blazor's Material Design theme system to match the Tempus brand identity with a blue/teal color scheme.

## Overview

The custom themes provide:

- **Light Mode**: Extends Radzen's material theme with blue/teal brand colors
- **Dark Mode**: Extends Radzen's material-dark theme with brightened blue/teal colors
- **Radzen Integration**: Works seamlessly with RadzenAppearanceToggle component
- **Automatic Switching**: Theme switching handled by Radzen's built-in system

## How It Works

### Theme Switching

Radzen's `<RadzenAppearanceToggle />` component handles theme switching by adding/removing the `.rz-dark-mode` class on the `<body>` element. Our custom CSS files extend Radzen's themes by:

1. **Light Mode** (`theme-tempus-light.css`): Targets `body:not(.rz-dark-mode)`
2. **Dark Mode** (`theme-tempus-dark.css`): Targets `body.rz-dark-mode`

### File Structure

Theme files are located in `/wwwroot/css/`:

- `theme-tempus-light.css` - Color overrides for light mode
- `theme-tempus-dark.css` - Color overrides for dark mode

Both files:
- Override Radzen's CSS custom properties with Tempus brand colors
- Add visual enhancements (gradients, hover effects)
- Maintain full compatibility with all Radzen components

## Color Palette

### Light Mode Colors

| Semantic Color | Hex Value | Usage |
|----------------|-----------|-------|
| Primary (Blue) | `#0EA5E9` | Primary actions, links, active states |
| Secondary (Teal) | `#14B8A6` | Secondary actions, accents |
| Success (Green) | `#10b981` | Success states, confirmations |
| Info (Cyan) | `#06b6d4` | Informational messages |
| Warning (Amber) | `#f59e0b` | Warnings, cautionary states |
| Danger (Red) | `#ef4444` | Errors, destructive actions |

### Dark Mode Colors (Brightened)

| Semantic Color | Hex Value | Usage |
|----------------|-----------|-------|
| Primary (Blue) | `#38bdf8` | Primary actions (brightened for contrast) |
| Secondary (Teal) | `#2dd4bf` | Secondary actions (brightened) |
| Success (Green) | `#34d399` | Success states (brightened) |
| Info (Cyan) | `#22d3ee` | Informational messages (brightened) |
| Warning (Amber) | `#fbbf24` | Warnings (brightened) |
| Danger (Red) | `#f87171` | Errors (brightened) |

> **Note**: Dark mode colors are brightened to maintain WCAG contrast ratios on dark backgrounds.

## CSS Variable Overrides

Our themes override Radzen's CSS variables:

```css
/* Light Mode Example */
body:not(.rz-dark-mode) {
    --rz-primary: #0EA5E9;
    --rz-primary-light: #38bdf8;
    --rz-primary-lighter: rgba(14, 165, 233, 0.16);
    --rz-primary-dark: #0284c7;
    --rz-primary-darker: #0369a1;
    /* ... additional colors */
}

/* Dark Mode Example */
body.rz-dark-mode {
    --rz-primary: #38bdf8;
    --rz-primary-light: #7dd3fc;
    --rz-primary-lighter: rgba(56, 189, 248, 0.2);
    --rz-primary-dark: #0EA5E9;
    --rz-primary-darker: #0284c7;
    /* ... additional colors */
}
```

## Visual Enhancements

### Gradient Buttons

Primary buttons use brand gradient:

```css
body:not(.rz-dark-mode) .rz-button-primary {
    background: linear-gradient(135deg, #0EA5E9 0%, #14B8A6 100%);
}

body.rz-dark-mode .rz-button-primary {
    background: linear-gradient(135deg, #38bdf8 0%, #2dd4bf 100%);
}
```

### Enhanced Hover States

Cards, navigation items, and buttons have enhanced hover effects using the brand colors.

### Progress Bars

Progress bars use brand gradient for visual consistency.

## Customization Guide

### Changing Brand Colors

To customize colors, edit both theme files:

```css
/* In theme-tempus-light.css */
body:not(.rz-dark-mode) {
    --rz-primary: #YOUR_COLOR;
    --rz-secondary: #YOUR_COLOR;
}

/* In theme-tempus-dark.css */
body.rz-dark-mode {
    --rz-primary: #YOUR_BRIGHTENED_COLOR;
    --rz-secondary: #YOUR_BRIGHTENED_COLOR;
}
```

### Adding New Overrides

Follow the pattern of targeting Radzen classes with theme-specific selectors:

```css
/* Light mode */
body:not(.rz-dark-mode) .your-component {
    color: var(--rz-primary);
}

/* Dark mode */
body.rz-dark-mode .your-component {
    color: var(--rz-primary);
}
```

## Integration

### App.razor

Theme files are loaded after Radzen's base theme:

```html
<RadzenTheme Theme="material" @rendermode="RenderMode.InteractiveServer" />
<link rel="stylesheet" href="css/theme-tempus-light.css" />
<link rel="stylesheet" href="css/theme-tempus-dark.css" />
```

### MainLayout.razor

The `RadzenAppearanceToggle` component provides the theme switcher:

```razor
<RadzenAppearanceToggle />
```

No additional theme switcher is needed - Radzen handles everything.

## Accessibility

- **WCAG Compliance**: All color combinations meet WCAG AA standards
- **Dark Mode Colors**: Brightened specifically for proper contrast on dark backgrounds
- **Focus Indicators**: Maintained from Radzen's base theme
- **Semantic Colors**: Consistent meaning across both themes

## Browser Support

CSS Custom Properties (variables) are supported in:
- Chrome 49+
- Firefox 31+
- Safari 9.1+
- Edge 15+

## Troubleshooting

### Theme Not Switching

1. Verify `RadzenAppearanceToggle` is in the layout
2. Check browser console for CSS errors
3. Ensure theme files are loaded after Radzen's theme
4. Clear browser cache

### Colors Not Correct

1. Check CSS variable names match Radzen's naming
2. Verify selectors use `.rz-dark-mode` class correctly
3. Ensure custom CSS loads after Radzen's base theme

### White on White Text

This was caused by trying to override too much of Radzen's theme. The current implementation only overrides colors and specific enhancements, letting Radzen handle all base styling.

## Related Issues

- Issue #46 - Additional Custom Themes

## Technical Notes

### Why Not a Custom Theme Switcher?

We use Radzen's built-in `RadzenAppearanceToggle` because:
- It's already integrated with Radzen's theme system
- Handles persistence automatically
- Provides a consistent UI
- Works across all Radzen components
- Avoids duplicate functionality

### CSS Specificity

Our theme files use body-level selectors to ensure they override Radzen's defaults while maintaining proper specificity for component-level customizations.

## Future Enhancements

Potential improvements:
- [ ] High contrast variant
- [ ] Colorblind-friendly palette options
- [ ] Additional accent color variants
- [ ] Per-component color customization

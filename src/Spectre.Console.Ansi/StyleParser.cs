namespace Spectre.Console;

internal static class StyleParser
{
    public static Style Parse(string text)
    {
        return Parse(text, out var style, out var error)
            ? style
            : throw new InvalidOperationException(error);
    }

    public static bool TryParse(string text, out Style? style)
    {
        return Parse(text, out style, out _);
    }

    private static bool Parse(
        string text,
        [NotNullWhen(true)] out Style? result,
        [NotNullWhen(false)] out string? error)
    {
        var effectiveDecoration = (Decoration?)null;
        var effectiveForeground = (Color?)null;
        var effectiveBackground = (Color?)null;
        var effectiveLink = (string?)null;

        var parts = text.Split([' ']);
        var foreground = true;
        foreach (var part in parts)
        {
            if (part.Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (part.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                foreground = false;
                continue;
            }

            if (part.StartsWith("link=", StringComparison.OrdinalIgnoreCase))
            {
                if (effectiveLink != null)
                {
                    error = "A link has already been set.";
                    result = null;
                    return false;
                }

                effectiveLink = part.Substring(5);
                continue;
            }

            if (part.StartsWith("link", StringComparison.OrdinalIgnoreCase))
            {
                effectiveLink = Constants.EmptyLink;
                continue;
            }

            var decoration = DecorationTable.GetDecoration(part);
            if (decoration != null)
            {
                effectiveDecoration ??= Decoration.None;

                effectiveDecoration |= decoration.Value;
            }
            else
            {
                var color = Color.FromName(part);
                if (color == null)
                {
                    if (part.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        color = ParseHexColor(part, out error);
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            result = null;
                            return false;
                        }
                    }
                    else if (part.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
                    {
                        color = ParseRgbColor(part, out error);
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            result = null;
                            return false;
                        }
                    }
                    else if (int.TryParse(part, out var number))
                    {
                        switch (number)
                        {
                            case < 0:
                                error = $"Color number must be greater than or equal to 0 (was {number})";
                                result = null;
                                return false;
                            case > 255:
                                error = $"Color number must be less than or equal to 255 (was {number})";
                                result = null;
                                return false;
                            default:
                                color = number;
                                break;
                        }
                    }
                    else
                    {
                        error = !foreground
                            ? $"Could not find color '{part}'."
                            : $"Could not find color or style '{part}'.";

                        result = null;
                        return false;
                    }
                }

                if (foreground)
                {
                    if (effectiveForeground != null)
                    {
                        error = "A foreground color has already been set.";
                        result = null;
                        return false;
                    }

                    effectiveForeground = color;
                }
                else
                {
                    if (effectiveBackground != null)
                    {
                        error = "A background color has already been set.";
                        result = null;
                        return false;
                    }

                    effectiveBackground = color;
                }
            }
        }

        error = null;
        result = new Style(
            effectiveForeground,
            effectiveBackground,
            effectiveDecoration,
            effectiveLink);

        return true;
    }

    private static Color? ParseHexColor(string hex, out string? error)
    {
        error = null;
        hex = hex.ReplaceExact("#", string.Empty).Trim();

        try
        {
            if (!string.IsNullOrWhiteSpace(hex))
            {
                switch (hex.Length)
                {
                    case 6:
                        return new Color(
                            (byte)Convert.ToUInt32(hex.Substring(0, 2), 16),
                            (byte)Convert.ToUInt32(hex.Substring(2, 2), 16),
                            (byte)Convert.ToUInt32(hex.Substring(4, 2), 16));
                    case 3:
                        return new Color(
                            (byte)Convert.ToUInt32(new string(hex[0], 2), 16),
                            (byte)Convert.ToUInt32(new string(hex[1], 2), 16),
                            (byte)Convert.ToUInt32(new string(hex[2], 2), 16));
                }
            }
        }
        catch (Exception ex)
        {
            error = $"Invalid hex color '#{hex}'. {ex.Message}";
            return null;
        }

        error = $"Invalid hex color '#{hex}'.";
        return null;
    }

    private static Color? ParseRgbColor(string rgb, out string? error)
    {
        try
        {
            error = null;

            var normalized = rgb ?? string.Empty;
            if (normalized.Length >= 3)
            {
                // Trim parentheses
                normalized = normalized.Substring(3).Trim();

                if (normalized.StartsWith("(", StringComparison.OrdinalIgnoreCase) &&
                   normalized.EndsWith(")", StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized.Trim('(').Trim(')');

                    var parts = normalized.Split([','], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        return new Color(
                            (byte)Convert.ToInt32(parts[0], CultureInfo.InvariantCulture),
                            (byte)Convert.ToInt32(parts[1], CultureInfo.InvariantCulture),
                            (byte)Convert.ToInt32(parts[2], CultureInfo.InvariantCulture));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            error = $"Invalid RGB color '{rgb}'. {ex.Message}";
            return null;
        }

        error = $"Invalid RGB color '{rgb}'.";
        return null;
    }
}
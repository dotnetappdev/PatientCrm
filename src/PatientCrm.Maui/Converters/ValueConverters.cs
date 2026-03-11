using System.Globalization;
using PatientCrm.Maui.Models;

namespace PatientCrm.Maui.Converters;

/// <summary>Returns true if the value is non-null and non-empty string.</summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s) return !string.IsNullOrWhiteSpace(s);
        return value is not null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Inverts a bool value.</summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}

/// <summary>Returns true if an integer is greater than zero.</summary>
public class PositiveIntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Returns 'Active' or 'Inactive' based on a bool.</summary>
public class BoolToActiveConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "Active" : "Inactive";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Maps PatientStatus to a color.</summary>
public class PatientStatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PatientStatus status)
        {
            return status switch
            {
                PatientStatus.Active => Color.FromArgb("#007F3B"),
                PatientStatus.Inactive => Color.FromArgb("#768692"),
                PatientStatus.Deceased => Color.FromArgb("#4C4C4C"),
                PatientStatus.Transferred => Color.FromArgb("#FFB81C"),
                _ => Color.FromArgb("#005EB8")
            };
        }
        return Color.FromArgb("#005EB8");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

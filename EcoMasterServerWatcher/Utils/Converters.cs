using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoMasterServerWatcher.Utils
{
    public class CursorSelectorConverter : IValueConverter
    {
        public static readonly CursorSelectorConverter Instance = new();

        public static ConcurrentDictionary<StandardCursorType, Cursor> CachedCursors = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is string parameterString && Enum.TryParse<StandardCursorType>(parameterString, out var cursorType) && targetType.IsAssignableTo(typeof(Cursor)))
            {
                if (b)
                    return CachedCursors.GetOrAdd(cursorType, (_) => new Cursor(cursorType));
                else
                    return CachedCursors.GetOrAdd(StandardCursorType.Arrow, (_) => new Cursor(StandardCursorType.Arrow));
            }

            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

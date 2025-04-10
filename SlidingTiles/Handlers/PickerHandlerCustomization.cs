using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;
using SlidingTiles.Controls;

#if ANDROID
using Android.Graphics;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Platform;
#elif WINDOWS
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Graphics.Platform;
#elif IOS || MACCATALYST
using UIKit;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
#endif

namespace SlidingTiles.Handlers
{
    public static class PickerHandlerCustomization
    {
        public static void CustomizePickerHandler()
        {
            // Register the custom mapper for the Picker control
#if ANDROID
            PickerHandler.Mapper.AppendToMapping("CustomChevronColor", (handler, picker) =>
            {
                if (picker is null || handler.PlatformView is null)
                    return;

                // Get the chevron color from attached property
                var customColor = Color.FromArgb("#5D4037"); // Use direct value for reliability
                
                // Get the Android platform view
                var editText = handler.PlatformView;
                
                // Set the dropdown arrow color
                if (editText is AppCompatSpinner spinner)
                {
                    // Convert the MAUI color to Android color
                    var androidColor = customColor.ToAndroid();
                    
                    // Use reflection to get the dropdown arrow drawable
                    var method = spinner.Class.GetDeclaredMethod("getDropDownArrowDrawable");
                    if (method != null)
                    {
                        method.Accessible = true;
                        var drawable = method.Invoke(spinner);
                        if (drawable is Android.Graphics.Drawables.Drawable arrow)
                        {
                            // Set the tint color of the arrow
                            arrow.SetTint(androidColor);
                            spinner.DropDownVerticalOffset = 0;
                        }
                    }
                }
            });
#elif WINDOWS
            PickerHandler.Mapper.AppendToMapping("CustomChevronColor", (handler, picker) =>
            {
                if (picker is null || handler.PlatformView is null) 
                    return;

                // Get the chevron color - use direct value for reliability
                var customColor = Color.FromArgb("#5D4037");
                
                // Get the Windows platform view
                var comboBox = handler.PlatformView as ComboBox;
                if (comboBox != null)
                {
                    // Convert MAUI color to Windows color
                    // For Windows, we need to convert the MAUI color to a Windows UI color
                    var winColor = new Windows.UI.Color
                    {
                        A = (byte)(customColor.Alpha * 255),
                        R = (byte)(customColor.Red * 255),
                        G = (byte)(customColor.Green * 255),
                        B = (byte)(customColor.Blue * 255)
                    };
                    
                    // Create a new brush with the desired color - using fully qualified name to resolve ambiguity
                    var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(winColor);
                    
                    // Use resources to override the default ComboBox chevron style
                    comboBox.Resources["ComboBoxDropDownGlyphForeground"] = brush;
                }
            });
#elif IOS || MACCATALYST
            PickerHandler.Mapper.AppendToMapping("CustomChevronColor", (handler, picker) =>
            {
                if (picker is null || handler.PlatformView is null) 
                    return;

                // Get the chevron color - use direct value for reliability
                var customColor = Color.FromArgb("#5D4037");
                
                // Get the iOS platform view
                var pickerView = handler.PlatformView;
                
                // Set the text color of picker which affects the dropdown arrow on iOS
                pickerView.TintColor = customColor.ToPlatform();
            });
#endif
        }
    }
}
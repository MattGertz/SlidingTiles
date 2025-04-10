using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;
using SlidingTiles.Controls;

#if ANDROID
using Android.Graphics;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
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

                // Get the chevron color from attached property - using fully qualified name to resolve ambiguity
                var customColor = Microsoft.Maui.Graphics.Color.FromArgb("#5D4037"); // Use direct value for reliability
                
                // Get the Android platform view
                var platformView = handler.PlatformView;
                
                // Convert the MAUI color to Android color
                int androidColor = (int)(new Android.Graphics.Color(
                    (byte)(customColor.Red * 255),
                    (byte)(customColor.Green * 255),
                    (byte)(customColor.Blue * 255),
                    (byte)(customColor.Alpha * 255)
                ));
                
                try {
                    // Try to access the native EditText control inside the platform view
                    var property = platformView.GetType().GetProperty("EditText");
                    Android.Widget.EditText? editText = null;
                    
                    if (property != null)
                    {
                        editText = property.GetValue(platformView) as Android.Widget.EditText;
                    }
                    else
                    {
                        var field = platformView.GetType().GetField("EditText");
                        if (field != null)
                        {
                            editText = field.GetValue(platformView) as Android.Widget.EditText;
                        }
                    }
                    
                    if (editText != null)
                    {
                        // Set the right compound drawable's tint (the dropdown arrow)
                        var drawables = editText.GetCompoundDrawables();
                        if (drawables != null && drawables.Length > 2 && drawables[2] != null)
                        {
                            drawables[2].SetTint(androidColor);
                        }
                    }
                }
                catch {
                    // Fallback approach if the above doesn't work
                    if (platformView is Android.Views.View view)
                    {
                        // Use BackgroundTintList which is supported on Android 21+
                        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                        {
                            view.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(new Android.Graphics.Color(androidColor));
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
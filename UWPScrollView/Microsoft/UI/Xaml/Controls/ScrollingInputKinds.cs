using System;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Defines constants that specify kinds of input that are ignored by a <see cref="ScrollView"/>.
/// </summary>
/// <remarks>
/// This enumeration supports a bitwise combination of its member values.
/// </remarks>
[Flags]
public enum ScrollingInputKinds
{
    /// <summary>
    /// No inputs are ignored.
    /// </summary>
    None = 0,

    /// <summary>
    /// Touch input is ignored.
    /// </summary>
    Touch = 1,

    /// <summary>
    /// Pen input is ignored.
    /// </summary>
    Pen = 2,

    /// <summary>
    /// Mouse wheel input is ignored.
    /// </summary>
    MouseWheel = 4,

    /// <summary>
    /// Keyboard input is ignored.
    /// </summary>
    Keyboard = 8,

    /// <summary>
    /// Gamepad input is ignored.
    /// </summary>
    Gamepad = 16,

    /// <summary>
    /// All kinds of input are ignored.
    /// </summary>
    All = 255
}
// ApplicationIdleService.cs
// Provides an ApplicationIdle event that fires once after input processing settles.
// Registers global class handlers on TopLevel for pointer, keyboard, and text input,
// then posts a single idle callback via Dispatcher.UIThread.Post. Multiple inputs between
// dispatches are coalesced into one idle event. Works across all top-level windows.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AvPurplePen
{
    /// <summary>
    /// Static service that provides an ApplicationIdle event, similar to
    /// WinForms Application.Idle. Fires once after input processing settles.
    /// Call Initialize() once at application startup.
    /// </summary>
    public static class ApplicationIdleService
    {
        /// <summary>
        /// Raised once when the application becomes idle after processing input.
        /// </summary>
        public static event EventHandler? ApplicationIdle;

        // True when an idle callback has been posted but not yet executed.
        private static bool idleQueued = false;

        /// <summary>
        /// Initialize the service by registering global class handlers on TopLevel
        /// for all input events. These fire for every TopLevel window in the application.
        /// Must be called once at application startup.
        /// </summary>
        public static void Initialize()
        {
            // Global class handlers fire for all instances of the specified type,
            // so these cover every current and future TopLevel window.
            InputElement.PointerPressedEvent.AddClassHandler<TopLevel>(OnInput, handledEventsToo: true);
            InputElement.PointerReleasedEvent.AddClassHandler<TopLevel>(OnInput, handledEventsToo: true);
            InputElement.PointerMovedEvent.AddClassHandler<TopLevel>(OnInput, handledEventsToo: true);
            InputElement.PointerWheelChangedEvent.AddClassHandler<TopLevel>(OnInput, handledEventsToo: true);
            InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnInput, handledEventsToo: true);
            InputElement.KeyUpEvent.AddClassHandler<TopLevel>(OnInput, handledEventsToo: true);
            InputElement.TextInputEvent.AddClassHandler<TopLevel>(OnInput, handledEventsToo: true);

            // Queue an initial idle event so subscribers get notified at startup,
            // matching WinForms Application.Idle behavior.
            idleQueued = true;
            Dispatcher.UIThread.Post(RaiseIdle, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Called on any input event on any TopLevel. Queues a single idle callback
        /// if one is not already pending.
        /// </summary>
        private static void OnInput(object? sender, RoutedEventArgs e)
        {
            if (!idleQueued) {
                idleQueued = true;
                Dispatcher.UIThread.Post(RaiseIdle, DispatcherPriority.ApplicationIdle);
            }
        }

        /// <summary>
        /// Fires the ApplicationIdle event and resets the queued flag.
        /// </summary>
        private static void RaiseIdle()
        {
            idleQueued = false;
            ApplicationIdle?.Invoke(null, EventArgs.Empty);
        }
    }
}

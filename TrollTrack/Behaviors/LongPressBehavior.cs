using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace TrollTrack.Behaviors
{
    public class LongPressBehavior : Behavior<View>
    {
        private Timer? _timer;
        private bool _isPressed;

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(
                nameof(Command),
                typeof(ICommand),
                typeof(LongPressBehavior));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(
                nameof(CommandParameter),
                typeof(object),
                typeof(LongPressBehavior));

        public static readonly BindableProperty DurationProperty =
            BindableProperty.Create(
                nameof(Duration),
                typeof(int),
                typeof(LongPressBehavior),
                defaultValue: 500); // 500ms default

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public int Duration
        {
            get => (int)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);

            var touchGesture = new PointerGestureRecognizer();
            touchGesture.PointerPressed += OnPointerPressed;
            touchGesture.PointerReleased += OnPointerReleased;
            touchGesture.PointerExited += OnPointerExited;

            bindable.GestureRecognizers.Add(touchGesture);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);

            foreach (var gesture in bindable.GestureRecognizers.OfType<PointerGestureRecognizer>())
            {
                gesture.PointerPressed -= OnPointerPressed;
                gesture.PointerReleased -= OnPointerReleased;
                gesture.PointerExited -= OnPointerExited;
            }

            _timer?.Dispose();
        }

        private void OnPointerPressed(object? sender, PointerEventArgs e)
        {
            _isPressed = true;
            _timer = new Timer(
                _ =>
                {
                    if (_isPressed && Command?.CanExecute(CommandParameter) == true)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Command.Execute(CommandParameter);
                        });
                    }
                },
                null,
                Duration,
                Timeout.Infinite);
        }

        private void OnPointerReleased(object? sender, PointerEventArgs e)
        {
            CancelPress();
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            CancelPress();
        }

        private void CancelPress()
        {
            _isPressed = false;
            _timer?.Dispose();
            _timer = null;
        }
    }
}
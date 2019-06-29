using Microsoft.Toolkit.Uwp.UI.Animations.Expressions;
using System;
using System.Numerics;
using Windows.Devices.Input;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using EF = Microsoft.Toolkit.Uwp.UI.Animations.Expressions.ExpressionFunctions;

namespace SpringAnimationSample
{
    public sealed partial class MainPage : Page
    {
        private readonly Compositor _compositor;
        private VisualInteractionSource _interactionSource;

        private readonly Visual _root;

        public MainPage()
        {
            InitializeComponent();

            _compositor = Window.Current.Compositor;
            _root = ElementCompositionPreview.GetElementVisual(Root);

            Loaded += (s, e) =>
            {
                _interactionSource = VisualInteractionSource.Create(_root);
                _interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
                _interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;
                _interactionSource.IsPositionXRailsEnabled = false;
                _interactionSource.IsPositionYRailsEnabled = false;

                ConfigSpringBasedTracker(Image1, 240.0d, 0.4f);
                ConfigSpringBasedTracker(Image2, 180.0d, 0.2f, -240.0f);
                ConfigSpringBasedTracker(Image3, 120.0d, 0.15f, 240.0f);
            };
        }

        private void ConfigSpringBasedTracker(UIElement element, double periodInMs, 
            float dampingRatio, float finalValue = 0.0f, double delayInMs = 0.0f)
        {
            ElementCompositionPreview.SetIsTranslationEnabled(element, true);

            var tracker = InteractionTracker.Create(_compositor);
            tracker.InteractionSources.Add(_interactionSource);
            tracker.MaxPosition = new Vector3(Root.RenderSize.ToVector2(), 0.0f);
            tracker.MinPosition = -tracker.MaxPosition;

            var modifier = InteractionTrackerInertiaNaturalMotion.Create(_compositor);
            var springAnimation = _compositor.CreateSpringScalarAnimation();
            springAnimation.Period = TimeSpan.FromMilliseconds(periodInMs);
            springAnimation.DampingRatio = dampingRatio;
            springAnimation.FinalValue = finalValue;
            // This will cause a black screen for a few seconds. Bug??
            springAnimation.DelayTime = TimeSpan.FromMilliseconds(delayInMs);

            modifier.Condition = _compositor.CreateExpressionAnimation("true");
            modifier.NaturalMotion = springAnimation;

            tracker.ConfigurePositionXInertiaModifiers(new InteractionTrackerInertiaModifier[] { modifier });
            tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { modifier });

            var imageVisual = ElementCompositionPreview.GetElementVisual(element);
            imageVisual.StartAnimation("Translation", -tracker.GetReference().Position);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
            {
                try
                {
                    _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Root));
                }
                catch (Exception)
                {
                    // Catch unauthorized input.
                }
            }
        }
    }
}
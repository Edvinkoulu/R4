using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.Globalization;

namespace Village_Newbies.Behaviors
{
    public class NumericValidationBehavior : Behavior<Entry>
    {
        public static readonly BindableProperty IsDoubleProperty =
            BindableProperty.Create(nameof(IsDouble), typeof(bool), typeof(NumericValidationBehavior), true);

        public bool IsDouble
        {
            get => (bool)GetValue(IsDoubleProperty);
            set => SetValue(IsDoubleProperty, value);
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            bindable.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(bindable);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.NewTextValue))
            {
                ((Entry)sender).Text = "";
                return;
            }

            if (IsDouble)
            {
                if (!double.TryParse(args.NewTextValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _))
                {
                    ((Entry)sender).Text = args.OldTextValue;
                }
            }
            else
            {
                if (!int.TryParse(args.NewTextValue, out _))
                {
                    ((Entry)sender).Text = args.OldTextValue;
                }
            }
        }
    }
}
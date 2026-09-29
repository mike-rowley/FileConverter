// <copyright file="PathTextBlock.cs" company="AAllard">License: http://www.gnu.org/licenses/gpl.html GPL version 3.</copyright>

namespace FileConverter.Controls
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    /// <summary>
    /// Displays a file path on a single line. When the path is too long for the available width, characters are removed
    /// from the middle of its folder part and replaced by an ellipsis, so the file name stays visible: it is the part that
    /// tells the files apart, and the one that carries the name of the conversion preset.
    /// </summary>
    /// <remarks>
    /// TextBlock can only trim the end of a text, which is precisely the part of a path that has to be kept.
    /// </remarks>
    public class PathTextBlock : FrameworkElement
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(PathTextBlock),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(PathTextBlock));
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(PathTextBlock));
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(PathTextBlock));
        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(PathTextBlock));
        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(PathTextBlock));

        private const string Ellipsis = "…";
        private static readonly char[] PathSeparators = { '\\', '/' };

        public string Text
        {
            get => (string)this.GetValue(PathTextBlock.TextProperty);
            set => this.SetValue(PathTextBlock.TextProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush)this.GetValue(PathTextBlock.ForegroundProperty);
            set => this.SetValue(PathTextBlock.ForegroundProperty, value);
        }

        public FontFamily FontFamily
        {
            get => (FontFamily)this.GetValue(PathTextBlock.FontFamilyProperty);
            set => this.SetValue(PathTextBlock.FontFamilyProperty, value);
        }

        public double FontSize
        {
            get => (double)this.GetValue(PathTextBlock.FontSizeProperty);
            set => this.SetValue(PathTextBlock.FontSizeProperty, value);
        }

        public FontStyle FontStyle
        {
            get => (FontStyle)this.GetValue(PathTextBlock.FontStyleProperty);
            set => this.SetValue(PathTextBlock.FontStyleProperty, value);
        }

        public FontWeight FontWeight
        {
            get => (FontWeight)this.GetValue(PathTextBlock.FontWeightProperty);
            set => this.SetValue(PathTextBlock.FontWeightProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            string text = this.Text;
            if (string.IsNullOrEmpty(text))
            {
                return new Size(0, this.Format(" ").Height);
            }

            FormattedText formattedText = this.Format(text);
            return new Size(Math.Min(formattedText.WidthIncludingTrailingWhitespace, availableSize.Width), formattedText.Height);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            string text = this.Text;
            double width = this.RenderSize.Width;
            if (string.IsNullOrEmpty(text) || width <= 0)
            {
                return;
            }

            FormattedText formattedText = this.Format(this.ShortenToFit(text, width));

            // Safety net for a width too small even for the shortest form: never draw outside of the element.
            formattedText.MaxTextWidth = width;
            formattedText.Trimming = TextTrimming.CharacterEllipsis;

            drawingContext.DrawText(formattedText, new Point(0, 0));
        }

        private static string ElideMiddle(string text, int keptLength)
        {
            // Keep a bit more of the end, where the name of the preset and the extension are.
            int keptAtStart = keptLength * 2 / 5;
            int keptAtEnd = keptLength - keptAtStart;
            return text.Substring(0, keptAtStart) + PathTextBlock.Ellipsis + text.Substring(text.Length - keptAtEnd);
        }

        private string ShortenToFit(string path, double width)
        {
            if (this.Fits(path, width))
            {
                return path;
            }

            // Like the path ellipsis of the Windows shell: keep everything after the last separator, and as much of the
            // beginning of the path as fits in front of it.
            int separatorIndex = path.LastIndexOfAny(PathTextBlock.PathSeparators);
            if (separatorIndex > 0)
            {
                string folder = path.Substring(0, separatorIndex);
                string fileName = path.Substring(separatorIndex);
                if (this.Fits(PathTextBlock.Ellipsis + fileName, width))
                {
                    int keptLength = this.LongestFittingLength(folder.Length - 1, length => folder.Substring(0, length) + PathTextBlock.Ellipsis + fileName, width);
                    return folder.Substring(0, keptLength) + PathTextBlock.Ellipsis + fileName;
                }

                path = path.Substring(separatorIndex + 1);
            }

            // Even the file name alone is too long: remove characters from its middle.
            string name = path;
            int keptInName = this.LongestFittingLength(name.Length - 1, length => PathTextBlock.ElideMiddle(name, length), width);
            return PathTextBlock.ElideMiddle(name, keptInName);
        }

        /// <summary>
        /// Finds the largest length, between 0 and the given maximum, for which the candidate text fits in the width.
        /// </summary>
        private int LongestFittingLength(int maximumLength, Func<int, string> candidate, double width)
        {
            int low = 0;
            int high = maximumLength;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (this.Fits(candidate(middle), width))
                {
                    low = middle;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return low;
        }

        private bool Fits(string text, double width)
        {
            return this.Format(text).WidthIncludingTrailingWhitespace <= width;
        }

        private FormattedText Format(string text)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                this.FlowDirection,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, FontStretches.Normal),
                this.FontSize,
                this.Foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }
    }
}

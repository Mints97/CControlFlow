namespace DrawCFGraph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        const string fontFamily = "Verdana";

        const double lineThickness = 4;

        private double deltaWidth = 0;
        private double deltaHeight = 0;

        private double moveX = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void resizeCanvas()
        {
            this.drawingCanvas.Width += this.deltaWidth + this.moveX;
            this.drawingCanvas.Height += this.deltaHeight;

            this.scrollViewerDisplay.ScrollToHorizontalOffset(scrollViewerDisplay.ScrollableWidth / 2);

            foreach (System.Windows.UIElement child in this.drawingCanvas.Children)
            {
                child.RenderTransform = new System.Windows.Media.TransformGroup()
                {
                    Children = new System.Windows.Media.TransformCollection()
                    {
                        child.RenderTransform,
                        new System.Windows.Media.TranslateTransform(this.moveX, 0)
                    }
                };
            }
        }

        private void updateCounters(double x1, double y1, double x2, double y2)
        {
            if (x1 < 0 && -x1 > this.moveX)
            {
                this.moveX = -x1 + lineThickness / 2;
                if (-x1 > this.deltaWidth)
                    this.deltaWidth = -x1 + lineThickness / 2;
            }

            if (x1 > this.drawingCanvas.Width && x1 - this.drawingCanvas.Width > this.deltaWidth)
                this.deltaWidth = x1 - this.drawingCanvas.Width + lineThickness / 2;
            if (y1 > this.drawingCanvas.Height && y1 - this.drawingCanvas.Height > this.deltaHeight)
                this.deltaHeight = y1 - this.drawingCanvas.Height + lineThickness / 2;

            if (x2 < 0 && -x2 > this.moveX)
            {
                this.moveX = -x2 + lineThickness / 2;
                if (-x2 > this.deltaWidth)
                    this.deltaWidth = -x2 + lineThickness / 2;
            }

            if (x2 > this.drawingCanvas.Width && x2 - this.drawingCanvas.Width > this.deltaWidth)
                this.deltaWidth = x2 - this.drawingCanvas.Width + lineThickness / 2;
            if (y2 > this.drawingCanvas.Height && y2 - this.drawingCanvas.Height > this.deltaHeight)
                this.deltaHeight = y2 - this.drawingCanvas.Height + lineThickness / 2;
        }

        private void drawGraph(IDrawable start, double startX, double startY)
        {
            start.draw(startX, startY,
                start.width(
                    (codeString, fontHeight) =>
                    {
                        System.Windows.Media.FormattedText formattedText = new System.Windows.Media.FormattedText(codeString,
                            System.Globalization.CultureInfo.GetCultureInfo("en-us"), System.Windows.FlowDirection.LeftToRight,
                            new System.Windows.Media.Typeface(fontFamily), fontHeight, System.Windows.Media.Brushes.Transparent);

                        return formattedText.Width;
                    }),

                (x1, y1, x2, y2) =>
                {
                    System.Windows.Shapes.Line line = new System.Windows.Shapes.Line()
                    {
                        Visibility = System.Windows.Visibility.Visible,
                        StrokeThickness = lineThickness,
                        Stroke = System.Windows.Media.Brushes.Black,
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2
                    };

                    updateCounters(x1, y1, x2, y2);

                    this.drawingCanvas.Children.Add(line);
                },

                (text, fontHeight, x, y) =>
                {
                    System.Windows.Controls.TextBlock textBlock = new System.Windows.Controls.TextBlock()
                    {
                        FontSize = fontHeight,
                        Text = text,
                        RenderTransform = new System.Windows.Media.TranslateTransform(x, y),

                        FontFamily = new System.Windows.Media.FontFamily(fontFamily)
                    };

                    this.drawingCanvas.Children.Add(textBlock);
                });

            resizeCanvas();
        }

        private void displayProgram(string program)
        {
            double currY = 10;

            this.drawingCanvas.Children.Clear();

            this.drawingCanvas.Height = 0;
            this.drawingCanvas.Width = 0;
            this.deltaWidth = 0;
            this.deltaHeight = 0;
            this.moveX = 0;

            try
            {
                CParser.CToken[] tokens = CParser.CLexer.parseTokens(program);

                foreach (CGrammar.CFunctionDefinition matchedFunction in CParser.CParser.getFunctionContents(tokens))
                {
                    drawGraph(matchedFunction, 0, currY);
                    currY += matchedFunction.height + 60;
                }
            }
            catch (System.ArgumentException exc)
            {
                System.Windows.MessageBox.Show("There is an error in your source code: " + exc.Message);
            }
            catch (System.InvalidOperationException exc)
            {
                System.Windows.MessageBox.Show("There is an error in your source code: " + exc.Message);
            }
            catch
            {
                System.Windows.MessageBox.Show(
                    "An unexpected error occured! Please send a copy of the " +
                    "source code you used to max.mints@mail.ru. A bugfix will soon be underway!");
            }
        }

        private void drawingCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.scrollViewerDisplay.ScrollToHorizontalOffset((e.NewSize.Width - this.ActualWidth) / 2);
        }

        private void Window_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.scrollViewerDisplay.ScrollToHorizontalOffset((this.drawingCanvas.ActualWidth - e.NewSize.Width) / 2);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.DefaultExt = ".c";
            dialog.Filter = "C source files (*.c)|*.c;*.h|All Files|*";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    displayProgram(System.IO.File.ReadAllText(dialog.FileName));
                }
                catch
                {
                    System.Windows.MessageBox.Show("error reading file " + dialog.FileName);
                }
            }
        }
    }
}

using Microsoft.Maui.Handlers;
using Syncfusion.Maui.PdfToImageConverter;
using Syncfusion.Maui.PdfViewer;
using System.Reflection;

namespace PdfZoomCallout
{
    public partial class MainPage : ContentPage
    {
        PdfToImageConverter? imageConverter;
        bool showHint = true;

        public MainPage()
        {
            InitializeComponent();
            PdfViewer.AnnotationSettings.Square.BorderWidth = 1;
            PdfViewer.DocumentSource = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("PdfZoomCallout.Assets.pdf_succinctly.pdf");
        }

        // Returns the calculated scale factor to show the specific portion without quality loss.
        float GetScaleFactor(float selectedAreaWidth, float selectedAreaHeight)
        {
            // Compare the size of the selected area in the PDF and callout image popup size to get the best quality factor.
            if (selectedAreaWidth > selectedAreaHeight)
            {
                return (float)(PdfViewer.Bounds.Width - CalloutImage.Margin.HorizontalThickness) / selectedAreaWidth;
            }
            else
            {
                return (float)(PdfViewer.Bounds.Height - CalloutImage.Margin.VerticalThickness) / selectedAreaHeight;
            }
        }

        /// <summary>
        /// Closes the callout popup.
        /// </summary>
        void CloseCalloutPopup()
        {
            CalloutImage.Source = null;
            CalloutPopup.IsVisible = false;
        }

        /// <summary>
        /// Displays the portion of image from the given source.
        /// </summary>
        void ShowCalloutPortion(Stream? source)
        {
            CalloutPopup.IsVisible = true;
            CalloutImage.Source = ImageSource.FromStream(() => source);
        }

        #region Event handlers
        private void PdfViewer_DocumentLoaded(object sender, EventArgs e)
        {
            // Initiate image converter for the document loaded in the PDF Viewer.
            imageConverter?.Dispose();
            if (PdfViewer.DocumentSource is Stream documentStream)
            {
                documentStream.Position = 0;
                Stream stream = new MemoryStream();
                documentStream.CopyTo(stream);
                stream.Position = 0;
                imageConverter = new PdfToImageConverter(stream);
            }
        }

        private void PdfViewer_AnnotationAdded(object sender, Syncfusion.Maui.PdfViewer.AnnotationEventArgs e)
        {
            // Get the bounds of the rectangle drawn to crop and show the specific portion.
            Annotation? annotation = e.Annotation;
            if (annotation != null && annotation is SquareAnnotation squareAnnotation)
            {
                float scaleFactor = GetScaleFactor(squareAnnotation.Bounds.Width, squareAnnotation.Bounds.Height);
                Stream? imageStream = imageConverter?.Convert(annotation.PageNumber - 1,
                    squareAnnotation.Bounds, scaleFactor);
                ShowCalloutPortion(imageStream);
                PdfViewer.RemoveAnnotation(annotation);
            }
        }

        private async void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value == true)
            {
                // Enable rectangle drawing mode.
                PdfViewer.AnnotationMode = AnnotationMode.Square;
                if (showHint)
                {
                    // Show hint for zoom and highlight a specific portion.
                    showHint = await DisplayAlert("Zoom and Highlighter", "To zoom and highlight a specific area, draw a rectangle over it.", "OK", "Don't show again");
                }
            }
            else
            {
                // Disable the rectangle drawing mode.
                PdfViewer.AnnotationMode = AnnotationMode.None;
                // Close callout popup.
                CloseCalloutPopup();
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            CloseCalloutPopup();
        }

        /// <summary>
        /// This handler is to remove the "On/Off text in Windows platform"
        /// </summary>
        private void Switch_HandlerChanged(object sender, EventArgs e)
        {
#if WINDOWS
            if (sender is Switch calloutModeSwitch &&
                calloutModeSwitch.Handler is SwitchHandler switchHandler)
            {
                switchHandler.PlatformView.OnContent = "Callout Mode";
                switchHandler.PlatformView.OffContent = "Callout Mode";
            }
#endif
        }
        #endregion
    }
}
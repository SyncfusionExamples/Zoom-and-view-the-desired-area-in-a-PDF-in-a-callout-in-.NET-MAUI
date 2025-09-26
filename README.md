# Zoom and view the desired area in a PDF in a callout in .NET MAUI

This repository contains the example which demonstrates how to zoom and view the desired text or area in a PDF in a callout using Syncfusion&reg; .NET MAUI PDF Viewer.

## Prerequisites

1. A .NET MAUI project set up.
2. The [Syncfusion.Maui.PdfViewer](https://www.nuget.org/packages/Syncfusion.Maui.PdfViewer) package.

## How to Zoom and view the desired area in a PDF in a callout in .NET MAUI

### 1. Install Required NuGet Package

Create a new [MAUI App](https://dotnet.microsoft.com/en-us/learn/maui/first-app-tutorial/create), install the [Syncfusion.Maui.PdfViewer](https://www.nuget.org/packages/Syncfusion.Maui.PdfViewer) package using either.

* NuGet Package Manager
* NuGet CLI

### 3. Initialize and Configure the PDF Viewer and Callout Layout

Start by adding the Syncfusion PDF Viewer control to your XAML file.

#### a. Add the Syncfusion namespace in `MainPage.xaml`

This namespace enables access to the PDF Viewer control.

**XAML:**

```xaml
    xmlns:pv="clr-namespace:Syncfusion.Maui.PdfViewer;assembly=Syncfusion.Maui.PdfViewer"
```

#### b. Add the PDF Viewer to your layout

**XAML:**

```xaml
     <Grid>
        <pv:SfPdfViewer x:Name="pdfViewer"/>
     </Grid>
 ```

#### c. Add Callout Layout.

Layout to zoom and view specific area in the PDF, and `TapGestureRecognizer` is added to the layout.

**XAML:**

```xaml
  
      <!--Callout view-->
      <Grid BackgroundColor="#33000000" x:Name="CalloutPopup" IsVisible="False">
          <Grid.GestureRecognizers>
              <TapGestureRecognizer
                   NumberOfTapsRequired="1" />
          </Grid.GestureRecognizers>
          <Image 
              x:Name="CalloutImage"
              VerticalOptions="Center" HorizontalOptions="Center" 
              Margin="{OnPlatform 40, iOS=20, Android=20}">
              <Image.GestureRecognizers>
                  <TapGestureRecognizer NumberOfTapsRequired="1" />
              </Image.GestureRecognizers>
          </Image>
      </Grid>
```

#### d. Add switch.

To enable and disable the callout layout add switch.

**XAML:**

```xaml
      <HorizontalStackLayout HorizontalOptions="End" Margin="0,0,12,0">
          <Switch IsToggled="False"/>
      </HorizontalStackLayout>
```

### 4. Calculate the scale factor to show the specific portion without quality loss.

The `GetScaleFactor` method determines the appropriate zoom level to clearly display a selected region of a PDF within a callout popup, ensuring the content fits well and maintains visual quality.

**C#:**

```csharp
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
```

### 5. Create DocumentLoaded and AnnoatationAdded event handler.

#### a. DocumentLoaded event handler:

Initiate image converter for the document loaded in the PDF Viewer.

**C#:**

```csharp
      private void PdfViewer_DocumentLoaded(object sender, EventArgs e)
      {
          // Dispose existing image converter
          imageConverter?.Dispose();

          // Check if the PDF source is a stream
          if (PdfViewer.DocumentSource is Stream documentStream)
          {
              // Reset and copy the stream
              documentStream.Position = 0;
              Stream stream = new MemoryStream();
              documentStream.CopyTo(stream);
              stream.Position = 0;

              // Initialize the image converter using the copied stream to extract the image from the PDF pages later.
              imageConverter = new PdfToImageConverter(stream);
          }
      }
```

#### b. AnnotationAdded event handler:

Get the bounds of the rectangle drawn to crop and show the specific portion in the callout layout.

**C#:**

```csharp
      private void PdfViewer_AnnotationAdded(object sender, Syncfusion.Maui.PdfViewer.AnnotationEventArgs e)
      {
          // Retrieve the annotation object that was just added to the PDF.
          Annotation? annotation = e.Annotation;

          // Check if the annotation is not null and is specifically a square annotation.
          if (annotation != null && annotation is SquareAnnotation squareAnnotation)
          {
              // Calculate the appropriate zoom scale based on the size of the selected rectangle.
              float scaleFactor = GetScaleFactor(squareAnnotation.Bounds.Width, squareAnnotation.Bounds.Height);

              // Use the image converter to extract the selected area from the PDF page as an image.
              Stream? imageStream = imageConverter?.Convert(annotation.PageNumber - 1,
                  squareAnnotation.Bounds, scaleFactor);

              // Display the callout popup overlay.
              CalloutPopup.IsVisible = true;

              // Set the extracted image as the source for the callout image view.
              CalloutImage.Source = ImageSource.FromStream(() => imageStream);

              // Remove the annotation from the PDF viewer to keep the interface clean.
              PdfViewer.RemoveAnnotation(annotation);
          }
      }
```

### 6. Wire the DocumentLoaded and AnnoatationAdded event handler in PdfViewer control.

Wire the event handler for the PdfViewer control. 

**XAML:**

``` xaml
      <syncfusion:SfPdfViewer 
      x:Name="PdfViewer"
      AnnotationAdded="PdfViewer_AnnotationAdded"
      DocumentLoaded="PdfViewer_DocumentLoaded">
      </syncfusion:SfPdfViewer>
```

### 7. Create Toggled event handler.

In the `Toggled` event handler, the callout layout is enabled and disabled.

**C#:**

```csharp
      private async void CalloutMode_Toggled(object sender, ToggledEventArgs e)
      {
          if (e.Value == true)
          {
              // Enable rectangle drawing mode.
              PdfViewer.AnnotationMode = AnnotationMode.Square;
          }
          else
          {
              // Disable the rectangle drawing mode.
              PdfViewer.AnnotationMode = AnnotationMode.None;
              // Close callout popup.
              CalloutImage.Source = null;
              CalloutPopup.IsVisible = false;
          }
      }
```

### 8. Wire the Toggled event handler with the switch.

**XAML:**

```xaml
      <Switch IsToggled="False" Toggled="CalloutMode_Toggled"/>
```

### 9. Create Tapped event handler.

In this Tapped event handler, the callout layout is closed when we tap on the outside the image of the specific content in the pdf rendered.

**C#:**

```csharp
      private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
      {
          // Close callout popup.
          CalloutImage.Source = null;
          CalloutPopup.IsVisible = false;
      }
```

### 10. Add the Tapped event handler to the callout popup.

While tapping on the callout popup outside the image of the specific content rendered, the callout popup is closed.

**XAML:**

```xaml
      <Grid BackgroundColor="#33000000" x:Name="CalloutPopup" IsVisible="False">
          <Grid.GestureRecognizers>
              <TapGestureRecognizer Tapped="TapGestureRecognizer_Tapped"
                   NumberOfTapsRequired="1" />
          </Grid.GestureRecognizers>
          <Image 
              x:Name="CalloutImage"
              VerticalOptions="Center" HorizontalOptions="Center" 
              Margin="{OnPlatform 40, iOS=20, Android=20}">
              <Image.GestureRecognizers>
                  <TapGestureRecognizer NumberOfTapsRequired="1" />
              </Image.GestureRecognizers>
          </Image>
      </Grid>
```

### Conclusion

We hope you enjoyed learning how to Zoom and view the desired area in a PDF in a callout in .NET MAUI.

Refer to our [.NET MAUI PDF Viewer's feature tour](https://www.syncfusion.com/maui-controls/maui-pdf-viewer) page to learn about its other groundbreaking feature representations. You can also explore our [.NET MAUI PDF Viewer Documentation](https://help.syncfusion.com/maui/pdf-viewer/getting-started) to understand how to present and manipulate data.

For current customers, check out our .NET MAUI components on the [License and Downloads](https://www.syncfusion.com/sales/teamlicense) page. If you are new to Syncfusion, try our 30-day [free trial](https://www.syncfusion.com/downloads/maui) to explore our .NET MAUI PDF Viewer and other .NET MAUI components.

Please let us know in the following comments if you have any queries or require clarifications. You can also contact us through our [support forums](https://www.syncfusion.com/downloads/maui), [support ticket](https://support.syncfusion.com/create) or [feedback portal](https://www.syncfusion.com/feedback/maui). We are always happy to assist you!

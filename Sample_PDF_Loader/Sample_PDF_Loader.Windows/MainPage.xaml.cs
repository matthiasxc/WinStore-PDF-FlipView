using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Sample_PDF_Loader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<string> PdfImages = new ObservableCollection<string>();
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            PdfImages = new ObservableCollection<string>();
            this.pdfViewer.ItemsSource = PdfImages;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime startTime = DateTime.Now;

            if (PdfImages == null)
                PdfImages = new ObservableCollection<string>();
            PdfImages.Clear();

            string pdfFileName = "6112941";
            Uri fileTarget = new Uri(@"ms-appx:///Data/" + pdfFileName + ".pdf");
            var file = await Package.Current.InstalledLocation.GetFileAsync(@"Data\" + pdfFileName + ".pdf");
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(fileTarget);

            /// <summary> 
            /// This renders PDF page with render options  
            /// Rendering a pdf page requires following 3 steps 
            ///     1. PdfDocument.LoadFromFileAsync(pdfFile) which returns pdfDocument 
            ///     2. pdfDocument.GetPage(pageIndex)  
            ///     3. pdfPage.RenderToStreamAsync(stream) or pdfPage.RenderToStreamAsync(stream,pdfPageRenderOptions) 
            /// </summary> 

            var pdfFile = await PdfDocument.LoadFromFileAsync(file);

            if (pdfFile == null)
                return;

            for (uint i = 0; i < pdfFile.PageCount; i++)
            {

                StorageFolder tempFolder = ApplicationData.Current.LocalFolder;
                StorageFile jpgFile = await tempFolder.CreateFileAsync(pdfFile + "-Page-" + i.ToString() + ".png", CreationCollisionOption.ReplaceExisting);

                var pdfPage = pdfFile.GetPage(i);

                if (jpgFile != null && pdfPage != null)
                {
                    IRandomAccessStream randomStream = await jpgFile.OpenAsync(FileAccessMode.ReadWrite);

                    //PdfPageRenderOptions pdfPageRenderOptions = new PdfPageRenderOptions();
                    await pdfPage.RenderToStreamAsync(randomStream);
                    await randomStream.FlushAsync();
                    randomStream.Dispose();
                    pdfPage.Dispose();
                }

                PdfImages.Add(jpgFile.Path);
            }

            TimeSpan processTime = DateTime.Now - startTime;
            Debug.WriteLine(processTime.TotalMilliseconds + " ms to process PDF");
        }

        public async Task DisplayImageFileAsync(StorageFile file)
        {
            // Display the image in the UI. 
            BitmapImage src = new BitmapImage();
            src.SetSource(await file.OpenAsync(FileAccessMode.Read));
            //Image1.Source = src;
        }
        private async void Stuff()
        {

            // Use this if the file is preinstalled with your app 
            //  - Load file into your app
            //  - set file to 
            //      - Build Action -> Content
            //      - Copy To Output Directory -> Copy if newer
            //var file = await Package.Current.InstalledLocation.GetFileAsync(@"Data\MyPdfFile.pdf");

            // Use this if the file is saved to the application storage 
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync("MyPdfFile.pdf");

            // Load the file as a pdf file
            var pdfFile = await PdfDocument.LoadFromFileAsync(file);
            if (pdfFile == null)
                return;

            StorageFolder tempFolder = ApplicationData.Current.LocalFolder;
            StorageFile pdfImageFile = await tempFolder.CreateFileAsync("MyPdfFile-Page1.png", CreationCollisionOption.ReplaceExisting);

            var pdfPage = pdfFile.GetPage(0);
            if(pdfImageFile != null && pdfPage != null)
            {
                var pdfStream = await pdfImageFile.OpenAsync(FileAccessMode.ReadWrite);
                await pdfPage.RenderToStreamAsync(pdfStream);
                await pdfStream.FlushAsync();
                pdfStream.Dispose();
                pdfPage.Dispose();
            }

            // If you're using binding, you can just use a string to the image path
            string pathForImage = pdfImageFile.Path;

            // or you can set the image source of a XAML Image in code
            BitmapImage src = new BitmapImage();
            src.SetSource(await file.OpenAsync(FileAccessMode.Read));
            MyImageElement.Source = src;
                        
        }
    }

}

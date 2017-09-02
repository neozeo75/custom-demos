using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System.Display;

namespace HomeAutomationDevice
{
    public class CameraHelper
    {
        public CameraHelper(CaptureElement captureElement)
        {
            this.captureElement = captureElement;
        }
 
        private static async Task<DeviceInformation> GetCameraID(Windows.Devices.Enumeration.Panel desired)
        {
            DeviceInformation deviceID = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture))
                .FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desired);

            if (deviceID != null) return deviceID;
            else throw new Exception(string.Format("Camera of type {0} doesn't exist.", desired));
        }

        public async Task<string> TakePhotoAsync()
        {
            string filePath = "";

            try
            {
                var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                var guid = Guid.NewGuid().ToString();
                var captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
                var file = await captureFolder.CreateFileAsync($"img-{guid}.jpg", CreationCollisionOption.GenerateUniqueName);
                var stream = new InMemoryRandomAccessStream();
                await _mediaCapture.CapturePhotoToStreamAsync(properties, stream);
                await ReencodeAndSavePhotoAsync(stream, file, PhotoOrientation.Normal);
                filePath = file.Path;
              }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when taking a photo: " + ex.ToString());
            }
            return filePath;
        }

        private ImageEncodingProperties properties;
        public async void InitializeCamera()
        {
            if (isCameraInitialized) return;
            request = new DisplayRequest();
            try
            {
                //var cameraID = await GetCameraID(Windows.Devices.Enumeration.Panel.Unknown);
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();

             //   await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
               // {

                 //   StreamingCaptureMode = StreamingCaptureMode.Video,
                 //   PhotoCaptureSource = PhotoCaptureSource.Photo,
                  //  AudioDeviceId = string.Empty,
                  //  VideoDeviceId = cameraID.Id
               // });
                // mediaCapture.VideoDeviceController.PrimaryUse = primaryUse;


                properties = ImageEncodingProperties.CreateJpeg();
                properties.Height = 450;
                properties.Width = 800;

                captureElement.Source = _mediaCapture;
                request.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                isCameraInitialized = true;

                if (isCameraPreviewing) return;
                await _mediaCapture.StartPreviewAsync();
                isCameraPreviewing = true;
            }
            catch (Exception ex)
            {
                isCameraInitialized = false;
                Debug.WriteLine("MediaCapture initialization failed. {0}", ex.Message);
            }
        }

        public async Task StartCameraPreviewAsync()
        {
            if (isCameraInitialized)
            {
                try
                {
                    await _mediaCapture.StartPreviewAsync();
                    isCameraPreviewing = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MediaCapture initialization failed. {0}", ex.Message);
                }
            }
            else
            {
                try
                {
                    InitializeCamera();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MediaCapture initialization failed. {0}", ex.Message);
                }
            }
        }

        public async Task StopCameraPreviewAsync()
        {
            try
            {
                await _mediaCapture.StopPreviewAsync();
                isCameraPreviewing = false;
            }
            catch
            {
                Debug.WriteLine("The app was denied access to the camera");
            }

        }
        private async Task CleanupCameraAsync()
        {
            Debug.WriteLine("CleanupCameraAsync");
            if (isCameraInitialized)
            {
                if (isCameraPreviewing)
                {
                    await StopCameraPreviewAsync();
                }
                isCameraInitialized = false;
            }

            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        private static async Task ReencodeAndSavePhotoAsync(IRandomAccessStream stream, StorageFile file, PhotoOrientation photoOrientation)
        {
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);
                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);
                    var properties = new BitmapPropertySet {
                        {
                            "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16)
                        }
                    };
                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }
            }
        }

        private MediaCapture _mediaCapture;
        private bool isCameraPreviewing;
        private bool isCameraInitialized;
        private DisplayRequest request;
        private CaptureElement captureElement { get; set; }
    }
}

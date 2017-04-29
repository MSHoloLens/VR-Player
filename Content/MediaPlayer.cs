using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VR_Player.Content
{
    class MediaPlayer
    {
        private MediaEngineEx mediaEngine;
        private bool ready = false;
        private Texture2D texture;
        private Surface dxgiSurface;
        public ShaderResourceView textureView;

        private SharpDX.Direct3D11.Device3 d3dDevice;

        //public static String FileName = "C:\\Data\\Users\\DefaultAccount\\Pictures\\Camera Roll\\publichd.best.of.3D.05.1080p_201731221849.mp4";
        public static String FileName = Windows.ApplicationModel.Package.Current.InstalledLocation.Path
                                        + "\\Assets\\publichd.best.of.3D.05.1080p_201731221849.mp4";

        public MediaPlayer(SharpDX.Direct3D11.Device3 device)
        {
            d3dDevice = device;
            InitMediaEngine();
        }

        public async void InitMediaEngine()
        {
            MediaManager.Startup();

            DeviceMultithread mt = d3dDevice.QueryInterface<DeviceMultithread>();
            mt.SetMultithreadProtected(true);

            DXGIDeviceManager deviceManager = new DXGIDeviceManager();
            deviceManager.ResetDevice(d3dDevice);

            MediaEngineAttributes attr = new MediaEngineAttributes();
            attr.VideoOutputFormat = (int)SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            attr.DxgiManager = deviceManager;

            MediaEngineClassFactory mediaFactory = new MediaEngineClassFactory();

             var mEngine = new MediaEngine(
                mediaFactory,
                attr,
                MediaEngineCreateFlags.None,
                mediaEngine_PlaybackEvent);

            

            this.mediaEngine = mEngine.QueryInterface<MediaEngineEx>();
            Windows.Storage.StorageFile sampleFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(FileName);
            var stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            ByteStream byteStream = new ByteStream(stream);
            this.mediaEngine.SetSourceFromByteStream(byteStream, FileName);
        }

        public void mediaEngine_PlaybackEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            Debug.WriteLine("mediaEvent :" + mediaEvent);
            if (mediaEvent == MediaEngineEvent.CanPlay)
            {

                if (mediaEngine.Error != null)
                {
                    Debug.WriteLine("mediaEngine Error :" + mediaEngine.Error);
                    return;
                }

                if (this.mediaEngine.HasVideo())
                {
                    int width = 0;
                    int height = 0;
                    this.mediaEngine.GetNativeVideoSize(out width, out height);

                    texture = new SharpDX.Direct3D11.Texture2D(
                            d3dDevice,
                            new SharpDX.Direct3D11.Texture2DDescription()
                            {
                                ArraySize = 1,
                                Width = width,
                                Height = height,
                                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                                Format = Format.B8G8R8A8_UNorm,
                                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                                BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource,
                                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                                SampleDescription = new SampleDescription(1, 0),
                                MipLevels = 1,
                            });

                    this.dxgiSurface = this.texture.QueryInterface<SharpDX.DXGI.Surface>();

                    this.textureView = new ShaderResourceView(d3dDevice, texture);

                    ready = true;

                    this.mediaEngine.Play();
                }

            }
            else if (mediaEvent == MediaEngineEvent.Error)
            {
                Debug.WriteLine("mediaEngine Error :" + mediaEngine.Error.GetErrorCode());
            }
        }

        public void TransferVideoFrame()
        {
            if (!ready)
            {
                Debug.WriteLine("ready is false");
                return;
            }


            if (this.dxgiSurface == null)
            {
                Debug.WriteLine("dxgiSurface is null");
                return;
            }


            long ts;
            if (!mediaEngine.OnVideoStreamTick(out ts))
            {
                Debug.WriteLine("mediaEngine.OnVideoStreamTick(out ts) is false");
                return;
            }


            if (ts < 0)
                return;

            this.mediaEngine.TransferVideoFrame(
                this.dxgiSurface,
                null,
                new SharpDX.Mathematics.Interop.RawRectangle(0, 0, this.texture.Description.Width, this.texture.Description.Height),
                null
                );
        }


    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace MU.Global.Tools
{
    public static class SoftwareBitmapHelper
    {
        public static async Task<Stream> GetBitmapStream(SoftwareBitmap softwareBitmap)
        {
            if (softwareBitmap != null)
            {

                var bitmapImageInMemoryRandomAccessStream = new InMemoryRandomAccessStream();
                var bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, bitmapImageInMemoryRandomAccessStream);
                bitmapEncoder.SetSoftwareBitmap(softwareBitmap);
                await bitmapEncoder.FlushAsync();

                return bitmapImageInMemoryRandomAccessStream.AsStream();
            }
            else
            {
                return null;
            }
        }
    }
}

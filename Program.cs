using System;
using System.IO;
using ImageMagick;

namespace PhotoResizer4K
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: dotnet run <input_folder> <output_folder>");
                Console.WriteLine("Example: dotnet run ~/Photos/Input ~/Photos/Output");
                return;
            }

            string inputFolder = ExpandPath(args[0]);
            string outputFolder = ExpandPath(args[1]);

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine($"❌ Source folder does not exist: {inputFolder}");
                return;
            }

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            var files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly);
            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".heic", ".heif", ".bmp", ".tiff", ".cr2", ".nef", ".arw" };

            int processed = 0;
            int errors = 0;

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (Array.IndexOf(supportedExtensions, ext) >= 0)
                {
                    try
                    {
                        ProcessImage(file, outputFolder);
                        Console.WriteLine($"✅ Processed: {Path.GetFileName(file)}");
                        processed++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error on {Path.GetFileName(file)}: {ex.Message}");
                        errors++;
                    }
                }
            }

            Console.WriteLine($"\n🎉 Processing complete! {processed} files processed, {errors} errors");
        }

        static string ExpandPath(string path)
        {
            if (path.StartsWith("~/"))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                                   path.Substring(2));
            }
            return Path.GetFullPath(path);
        }

        static void ProcessImage(string inputPath, string outputFolder)
        {
            const int targetWidth = 3840;
            const int targetHeight = 2160;
            const double targetRatio = 16.0 / 9.0;

            using (var image = new MagickImage(inputPath))
            {
                // Apply EXIF orientation
                image.AutoOrient();

                double currentRatio = (double)image.Width / image.Height;

                uint cropWidth, cropHeight;
                int cropX, cropY;

                if (currentRatio > targetRatio)
                {
                    // Image wider than 16:9 - horizontal crop
                    cropHeight = (uint)image.Height;
                    cropWidth = (uint)(cropHeight * targetRatio);
                    cropY = 0;
                    cropX = FindBestCropPositionHorizontal(image, cropWidth);
                }
                else
                {
                    // Image taller than 16:9 - vertical crop
                    cropWidth = (uint)image.Width;
                    cropHeight = (uint)(cropWidth / targetRatio);
                    cropX = 0;
                    cropY = FindBestCropPositionVertical(image, cropHeight);
                }

                // Apply crop
                var cropGeometry = new MagickGeometry(cropX, cropY, cropWidth, cropHeight);
                image.Crop(cropGeometry);
                image.ResetPage();

                // Resize to 4K with high quality filter
                image.FilterType = FilterType.Lanczos;
                image.Resize(targetWidth, targetHeight);

                // Improve sharpness
                image.UnsharpMask(1.5, 0.5);

                // Optimal JPEG configuration
                image.Quality = 92;
                image.Format = MagickFormat.Jpeg;
                image.Settings.Interlace = Interlace.Plane; // Progressive JPEG

                // Preserve important EXIF metadata
                if (image.GetExifProfile() != null)
                {
                    // Keep metadata but remove thumbnails
                    image.RemoveProfile("icc");
                }

                // Save
                string outputFileName = Path.GetFileNameWithoutExtension(inputPath) + "_4K.jpg";
                string outputPath = Path.Combine(outputFolder, outputFileName);
                image.Write(outputPath);
            }
        }

        static int FindBestCropPositionHorizontal(MagickImage image, uint cropWidth)
        {
            using (var grayscale = (MagickImage)image.Clone())
            {
                grayscale.Grayscale();
                grayscale.Enhance();

                int maxX = (int)(image.Width - cropWidth);
                double bestScore = 0;
                int bestPosition = maxX / 2;

                int step = Math.Max(1, maxX / 20);

                for (int x = 0; x <= maxX; x += step)
                {
                    double score = CalculateRegionInterest(grayscale, x, 0, cropWidth, image.Height);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPosition = x;
                    }
                }

                return bestPosition;
            }
        }

        static int FindBestCropPositionVertical(MagickImage image, uint cropHeight)
        {
            int maxY = (int)(image.Height - cropHeight);
            return Math.Min(maxY / 4, maxY);
        }

        static double CalculateRegionInterest(MagickImage image, int x, int y, uint width, uint height)
        {
            using (var region = (MagickImage)image.Clone())
            {
                region.Crop(new MagickGeometry(x, y, width, height));
                var stats = region.Statistics();
                var channelStats = stats.GetChannel(PixelChannel.Red);
                return channelStats?.StandardDeviation ?? 0;
            }
        }
    }
}
using CommandLine;
using System;
using System.IO;
using System.Threading.Tasks;
using VideoLibrary;
using Xabe.FFmpeg;

namespace YTD
{
    public static class Program
    {
        private const string TempDirectory = "Temp";

        public static void Main(string[] args) => Parser.Default
            .ParseArguments<CLIOptions>(args)
            .WithParsed(OnParsed);

        private static void OnParsed(CLIOptions options)
        {
            Console.WriteLine("Getting video");
            YouTube youtube = YouTube.Default;
            YouTubeVideo video = youtube.GetVideo(options.Source);
            Console.WriteLine("Video downloaded");
            Console.WriteLine();

            string outdir = options.OutDir;
            string outputFile = options.Output ?? Path.ChangeExtension($"{video.FullName}\"", "mp3");
            string output = Path.Combine(options.OutDir, outputFile);

            if (File.Exists(output))
            {
                Console.WriteLine($"File '{output}' already exists");
                if (options.OverrideExisting)
                {
                    Console.WriteLine("Deleting previous version to override");
                    File.Delete(output);
                }
                else
                {
                    Console.WriteLine("Use '--override' option if you want to override files");
                    return;
                }
            }

            

            if (!Directory.Exists(TempDirectory))
            {
                Console.WriteLine("Creating Temp directory");
                Directory.CreateDirectory(TempDirectory);
            }

            string tempFileName = Path.Combine(TempDirectory, video.FullName);

            if (File.Exists(tempFileName))
                File.Delete(tempFileName);

            Console.WriteLine("Creating temp file");
            using (Stream vs = video.Stream())
            using (FileStream fs = File.Create(tempFileName))
            {
                vs.CopyTo(fs);
            }
            Console.WriteLine("Temp file created");
            Console.WriteLine();

            Console.WriteLine("Setting up FFMPEG");
            FFmpeg.SetExecutablesPath("FFMPEG");
            Console.WriteLine("FFMPEG set up");
            Console.WriteLine();

            Console.WriteLine("Converting clip");

            Console.WriteLine("Obtaining Mediainfo");
            Task<IMediaInfo> miTask = FFmpeg.GetMediaInfo(tempFileName);
            miTask.Wait();

            IMediaInfo mediaInfo = miTask.Result;

            Console.WriteLine("Mediainfo obtained");

            string tempOutput = Path.Combine(outdir, $"Temp_{Guid.NewGuid()}.mp3");
            Console.WriteLine($"Using temp file: {tempOutput}");
            Console.WriteLine();
            Task<IConversionResult> conversionTask = FFmpeg.Conversions.New()
                .AddStream(mediaInfo.AudioStreams)
                .SetOutput(tempOutput)
                .Start();
            conversionTask.Wait();

            File.Move(tempOutput, output);

            Console.WriteLine("Clip converted");
            Console.WriteLine(output);
            Console.WriteLine();

            Console.WriteLine("Deleting temp file");
            File.Delete(tempFileName);
            Console.WriteLine("Temp file deleted");
        }
    }
}

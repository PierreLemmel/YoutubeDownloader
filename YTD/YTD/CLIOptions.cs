using CommandLine;

namespace YTD
{
    public class CLIOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source video from youtube")]
        public string Source { get; set; } = "";

        [Option('o', "output", Required = false, HelpText = "Output filename")]
        public string? Output { get; set; }

        [Option("outdir", Required = false, Default = "Result", HelpText = "Output directory")]
        public string OutDir { get; set; } = "Result";

        [Option("override", Required = false, Default = false, HelpText = "Override existing files")]
        public bool OverrideExisting { get; set; }
    }
}
using CommandLine;
using System;
using System.IO;

public class Options
{
    [Option('u', "url", Required = true, HelpText = "URL for yaml file")]
    public Uri Url { get; set; }

    [Option('d', "db", Required = false, HelpText = "DB filepath")]
    public FileInfo DbPath { get; set; }

    // Omitting long name, defaults to name of property, ie "--verbose"
    [Option('m', "in-memory", Default = false, HelpText = "Use in-memory db")]
    public bool InMemory { get; set; }
}

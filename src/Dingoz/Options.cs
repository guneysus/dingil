using CommandLine;
using System;
using System.IO;

public class Options
{
    [Option('u', "url", Required = true, HelpText = "URL for yaml file")]
    public Uri Url { get; set; }

    [Option('d', "db", Required = true, HelpText = "DB filepath")]
    public FileInfo DbPath { get; set; }
}

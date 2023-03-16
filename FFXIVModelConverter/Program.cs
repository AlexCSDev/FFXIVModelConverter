using System.Globalization;
using CommandLine;
using FFXIVModelConverter.Enums;
using FFXIVModelConverter.Models;
using Microsoft.Extensions.Configuration;
using NLog;
using SharpDX.Direct2D1;
using xivModdingFramework.Cache;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Models.Helpers;
using LogLevel = FFXIVModelConverter.Enums.LogLevel;

namespace FFXIVModelConverter
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            var ci = new CultureInfo("en-US")
            {
                NumberFormat = { NumberDecimalSeparator = "." }
            };

            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;

            LoggingManager.ConfigureLogging(LogLevel.Trace, true);

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("settings.json", false, false)
                .Build();

            XivCache.SetGameInfo(new DirectoryInfo(config["GameDataDirectory"]), XivLanguages.GetXivLanguage(config["GameLanguage"]));

            object commandLineOptions = null;
            var result = Parser.Default.ParseArguments<Fbx2MdlOptions, Mdl2FbxOptions>(args)
                .WithParsed(options => commandLineOptions = options)
                .WithNotParsed(errors => { });

            switch (commandLineOptions)
            {
                case Fbx2MdlOptions importOptions:
                    await RunImportAsync(importOptions);
                    break;
                case Mdl2FbxOptions exportOptions:
                    await RunExportAsync(exportOptions);
                    break;
                default:
                    return;
            }
        }

        private static async Task RunExportAsync(Mdl2FbxOptions exportOptions)
        {
            Logger.Warn("Exporting is a test feature, it is recommended to export models using textools");
            ModelExporter exporter = new ModelExporter();
            await exporter.ExportModelToFile(
                exportOptions.InputPath,
                exportOptions.InGamePath,
                exportOptions.OutputPath,
                1, false);
        }

        private static async Task RunImportAsync(Fbx2MdlOptions importOptions)
        {
            ModelConverterMdl mdl = new ModelConverterMdl(XivCache.GameInfo.GameDirectory);
            await mdl.ConvertToMDL(
                importOptions.InputPath,
                importOptions.OutputPath,
                importOptions.InGamePath,
                importOptions.BaseModelPath,
                new ModelModifierOptions()
                {
                    AutoScale = importOptions.AutoScale,
                    ClearUV2 = importOptions.ClearUV2,
                    ClearVColor = importOptions.ClearVColor,
                    ForceUVQuadrant = importOptions.ForceUVQuadrant,
                    CloneUV2 = importOptions.CloneUV2,
                    ClearVAlpha = importOptions.ClearVAlpha,
                    UseOriginalShapeData = importOptions.UseOriginalShapeData,
                    CopyAttributes = importOptions.CopyAttributes,
                    CopyMaterials = importOptions.CopyMaterials,
                    SourceRace = importOptions.SourceRace
                }, LoggingFunction
            );
        }

        private static void LoggingFunction(bool arg1, string arg2)
        {
            Logger.Info(arg2);
        }
    }
}
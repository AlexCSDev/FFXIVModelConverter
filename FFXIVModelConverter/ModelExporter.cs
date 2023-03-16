using System.Diagnostics;
using System.Text.RegularExpressions;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.Helpers;
using Index = xivModdingFramework.SqPack.FileTypes.Index;

namespace FFXIVModelConverter
{
    internal class ModelExporter
    {
        private static ModelLoader _modelLoader = new ModelLoader();
        /// <summary>
        /// Retreieves the available list of file extensions the framework has exporters available for.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableExporters()
        {
            var cwd = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            cwd = cwd.Replace("\\", "/");
            string importerPath = cwd + "/converters/";
            var ret = new List<string>();
            ret.Add("obj"); // OBJ handler is internal.
            ret.Add("db");  // Raw already-parsed DB files are fine.

            var directories = Directory.GetDirectories(importerPath);
            foreach (var d in directories)
            {
                var suffix = (d.Replace(importerPath, "")).ToLower();
                if (ret.IndexOf(suffix) < 0)
                {
                    ret.Add(suffix);
                }
            }
            return ret;
        }

        /// <summary>
        /// Converts and exports MDL file, passing it to the appropriate exporter as necessary
        /// to match the target file extention.
        /// </summary>
        /// <param name="mdlPath"></param>
        /// <param name="outputFilePath"></param>
        /// <returns></returns>
        public async Task ExportModelToFile(string path, string inGamePath, string outputFilePath, int mtrlVariant = 1, bool includeTextures = true, Action<bool, string> loggingFunction = null)
        {
            // Importers and exporters currently use the same criteria.
            // Any available exporter is assumed to be able to import and vice versa.
            // This may change at a later date.
            var exporters = GetAvailableExporters();
            var fileFormat = Path.GetExtension(outputFilePath).Substring(1);
            fileFormat = fileFormat.ToLower();
            if (!exporters.Contains(fileFormat))
            {
                throw new NotSupportedException(fileFormat.ToUpper() + " File type not supported.");
            }

            var dir = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            var model = await _modelLoader.LoadModel(path, inGamePath, loggingFunction);
            await ExportModel(model, outputFilePath, mtrlVariant, includeTextures);
        }

        /// <summary>
        /// Exports a TTModel file to the given output path.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="outputFilePath"></param>
        /// <returns></returns>
        private async Task ExportModel(TTModel model, string outputFilePath, int mtrlVariant = 1, bool includeTextures = true)
        {
            var exporters = GetAvailableExporters();
            var fileFormat = Path.GetExtension(outputFilePath).Substring(1);
            fileFormat = fileFormat.ToLower();
            if (!exporters.Contains(fileFormat))
            {
                throw new NotSupportedException(fileFormat.ToUpper() + " File type not supported.");
            }

            var dir = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            // Remove the existing file if it exists, so that the user doesn't get confused thinking an old file is the new one.
            File.Delete(outputFilePath);

            outputFilePath = outputFilePath.Replace("/", "\\");

            // OBJ is a bit of a special, speedy case.  The format both has no textures, and no weights,
            // So we don't need to do any heavy lifting for that stuff.
            if (fileFormat == "obj")
            {
                //TODO: obj support
                /*var obj = new Obj(_gameDirectory);
                obj.ExportObj(model, outputFilePath);*/
                return;
            }

            if (!model.IsInternal)
            {
                // This isn't *really* true, but there's no case where we are re-exporting TTModel objects
                // right now without them at least having an internal XIV path associated, so I don't see a need to fuss over this,
                // since it would be complicated.
                throw new NotSupportedException("Cannot export non-internal model - Skel data unidentifiable.");
            }

            // The export process could really be sped up by forking threads to do
            // both the bone and material exports at the same time.

            // Pop the textures out so the exporters can reference them.
            if (includeTextures)
            {
                // Fix up our skin references in the model before exporting, to ensure
                // we supply the right material names to the exporters down-chain.
                if (model.IsInternal)
                {
                    ModelModifiers.FixUpSkinReferences(model, model.Source, null);
                }
                //TODO: texture support
                //await ExportMaterialsForModel(model, outputFilePath, _gameDirectory, mtrlVariant);
            }



            // Save the DB file.

            var cwd = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var converterFolder = cwd + "\\converters\\" + fileFormat;
            Directory.CreateDirectory(converterFolder);
            var dbPath = converterFolder + "\\input.db";
            model.SaveToFile(dbPath, outputFilePath);


            if (fileFormat == "db")
            {
                // Just want the intermediate file? Just see if we need to move it.
                if (!Path.Equals(outputFilePath, dbPath))
                {
                    File.Delete(outputFilePath);
                    File.Move(dbPath, outputFilePath);
                }
            }
            else
            {
                // We actually have an external importer to use.

                // We don't really care that much about showing the user a log
                // during exports, so we can just do this the simple way.

                var outputFile = converterFolder + "\\result." + fileFormat;

                // Get rid of any existing intermediate output file, in case it causes problems for any converters.
                File.Delete(outputFile);

                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = converterFolder + "\\converter.exe",
                        Arguments = "\"" + dbPath + "\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        WorkingDirectory = "" + converterFolder + "",
                        CreateNoWindow = true
                    }
                };

                proc.Start();
                proc.WaitForExit();
                var code = proc.ExitCode;

                if (code != 0)
                {
                    throw new Exception("Exporter threw error code: " + proc.ExitCode);
                }

                // Just move the result file if we need to.
                if (!Path.Equals(outputFilePath, outputFile))
                {
                    File.Delete(outputFilePath);
                    File.Move(outputFile, outputFilePath);
                }
            }
        }

        /// <summary>
        /// Retrieves and exports the materials for the current model, to be used alongside ExportModel
        /// </summary>
      /*  public static async Task ExportMaterialsForModel(TTModel model, string outputFilePath, DirectoryInfo gameDirectory, int mtrlVariant = 1, XivRace targetRace = XivRace.All_Races)
        {
            var modelName = Path.GetFileNameWithoutExtension(model.Source);
            var directory = Path.GetDirectoryName(outputFilePath);

            // Language doesn't actually matter here.
            var _mtrl = new Mtrl(XivCache.GameInfo.GameDirectory);
            var _tex = new Tex(gameDirectory);
            var _index = new Index(gameDirectory);
            var materialIdx = 0;


            foreach (var materialName in model.Materials)
            {
                try
                {
                    var mdlPath = model.Source;

                    // Set source race to match so that it doesn't get replaced
                    if (targetRace != XivRace.All_Races)
                    {
                        var bodyRegex = new Regex("(b[0-9]{4})");
                        var faceRegex = new Regex("(f[0-9]{4})");
                        var tailRegex = new Regex("(t[0-9]{4})");

                        if (bodyRegex.Match(materialName).Success)
                        {
                            var currentRace = model.Source.Substring(model.Source.LastIndexOf('c') + 1, 4);
                            mdlPath = model.Source.Replace(currentRace, targetRace.GetRaceCode());
                        }

                        var faceMatch = faceRegex.Match(materialName);
                        if (faceMatch.Success)
                        {
                            var mdlFace = faceRegex.Match(model.Source).Value;

                            mdlPath = model.Source.Replace(mdlFace, faceMatch.Value);
                        }

                        var tailMatch = tailRegex.Match(materialName);
                        if (tailMatch.Success)
                        {
                            var mdlTail = tailRegex.Match(model.Source).Value;

                            mdlPath = model.Source.Replace(mdlTail, tailMatch.Value);
                        }
                    }

                    // This messy sequence is ultimately to get access to _modelMaps.GetModelMaps().
                    var mtrlPath = _mtrl.GetMtrlPath(mdlPath, materialName, mtrlVariant);
                    var mtrlOffset = await _index.GetDataOffset(mtrlPath);
                    var mtrl = await _mtrl.GetMtrlData(mtrlOffset, mtrlPath, 11);
                    var modelMaps = await ModelTexture.GetModelMaps(gameDirectory, mtrl);

                    // Outgoing file names.
                    var mtrl_prefix = directory + "\\" + Path.GetFileNameWithoutExtension(materialName.Substring(1)) + "_";
                    var mtrl_suffix = ".png";

                    if (modelMaps.Diffuse != null && modelMaps.Diffuse.Length > 0)
                    {
                        using (Image<Rgba32> img = Image.LoadPixelData<Rgba32>(modelMaps.Diffuse, modelMaps.Width, modelMaps.Height))
                        {
                            img.Save(mtrl_prefix + "d" + mtrl_suffix, new PngEncoder());
                        }
                    }

                    if (modelMaps.Normal != null && modelMaps.Diffuse.Length > 0)
                    {
                        using (Image<Rgba32> img = Image.LoadPixelData<Rgba32>(modelMaps.Normal, modelMaps.Width, modelMaps.Height))
                        {
                            img.Save(mtrl_prefix + "n" + mtrl_suffix, new PngEncoder());
                        }
                    }

                    if (modelMaps.Specular != null && modelMaps.Diffuse.Length > 0)
                    {
                        using (Image<Rgba32> img = Image.LoadPixelData<Rgba32>(modelMaps.Specular, modelMaps.Width, modelMaps.Height))
                        {
                            img.Save(mtrl_prefix + "s" + mtrl_suffix, new PngEncoder());
                        }
                    }

                    if (modelMaps.Alpha != null && modelMaps.Diffuse.Length > 0)
                    {
                        using (Image<Rgba32> img = Image.LoadPixelData<Rgba32>(modelMaps.Alpha, modelMaps.Width, modelMaps.Height))
                        {
                            img.Save(mtrl_prefix + "o" + mtrl_suffix, new PngEncoder());
                        }
                    }

                    if (modelMaps.Emissive != null && modelMaps.Diffuse.Length > 0)
                    {
                        using (Image<Rgba32> img = Image.LoadPixelData<Rgba32>(modelMaps.Emissive, modelMaps.Width, modelMaps.Height))
                        {
                            img.Save(mtrl_prefix + "e" + mtrl_suffix, new PngEncoder());
                        }
                    }

                }
                catch (Exception exc)
                {
                    // Failing to resolve a material is considered a non-critical error.
                    // Continue attempting to resolve the rest of the materials in the model.
                    //throw exc;
                }
                materialIdx++;
            }
        }*/
    }
}

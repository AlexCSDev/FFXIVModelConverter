using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace FFXIVModelConverter.Models
{
    [Verb("mdl2fbx", HelpText = "Export FFXIV model into FBX")]
    class Mdl2FbxOptions
    {
        [Option("input", Required = true, HelpText = "Path to the mdl file")]
        public string InputPath { get; set; }
        [Option("output", Required = true, HelpText = "Path to the output fbx file")]
        public string OutputPath { get; set; }
        [Option("game-path", Required = true, HelpText = "Path to the model inside of the game archives")]
        public string InGamePath { get; set; }
    }

    [Verb("fbx2mdl", HelpText = "Convert FBX into FFXIV model format")]
    class Fbx2MdlOptions
    {
        [Option("input", Required = true, HelpText = "Path to the fbx file")]
        public string InputPath { get; set; }
        [Option("base-model", Required = false, HelpText = "Path to the base model which is being used for modding. If not supplied the model currently active in the game (possibly modded) will be used.")]
        public string BaseModelPath { get; set; }
        [Option("output", Required = true, HelpText = "Path to the output mdl file")]
        public string OutputPath { get; set; }
        [Option("game-path", Required = true, HelpText = "Path to the model inside of the game archives")]
        public string InGamePath { get; set; }
        [Option("copy-attributes", Default = true, HelpText = "????")]
        public bool CopyAttributes { get; set; }
        [Option("copy-materials", Default = true, HelpText = "????")]
        public bool CopyMaterials { get; set; }
        [Option("use-original-shape-data", Default = false, HelpText = "Enable the model's original shape data (must be ON for Faces)")]
        public bool UseOriginalShapeData { get; set; }
        [Option("force-uv-quadrant", Default = false, HelpText = "Move all UV coordinates into the [1,-1] quadrant")]
        public bool ForceUVQuadrant { get; set; }
        [Option("clear-uv2", Default = false, HelpText = "Reset the second UV layer to [0,0]")]
        public bool ClearUV2 { get; set; }
        [Option("clone-uv2-to-uv1", Default = false, HelpText = "Copy UV1 into UV2. (Useful for Hair shader items)")]
        public bool CloneUV2 { get; set; }
        [Option("clear-vertex-color", Default = false, HelpText = "Reset the vertex color to [255,255,255]")]
        public bool ClearVColor { get; set; }
        [Option("clear-vertex-alpha", Default = false, HelpText = "Reset the vertex alpha to [255,255,255]")]
        public bool ClearVAlpha { get; set; }
        [Option("auto-scale", Default = true, HelpText = "Automatically attempt to fix errors caused by improper unit scalings")]
        public bool AutoScale { get; set; }
        [Option("override-incoming-race", Default = XivRace.All_Races, HelpText = $"Make converter convert your model from a different race to appropriate one for this item. Available options: Hyur_Midlander_Male, Hyur_Midlander_Male_NPC, Hyur_Midlander_Female, Hyur_Midlander_Female_NPC, Hyur_Highlander_Male, Hyur_Highlander_Male_NPC, Hyur_Highlander_Female, Hyur_Highlander_Female_NPC, Elezen_Male, Elezen_Male_NPC, Elezen_Female, Elezen_Female_NPC, Miqote_Male, Miqote_Male_NPC, Miqote_Female, Miqote_Female_NPC, Roegadyn_Male, Roegadyn_Male_NPC, Roegadyn_Female, Roegadyn_Female_NPC, Lalafell_Male, Lalafell_Male_NPC, Lalafell_Female, Lalafell_Female_NPC, AuRa_Male, AuRa_Male_NPC, AuRa_Female, AuRa_Female_NPC, Hrothgar_Male, Hrothgar_Male_NPC, Hrothgar_Female, Hrothgar_Female_NPC, Viera_Male, Viera_Male_NPC, Viera_Female, Viera_Female_NPC, NPC_Male, NPC_Female, All_Races, Monster,DemiHuman")]
        public XivRace SourceRace { get; set; }
    }
}

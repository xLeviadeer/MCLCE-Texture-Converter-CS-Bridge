using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MCLCE_Texture_Converter_CS_Bridge { 

    // ——༺ENUMS༻——

    /// <summary>
    /// behavior for when the texture builder encounters a sizing error when processing a texture
    /// </summary>
    public enum ErrorMode {

        /// <summary>
        /// replace the texture with a resized & cropped version
        /// </summary>
        Replace,

        /// <summary>
        /// place a NotFound texture in-place of the texture
        /// </summary>
        Error
    }

    /// <summary>
    /// specifier for what type of path has been supplied
    /// </summary>
    public enum InputPathType {

        /// <summary>
        /// source pack is a texture pack folder
        /// </summary>
        Folder,

        /// <summary>
        /// source pack is a texture pack zip file
        /// </summary>
        Zip,

        /// <summary>
        /// source pack is a texture pack mcpack file
        /// </summary>
        Mcpack
    }

    /// <summary>
    /// specifier for the game being converted from
    /// </summary>
    public enum InputGame {
        Java,
        Bedrock,

        /// <summary>
        /// use internal stored textures to generate default game textures
        /// </summary>
        /// <remarks>
        /// an `InputPath` should ¡not¡ be included | included as string.Empty
        /// a `InputPathType` should ¡not¡ be included | selection does not matter
        /// a `InputVersion` should ¡not¡ be included | selection does not matter
        /// </remarks>
        ConsoleDefault
    }

    /// <summary>
    /// the ⸉structure⸉ 🝗 game and file structure 𐑶 to convert to
    /// </summary>
    public enum OutputStructure {
        /// <summary>
        /// texture dump
        /// </summary>
        WiiuDump,

        /// <summary>
        /// sdcafiine
        /// </summary>
        WiiuPortPack,

        /// <summary>
        /// root file structure
        /// </summary>
        WiiuModpack,

        /// <summary>
        /// texture dump
        /// </summary>
        NSDump,

        /// <summary>
        /// texture dump
        /// </summary>
        XB360Dump,

        /// <summary>
        /// texture dump
        /// </summary>
        XBOneDump,

        /// <summary>
        /// texture dump
        /// </summary>
        PS3Dump,

        /// <summary>
        /// texture dump
        /// </summary>
        PSVDump,

        /// <summary>
        /// texture dump
        /// </summary>
        PS4Dump
    }

    /// <summary>
    /// behavior for what drive installation structure to output in
    /// </summary>
    /// <remarks>
    /// does nothing if the `OutputStructure` does not allow for drives
    /// </remarks>
    public enum OutputDrive { 
        /// <summary>
        /// system installation
        /// </summary>
        Sys,

        /// <summary>
        /// usb installation
        /// </summary>
        Usb
    }

    // ——༺ENTRY POINT༻——

    /// <summary>
    /// a data holder & process generator for execution of the TB
    /// </summary>
    public class EntryPoint {
        // ———ENUM MAPS———

        private static readonly Dictionary<ErrorMode, string> _MAP_ERROR_MODE = new Dictionary<ErrorMode, string> {
            [ErrorMode.Replace] = "replace",
            [ErrorMode.Error] = "error"
        };

        private static readonly Dictionary<InputPathType, string> _MAP_INPUT_PATH_TYPE = new Dictionary<InputPathType, string> {
            [InputPathType.Folder] = "folder",
            [InputPathType.Zip] = "zip",
            [InputPathType.Mcpack] = "mcpack"
        };

        private static readonly Dictionary<InputGame, string> _MAP_INPUT_GAME = new Dictionary<InputGame, string> {
            [InputGame.Java] = "java",
            [InputGame.Bedrock] = "bedrock",
            [InputGame.ConsoleDefault] = "wiiu"
        };

        private static readonly Dictionary<OutputStructure, string> _MAP_OUTPUT_STRUCTURE = new Dictionary<OutputStructure, string> {
            [OutputStructure.WiiuDump] = "wiiu dump",
            [OutputStructure.WiiuPortPack] = "wiiu port pack (root directory)",
            [OutputStructure.WiiuModpack] = "wiiu modpack (sdcafiine)",
            [OutputStructure.NSDump] = "nintendo switch dump",
            [OutputStructure.XB360Dump] = "xbox 360 dump",
            [OutputStructure.XBOneDump] = "xbox one dump",
            [OutputStructure.PS3Dump] = "PS 3 dump",
            [OutputStructure.PSVDump] = "PS vita dump",
            [OutputStructure.PS4Dump] = "PS 4 dump"
        };

        private static readonly Dictionary<OutputDrive, string> _MAP_OUTPUT_DRIVE = new Dictionary<OutputDrive, string> {
            [OutputDrive.Sys] = "system",
            [OutputDrive.Usb] = "usb"
        };

        // ———VARIABLES———

        /// <summary>
        /// the relative path to the ¡builder_cmd¡ directory in your project at execution time
        /// </summary>
        public string MainLoc { get; set; } = "builder_cmd";
        /// <summary>
        /// see <see cref="MCLCE_Texture_Converter_CS_Bridge.ErrorMode"/> for details
        /// </summary>
        public ErrorMode ErrorMode { get; set; } = ErrorMode.Replace;
        /// <summary>
        /// the expected general texture size during conversion 
        /// ╎ must be a multiple of two 
        /// </summary>
        /// <remarks>
        /// sizes x16, x32 and x64 have been tested ⧜ sizes >= x64 have not been tested
        /// ╎ is both the expected input texture pack size and definite output size ∋ `ErrorMode` determines what happens when textures are not sized as expected by the `ProcessingSize`
        /// </remarks>
        public int ProcessingSize { get; set; } = 16;
        
        /// <summary>
        /// texture pack input file-path (absolute)
        /// </summary>
        public string InputPath { get; set; }
        /// <summary>
        /// see <see cref="MCLCE_Texture_Converter_CS_Bridge.InputPathType"/> for details
        /// </summary>
        public InputPathType InputPathType { get; set; }
        /// <summary>
        /// see <see cref="MCLCE_Texture_Converter_CS_Bridge.InputGame"/> for details
        /// </summary>
        public InputGame InputGame { get; set; } 
        /// <summary>
        /// the expected version of the texture pack at `InputPath`
        /// </summary>
        public Version InputVersion { get; set; }

        /// <summary>
        /// texture pack output directory (absolute)
        /// </summary>
        public string OutputPath { get; set; }
        /// <summary>
        /// see <see cref="MCLCE_Texture_Converter_CS_Bridge.OutputStructure"/> for details
        /// </summary>
        public OutputStructure OutputStructure { get; set; }
        /// <summary>
        /// see <see cref="MCLCE_Texture_Converter_CS_Bridge.OutputDrive"/> for details
        /// </summary>
        public OutputDrive OutputDrive { get; set; } = OutputDrive.Sys;
        
        /// <summary>
        /// whether to use complex texture processing
        /// ⚠ setting to `true` for a size larger than x16 may cause significant processing delay
        /// </summary>
        public bool DoComplexProcessing { get; set; } = true;
        /// <summary>
        /// whether to use the Error texture when creating a texture which resulted in an exception rather than the program crashing
        /// </summary>
        public bool DoErrorTexture { get; set; } = false;
        /// <summary>
        /// whether the path is expected to be a folder of textures (true) or a folder containing subfolders of textures (false)
        /// </summary>
        public bool IsDirectPath { get; set; } = false;
        /// <summary>
        /// whether to force the program to use dump (output) mode regardless of `OutputStructure` selection
        /// </summary>
        public bool DoForceDumpMode { get; set; } = false;

        // ———METHODS———

        /// <summary>
        /// create a <see cref="Process"/> from the current <see cref="EntryPoint"/> settings
        /// ╎ does ¡not¡ start the process
        /// </summary>
        /// <returns>a non-started <see cref="Process"/> object</returns>
        public Process ToProcess() {
            var argumentsDict = new Dictionary<string, string> {
                ["--error-mode"] = EntryPoint._MAP_ERROR_MODE[this.ErrorMode],
                ["--processing-size"] = this.ProcessingSize.ToString(),
                ["--input-path"] = this.InputPath,
                ["--input-path-type"] = EntryPoint._MAP_INPUT_PATH_TYPE[this.InputPathType],
                ["--input-game"] = EntryPoint._MAP_INPUT_GAME[this.InputGame],
                ["--input-version"] = this.InputVersion.ToString(),
                ["--output-path"] = this.OutputPath,
                ["--output-structure"] = EntryPoint._MAP_OUTPUT_STRUCTURE[this.OutputStructure],
                ["--output-drive"] = EntryPoint._MAP_OUTPUT_DRIVE[this.OutputDrive],
                ["--use-simple-processing"] = (!this.DoComplexProcessing).ToString(),
                ["--use-error-texture"] = this.DoErrorTexture.ToString(),
                ["--is-direct-path"] = this.IsDirectPath.ToString(),
                ["--force-dump-mode"] = this.DoForceDumpMode.ToString()
            };
            var arguments = new StringBuilder("");
            foreach (var kvp in argumentsDict) {
                if (kvp.Value == "True") { arguments.Append(kvp.Key); }
                else if (kvp.Value == "False") { continue; }
                arguments.Append(kvp.Key);
                arguments.Append(" \"");
                arguments.Append(kvp.Value);
                arguments.Append("\" ");
            }
            var working = Path.Combine(Directory.GetCurrentDirectory(), this.MainLoc);
            var info = new ProcessStartInfo {
                WorkingDirectory = working,
                FileName = Path.Combine(working, "Entry_CMD.exe"),
                Arguments = arguments.ToString().Trim(),
                UseShellExecute = false
            };
            var process = new Process {
                StartInfo = info
            };
            return process;
        }
    }
}

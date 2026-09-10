# MCLCE-Texture-Converter-CS-Bridge
A simple C# bridge library for the MCLCE-Texture-Converter. Wraps the terminal implementation of the [MCLCE-Texture-Converter](https://github.com/xLeviadeer/MCLCE-Texture-Converter?tab=License-1-ov-file) to execute the Texture Converter using a Native C# `EntryPoint` implementation.

The compilation of this project is intended for x64 Windows. The actual Texture Converter does not support multiple operating systems so constructing this to work on multiple OS's does not make sense. 

## How to Use The CS-Bridge
### Step 1 — Download
You need to download **¡two¡** different components to use the bridge ⌄
1. the `MCLCE-Texture-Converter-CS-Bridge.dll`
2. the `builder_cmd.zip`

Both files are included in each [release](https://github.com/xLeviadeer/MCLCE-Texture-Converter-CS-Bridge/releases) for this project. 

### Step 2 — Install in Your Project
#### Step 2.1 — Install the `.dll`
Add `MCLCE-Texture-Converter-CS-Bridge.dll` as a `Reference` in your project. There are several ways to do this. One way is adding it directly to your `.csproj` ⌄
1. Open your `.csproj` file
2. Locate the `ItemGroup` containing `Reference`s
3. Add the following reference ⌄
```xml
<Reference Include="MCLCE_Texture_Converter_CS_Bridge">
    <HintPath>lib\MCLCE-Texture-Converter-CS-Bridge.dll</HintPath>
</Reference>
```

The ⟨HintPath⟩ is where you **¡also must place the actual `.dll` file¡**. In the directory with your `.csproj` create a folder, `lib` and place `MCLCE-Texture-Converter-CS-Bridge.dll` inside. 

#### Step 2.2 — Install the `builder_cmd`
1. Extract the contents of `builder_cmd.zip` into a folder `builder_cmd` *which should directly contain the contents 🜚 `_internal`, `base_textures`, ⌯
2. Place the extracted folder called `builder_cmd` in **¡the same directory as where your final project executable lives¡**. 

The `builder_cmd` must be relatively accessible from your compiled final project in order to run. 

## How to Use The CS-Bridge
The CS-Bridge provides a few very small wrapper classes that can hold data about Texture Converter execution settings and create a `Process` from the data. An example ConsoleApp is provided ⌄
```cs
using MCLCE_Texture_Converter_CS_Bridge;

internal class Program {
    static void Main(string[] args) {
        var entry = new EntryPoint {
            InputPath = "‹absolute path to input texture pack›",
            InputPathType = InputPathType.Folder,
            InputGame = InputGame.Java,
            InputVersion = new Version(1, 17),
            OutputPath = "‹absolute output path›",
            OutputStructure = OutputStructure.WiiuDump
        };
        var process = entry.ToProcess();
        process.Start();
        process.WaitForExit();
    }
}
```

## How to Build The CS-Bridge
### Step 1 — Build the `.dll`
This project was created with .NET 4.8 in VS2026. attempting to run the project should create a `.dll` in the `bin` directory. This is the `.dll` needed. 

### Step 2 — Build the `builder_cmd`
The `builder_cmd` is a frontend for the Texture Converter which provides access to the tool via the command line. The Texture Converter contains an *internal* function for generating builds of the Texture Converter. 

1. Clone the [MCLCE-Texture-Converter](https://github.com/xLeviadeer/MCLCE-Texture-Converter?tab=License-1-ov-file) 
2. Ensure all dependencies are installed
    - Dependencies can be found in the `pyproject.toml`
    - You will need to install [xLPyBasics](https://github.com/xLeviadeer/xLPyBasics)
3. Install `PyInstaller` (`pip install PyInstaller`)
4. Run `Entry_Internal.py` 
5. Select option `1` by typing 1 and hitting enter in the terminal
6. Select option `1` again by typing 1 and hitting enter in the terminal
    - The Texture Converter should take some time to process while it builds the terminal executable frontend
    - In the event of a failiure the fundamental actions that the automated builder does are as follows (implementation can be found in `TextureLibs/Internal.py`) ⌄
        - creates a massive command to include all files needed for the Texture Converter
        - adds data files
        - runs the PyInstaller command and waits for it to build
        - moves distributable to `builder_‹name›`
        - cleans up the directories after generation
# FFXIVModelConverter
This application is designed for converting Final Fantasy XIV files between in-game format and FBX. Main goal for this application is to use it alongside with Penumbra to speed up changes testing process.

Current features:
| | |
| --- | :---: |
| FBX -> MDL conversion | :heavy_check_mark: |
| MDL -> FBX conversion | Experimental |
| Textures import | Not planned for now |
| Textures export | Planned eventually |

**It is recommended to use TexTools for everything other than FBX -> MDL conversion.**

**⚠ Please note: This application is provided as-is, no support is provided. Only issues containing bug reports will be reviewed. ⚠**

This application is heavily based on the code developed as a part of [TexTools project](https://github.com/TexTools/FFXIV_TexTools_UI). Big thanks to everyone who contributed to that project.

## Usage
#### Setting up
After downloading application you need to edit `settings.json` as following:
* `GameDataDirectory` should point to `game\sqpack\ffxiv` folder. `\\` should be used as a path separator.
* `GameLanguage` should be set to your game language. Allowed values: `en, ja, de, fr, ko, chs, zh (same as chs)`
#### Convert FBX into MDL (modding square enix model)
`.\FFXIVModelConverter.exe fbx2mdl --input c:\my_mods\mod.fbx  --output "c:\my_mods\build\chara\equipment\e0000\model\c0000e0000_top.mdl" --game-path chara/equipment/e0000/model/c0000e0000_top.mdl --override-incoming-race Hyur_Midlander_Female`

Where
* `--input` is the path to the fbx file
* `--output` is the path where mdl file will be saved
*  `--game-path` is the path to the model inside game archives
* `--override-incoming-race` is one of the optional settings. Run `.\FFXIVModelConverter.exe fbx2mdl` to see more details about possible optional settings. **The settings are identical to TexTools, so please refer to TexTools tutorials and documentation for more information about those.**
#### Convert FBX into MDL (when you are changing model from another mod or the model is stored on disk)
`.\FFXIVModelConverter.exe fbx2mdl --input c:\my_mods\mod.fbx  --output "c:\my_mods\build\chara\equipment\e0000\model\c0000e0000_top.mdl" --base-model "c:\another_mod\chara\equipment\e0000\model\c0000e0000_top.mdl" --game-path chara/equipment/e0000/model/c0000e0000_top.mdl --override-incoming-race Hyur_Midlander_Female`

Where:
* `--base-model` is the path to mdl file your model is based on.

#### Convert MDL to FBX (experimental)
`.\FFXIVModelConverter.exe mdl2fbx --input "c:\mod_directory\chara\equipment\a0000\model\c0000a0000_top.mdl" --output "c:\mod_src\c0000a0000_top.fbx" --game-path chara/equipment/a0000/c0000a0000_top.mdl`

Where:
* `--input` is the path to the mdl file
* `--output` is the path to the fbx file
* `--game-path` is the path to the model inside game archives

## License
All files in this repository are licensed under the license listed in LICENSE.md file unless stated otherwise.

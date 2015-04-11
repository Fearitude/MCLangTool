This simple tool lets you convert .lang files used in minecraft mods to csv for easier editing.

Created by Fearitude
http://www.stealthware.co.uk
Licensed under GPLv3



Available Arguments (general):
/DevDir=\"PATH\"
	Specify the top level of the development directory. This should contain sub-folders for mods.
	You must use \" if the path contains spaces!
/TransDir=\"PATH\"
	Specify the translation directory. This is where CSV files will be written.
	You must use \" if the path contains spaces!
/Mod=Name OR /Mod=Name,AssetDir
	Add a mod to be processed. Can use multiple arguments to add more mods.
	Name is used as the mod name in files and also must match the folder in DevDir.
	AssetDir can be specified if the folder in the assets path needs to be different from Name.

Available Arguments (task):
/Input=FORMAT
	Selects the input format for the current conversion task.
	Supported Formats: Lang, CSV, GSheet
/Output=FORMAT
	Selects the output format for the current conversion task.
	Supported Formats: Lang, CSV, GSheet




https://developers.google.com/google-apps/spreadsheets/#setting_up_your_client_library




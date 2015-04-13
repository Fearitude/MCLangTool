# MCLangTool v0.3

This simple tool lets you convert .lang files used in Minecraft mods to CSV or Google Docs for easier editing.

Created by Fearitude  
http://www.stealthware.co.uk  
Licensed under GPLv3


## Usage

   MCLangTool.exe [OPTIONS] /Input=FORMAT /Output=FORMAT /Mod=MODNAME
   
### Required Arguments
* /Input=_FORMAT_  
   Selects the input format for the current conversion task.  
   Supported Formats: { _Lang_, _CSV_, _GSheet_ }

* /Output=_FORMAT_  
   Selects the output format for the current conversion task.  
   Supported Formats: { _Lang_, _CSV_, _GSheet_ }

* /Mod=_Name_ __OR__ /Mod=_Name_,_AssetDir_  
   Add a mod to be processed. This argument can be specified multiple times to add more mods.  
   Name is used as the mod name in files and also must match the folder in DevDir.  
   AssetDir can be specified if the folder in the assets path needs to be different from Name.  
   _At least one mod is required or the conversion will abort._

### Optional Arguments
* /DevDir=_"PATH"_  
   Specify the top level of the development directory. This should contain sub-folders for mods.  
   You must use " if the path contains spaces!  
   _Required if using the Lang format._

* /TransDir=_"PATH"_  
   Specify the translation directory. This is where CSV files will be written.  
   You must use " if the path contains spaces!  
   _Required if using the CSV format._

* /SheetId=_ID_  
   Specify ID of the Google Docs SpreadSheet to use. You need to create a spreadsheet if you dont already have one, and get the id from the url.  
   _Required if using the GSheet format._


## Google Docs API

A Google Docs API account, with a client_id and client_secret, is required to use the GSheet format.
See the link below to create your account and get the required information.
https://developers.google.com/google-apps/spreadsheets/#setting_up_your_client_library

Once thats done, create a text file named _googleapi.txt_ in the same directory as the executable.
Paste in your client_id on the first line and your client_secret on the second line.


## Examples

For examples of how to use this, see the two cmd scripts I use with my mods:
* LangToCSV.cmd
* CSVToLang.cmd

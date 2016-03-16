AutomatedTranslation
====================
## Summary
This project was created to allow Visual Studio developers to globalize their software. easily.  This tool creates automatic translations using an online translation service.
## Description 
This translation utility was built to support the new AngularJS GetText string translation library and the existing Microsoft String resource file.  It is able to get a basic and approximate translation from an on-line translation service such as Bing Translator, Google Translate, BabelFish, etc.  This tool was created to help developers update their resource files.
### Release Notes
At present only the Bing and Google translation engine are supported.
## Command Line
The utility will accept the following command line arguments:
* /languagePath - The is the path to the PO and POT language files that would be generated by a tool such as [POEdit](https://poedit.net).  These language files are used by Angular Get-Text to perform the translations in your AngularJS web application.
* /strResourcePath - This is the path to the Microsoft String Resources (strings.resx).  These string resources are commonly used by .NET applications to perform server side language translation.
* /engine - This option will specify the language engine that you would like to use for translation.  The current options are: **[ bing, google ]**

### Limitations
* The PO language files MUST have a country assigned to them in order for the translation to work correctly.  _"Language: de_DE\n"_
* The Microsoft string resource file must be called "Strings.resx" that will be the source of all of the translation.  When adding translations you would need to add them to this file.
* The Microsoft string resource files for all of your translation must be named for the language that you want to translate them into. _i.e. German must be de_DE.resx_
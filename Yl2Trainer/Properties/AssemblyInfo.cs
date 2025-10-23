using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(Yl2Trainer.BuildInfo.Description)]
[assembly: AssemblyDescription(Yl2Trainer.BuildInfo.Description)]
[assembly: AssemblyCompany(Yl2Trainer.BuildInfo.Company)]
[assembly: AssemblyProduct(Yl2Trainer.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Yl2Trainer.BuildInfo.Author)]
[assembly: AssemblyTrademark(Yl2Trainer.BuildInfo.Company)]
[assembly: AssemblyVersion(Yl2Trainer.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Yl2Trainer.BuildInfo.Version)]
[assembly: MelonInfo(typeof(Yl2Trainer.Yl2Trainer), Yl2Trainer.BuildInfo.Name, Yl2Trainer.BuildInfo.Version, Yl2Trainer.BuildInfo.Author, Yl2Trainer.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]
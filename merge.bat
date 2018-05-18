CD CastHelper\bin\Release
MKDIR Merged
"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe" /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7" /out:Merged\casthelper.exe casthelper.exe Newtonsoft.Json.dll System.Reactive.Core.dll System.Reactive.Interfaces.dll System.Reactive.Linq.dll System.Reactive.PlatformServices.dll System.Reactive.Windows.Threading.dll Zeroconf.dll
@PAUSE

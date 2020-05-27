CD CastHelper\bin\Release
MKDIR Merged
"%USERPROFILE%\.nuget\packages\ilmerge\3.0.40\tools\net452\ILMerge.exe" /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2" /out:Merged\casthelper.exe casthelper.exe RokuDotNet.Client.dll System.Reactive.Core.dll System.Reactive.Interfaces.dll System.Reactive.Linq.dll System.Reactive.PlatformServices.dll System.Reactive.Windows.Threading.dll Zeroconf.dll
@PAUSE

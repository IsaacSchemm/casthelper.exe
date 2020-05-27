CD CastHelper\bin\Release
MKDIR Merged
"%USERPROFILE%\.nuget\packages\ilmerge\3.0.40\tools\net452\ILMerge.exe" /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2" /out:Merged\casthelper.exe casthelper.exe RokuDotNet.Client.dll System.Reactive.dll System.Runtime.CompilerServices.Unsafe.dll System.Threading.Tasks.Extensions.dll System.ValueTuple.dll Zeroconf.dll

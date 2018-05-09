CD CastHelper\bin\Release
MKDIR Merged
"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe" /targetplatform:"v4,C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7" /out:Merged\casthelper.exe casthelper.exe Bonjour.NET.dll DnsResolver.dll Network.dll Services.NET.dll
@PAUSE

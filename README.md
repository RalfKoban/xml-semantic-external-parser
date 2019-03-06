# xml-semantic-external-parser
A semantic external parser for XML files that can be used together with [GMaster](https://gmaster.io), [PlasticSCM](https://www.plasticscm.com) or [SemanticMerge](https://semanticmerge.com/).

How to use it with GMaster is documented [here](http://blog.gmaster.io/2018/03/using-external-parsers-with-gmaster.html).

## Build status
[![Build status](https://ci.appveyor.com/api/projects/status/9dnbofw2gpedfiaa?svg=true)](https://ci.appveyor.com/project/RalfKoban/xml-semantic-external-parser/branch/master)
[![codecov](https://codecov.io/gh/RalfKoban/xml-semantic-external-parser/branch/master/graph/badge.svg)](https://codecov.io/gh/RalfKoban/xml-semantic-external-parser)

[![Build history](https://buildstats.info/appveyor/chart/RalfKoban/xml-semantic-external-parser)](https://ci.appveyor.com/project/RalfKoban/xml-semantic-external-parser/history)

## Issues
[![Waffle.io - Columns and their card count](https://badge.waffle.io/RalfKoban/xml-semantic-external-parser.svg?columns=all)](https://waffle.io/RalfKoban/xml-semantic-external-parser)

Please raise issues on [GitHub](https://github.com/RalfKoban/xml-semantic-external-parser/issues).
If you can repeat the issue then please provide a sample to make it easier for me to also repeat it and then implement a fix.

Please do not hijack unrelated issues, I would rather you create a new issue than add noise to an unrelated issue.

## Supported formats

| Description | File name / extension |
|-------------|-----------------------|
| .NET API reference XML comments | .xml
| .NET Application manifest | .manifest
| .NET Assembly manifest | .manifest
| .NET Configuration | .config
| .NET Settings | .settings
| C# Rule sets | .ruleset
| dotCover (NDepend XML format) | .xml
| Entity Framework Database Schema (V3) | .edmx
| FxCop | _CustomDictionary.xml_
| Microsoft Build Engine (MSBuild) | .proj, .targets, .props, .projitems
| Microsoft Prism (Library 5.0 for WPF) Module catalog | .xaml
| [NDepend](https://www.ndepend.com/) | .ndproj, .ndrules
| NuGet Configuration | _packages.config_
| NuGet Manifest | .nuspec
| [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB) Project  | .shfbproj
| Visual Build | .bld
| Visual Studio C++ Project | .vcxproj
| Visual Studio C# Project | .csproj
| Visual Studio CIL assembler Project | .ilproj
| Visual Studio Code Sharing App Project | .shproj
| Visual Studio Modeling Project | .modelproj
| Visual Studio SQL Server Project | .sqlproj
| Visual Studio Visual Basic Project | .vbproj
| Visual Studio Installer XML | .vsixmanifest
| WPF | .xaml
| [Wix Toolkit](http://wixtoolset.org/) | .wxi, .wxl, .wxs
| XML Localization Interchange File Format | .xlf
| XML | .xml
| XSD | .xsd
| XSL Transformation (XSLT) | .xsl, .xslt

_**Note:** A semantic parser for .NET resource files (.resx) can be found [here](https://github.com/RalfKoban/resx-semantic-external-parser)._

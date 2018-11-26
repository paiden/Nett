# What is Nett?
Nett is a library to read and write [TOML](https://github.com/toml-lang/toml) files in .Net.

| [Release Notes](http://paiden.github.io/Nett/RELEASENOTES.html) | [Documentation](http://paiden.github.io/Nett/) | [NuGet](https://www.nuget.org/packages/Nett/) |

# How to build?
## Prerequisites
1. [Visual Studio 2017+](https://visualstudio.microsoft.com/downloads/) with .Net development workload
1. [DocFX](https://dotnet.github.io/docfx/index.html) for generating the documentation 


## Steps

### Build Assemblies
1. After initial checkout the first build has to be done via running `.\build.ps1`
in PowerShell (this will auto generate some code files)
2. After that The solution can be build in Visual Studio

### Build & check documentation
1. Run `docfx.exe dfx\docfx.json --serve`
1. Open Browser with `http://localhost:8080`

### Build NuGet package

1. Run `.\build.ps1 -ngp` in PowerShell

# Contributing 

The primary way to contribute to `Nett` is via creating and discussing issues. 
You can also submit commit pull requests. But often filing a issue will be 
quicker and less painful.

Often, issues with good reproduction steps, will get fixed within a few days.

## Filing issues
Filing good issues can help to quickly fix bugs or add functionality you deem
important.

### Bugs

+ Try to add good reproduction steps
  + In General: Some code fragment is worth more than a 1000 words
  + Ideally you have a few lines of unit test code that reproduce the issue.
  + Or add some pseudo code to the issue 
+ If an exception is thrown
  + Include the exception type, message and stack trace
  + Try to use ```code blocks``` for formatting that
  + Also include inner and/or aggregate exceptions
+ Include a TOML fragment that causes an issue

### Features

+ Make a good clear use case description
+ Describe the benefits
+ What problems will that feature solve

## Pull requests

To ensure your PRs get merged you should ensure that the following 
requirements are met

+ Adhere to the existing coding standard
+ Added tests for the change you did
+ Ran all existing tests before submitting the PR
+ Added a section to RELEASENOTES.md

The final decision is up to the maintainer. Normally a reason 
why a PR is rejected will be given but is not mandatory.

Probably it's better to start a short discussion first instead
of doing a surprise PR. Also see 
[Don't "Push" Your Pull Request](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/)

**Note:** No written coding standard etc. is available. It has 
to be deducted from the existing code base.

Generally the directive in the Nett project is to add more
tool automation to check all these things automatically 
instead of written docs that are outdated the moment they 
are published.

More and more of these automated checks will be added over time. 






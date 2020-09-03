Solution Auditor
===============


## Purpose

This program will audit a solution for bad references. It includes checks for the following (command line flag in parentheses):

* Missing packages (-m, --missing)
* Unused packages (-u, --unused)
* Inconsistent versioning within the solution (-i, --inconsistent)
* The presence of snapshot packages (-s, --snapshot)
* Packages present only in a .csproj file or in packages.config (-f, --fileDiff)
* Binding Redirects where the new version doesn't match the version in the .csproj (-r, --redirectMismatch)
* Nuget commands for fixing missing or out of sync versions (-n, --nugetCommands)

By default, all checks will be run. To run a subset of checks, pass in their flags on the command line.

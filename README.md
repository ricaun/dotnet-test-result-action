# dotnet-test-result-action

[![Visual Studio 2022](https://img.shields.io/badge/Visual%20Studio-2022-blue)](../..)
[![Nuke](https://img.shields.io/badge/Nuke-Build-blue)](https://nuke.build/)
[![Build](../../actions/workflows/Build.yml/badge.svg)](../../actions)

Example of a GitHub Actions workflow for building and testing a .NET Framework project.

## Nuke

Using [Nuke](https://github.com/nuke-build/nuke/commit/04f2c8bd37d73db72d3a2b629c2b7ec14670b8d6) to generate the summary of the tests in the solution.

## Actions References

- [upload-artifact](https://github.com/actions/upload-artifact)
- [publish-unit-test-result-action](https://github.com/EnricoMi/publish-unit-test-result-action)

### dotnet test
```
dotnet test Tests.csproj --configuration Release --logger trx;LogFileName=Tests.Release.trx --results-directory .\TestResults --verbosity Normal
```
```
dotnet test Tests.csproj --configuration Debug --logger trx;LogFileName=Tests.Debug.trx --results-directory .\TestResults --verbosity Normal
```

## License

This project is [licensed](LICENSE) under the [MIT Licence](https://en.wikipedia.org/wiki/MIT_License).

---

Do you like this project? Please [star this project on GitHub](../../stargazers)!
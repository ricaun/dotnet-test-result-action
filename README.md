# dotnet-test-result-action

[![Visual Studio 2022](https://img.shields.io/badge/Visual%20Studio-2022-blue)](../..)
[![Build](../../actions/workflows/Build.yml/badge.svg)](../../actions)

Example of a GitHub Actions workflow for building and testing a .NET Framework project.

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
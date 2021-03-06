Smith.MatrixSdk Maintainership
==============================

Publish a New Version
---------------------

1. Prepare a corresponding entry in `CHANGELOG.md` (usually by renaming the
   "Unreleased" section).
2. Set `<Version>` in `Smith.MatrixSdk/Smith.MatrixSdk.csproj`.
3. Push a tag in form of `v<VERSION>`, e.g. `v0.0.1`. GitHub Actions will do the
   rest (push a NuGet package).

Prepare NuGet Package Locally
-----------------------------

```console
$ dotnet pack --configuration Release -p:ContinuousIntegrationBuild=true
```

Push a NuGet Package Manually
-----------------------------

```console
$ dotnet nuget push ./Smith.MatrixSdk/bin/Release/Smith.MatrixSdk.<VERSION>.nupkg --source https://api.nuget.org/v3/index.json --api-key <YOUR_API_KEY>
```

# Run 01

## Command
```bash
dotnet test /Users/abarmpas/Documents/GitHub/aatp_playwright_dotnet/AatpTemplateTestSuite.sln --nologo --filter FullyQualifiedName~Login 
```

## Result
FAIL

## Filter
- FullyQualifiedName~Login

## Exit code
1

## Excerpt
```
  Determining projects to restore...
/Users/abarmpas/Documents/GitHub/aatp_playwright_dotnet/AatpTemplateTestSuite/AatpTemplateTestSuite.csproj : error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json. [/Users/abarmpas/Documents/GitHub/aatp_playwright_dotnet/AatpTemplateTestSuite.sln]
  Failed to restore /Users/abarmpas/Documents/GitHub/aatp_playwright_dotnet/AatpTemplateTestSuite/AatpTemplateTestSuite.csproj (in 5.94 sec).
```

## Notes
- Raw log: `06-run/run-01.raw.txt`

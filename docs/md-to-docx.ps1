<#
.SYNOPSIS
  Convert Markdown to DOCX using Pandoc and a reference Word template (DOCX).
  Run from the Documentation folder (or pass absolute paths).
  DOCX is written to the matching Word Docs folder:
  - Type 2\*.md  ->  Type 2\Word Docs\*.docx
  - Type 1\Documentation\*.md  ->  Type 1\Documentation\Word Docs\*.docx
  (In general: ...\Word Docs\ next to the folder that contains the .md file.)
  Use -OutputPath to override the output file for a single input.
  Use -ReferenceDoc to override the reference (template) DOCX used for formatting.
.EXAMPLE
  cd Documentation
  .\md-to-docx.ps1 ".\Type 2\AI Use Self-Checklist for Employees.md"
.EXAMPLE
  # Convert a Markdown file stored directly under Documentation\
  cd Documentation
  .\md-to-docx.ps1 ".\Markdown-Elements-Style-Sample.md"
.EXAMPLE
  # Use your tuned sample DOCX as the reference template
  cd Documentation
  .\md-to-docx.ps1 ".\Type 2\AI Use Self-Checklist for Employees.md" -ReferenceDoc ".\Markdown-Elements-Style-Sample.docx"
.EXAMPLE
  Get-ChildItem ".\Type 2\*.md" | .\md-to-docx.ps1
.EXAMPLE
  .\md-to-docx.ps1 ".\Type 1\Documentation\Access Control.md"
.EXAMPLE
  Get-ChildItem ".\Type 1\Documentation\*.md" | .\md-to-docx.ps1
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
  [Alias("FullName")]
  [string[]] $MarkdownPath,

  [Parameter()]
  [string] $OutputPath,

  [Parameter()]
  [string] $ReferenceDoc
)

begin {
  $DocumentationRoot = (Resolve-Path -LiteralPath $PSScriptRoot).Path.TrimEnd('\')

  if (-not $ReferenceDoc) {
    # Default reference template (stored next to this script)
    $ReferenceDoc = Join-Path $DocumentationRoot "Markdown-Elements-Style-Sample.docx"
  }
  else {
    # Allow relative paths from the Documentation folder for convenience
    if (-not [System.IO.Path]::IsPathRooted($ReferenceDoc)) {
      $ReferenceDoc = Join-Path $DocumentationRoot $ReferenceDoc
    }
  }

  if (-not (Test-Path -LiteralPath $ReferenceDoc)) {
    Write-Error "Reference document not found: $ReferenceDoc"
    exit 1
  }

  Write-Host "Using reference DOCX: $ReferenceDoc"

  $PandocExe = $null
  $onPath = Get-Command pandoc -ErrorAction SilentlyContinue
  if ($onPath) {
    $PandocExe = $onPath.Source
  }
  else {
    foreach ($candidate in @(
        (Join-Path $env:LOCALAPPDATA "Pandoc\pandoc.exe"),
        (Join-Path $env:ProgramFiles "Pandoc\pandoc.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Pandoc\pandoc.exe")
      )) {
      if (Test-Path -LiteralPath $candidate) {
        $PandocExe = $candidate
        break
      }
    }
  }

  if (-not $PandocExe) {
    Write-Error "pandoc not found. Install: winget install --id JohnMacFarlane.Pandoc -e. If it is installed, close and reopen the terminal so PATH updates, or reinstall for all users."
    exit 1
  }

  Write-Host "Using pandoc: $PandocExe"
}

process {
  $paths = @($MarkdownPath)
  if ($OutputPath -and $paths.Count -gt 1) {
    Write-Error "-OutputPath may only be used with a single Markdown file."
    return
  }

  foreach ($path in $paths) {
    $resolved = Resolve-Path -LiteralPath $path -ErrorAction Stop
    $inFile = $resolved.Path

    if ($inFile.Length -le $DocumentationRoot.Length -or
        -not ($inFile.StartsWith($DocumentationRoot, [StringComparison]::OrdinalIgnoreCase))) {
      Write-Error "Markdown file must be under the Documentation folder: $inFile"
      continue
    }

    if ($OutputPath) {
      $outFile = $OutputPath
    }
    else {
      $sourceDir = Split-Path -Parent $inFile

      # Map output into the appropriate Word Docs folder.
      # - If the markdown is under Type 1\Documentation\*, write to Type 1\Documentation\Word Docs\
      # - If the markdown is under Type 2\*, write to Type 2\Word Docs\
      # - Otherwise (e.g., Documentation\*.md), write to Documentation\Word Docs\
      $relativeToDoc = $inFile.Substring($DocumentationRoot.Length).TrimStart('\')
      $wordDocsDir = $null
      if ($relativeToDoc -like "Type 1\\Documentation\\*") {
        $wordDocsDir = Join-Path $DocumentationRoot "Type 1\Documentation\Word Docs"
      }
      elseif ($relativeToDoc -like "Type 2\\*") {
        $wordDocsDir = Join-Path $DocumentationRoot "Type 2\Word Docs"
      }
      else {
        $wordDocsDir = Join-Path $sourceDir "Word Docs"
      }

      if (-not (Test-Path -LiteralPath $wordDocsDir)) {
        New-Item -ItemType Directory -Path $wordDocsDir -Force | Out-Null
      }
      $baseName = [System.IO.Path]::GetFileNameWithoutExtension($inFile)
      $outFile = Join-Path $wordDocsDir "$baseName.docx"
    }

    & $PandocExe $inFile -o $outFile --reference-doc=$ReferenceDoc
    if ($LASTEXITCODE -ne 0) {
      Write-Error "pandoc failed for: $inFile (exit $LASTEXITCODE)"
      continue
    }
    Write-Host "Wrote: $outFile"
  }
}

param (
  [string]$serviceName
)

if (-not $serviceName) {
  Write-Host "Usage: ./start-wd.ps1 <service-name>"
  exit 1
}

Write-Host "Copying proto files..."
Remove-Item -Recurse -Force ./protos -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path ./protos | Out-Null
Copy-Item -Recurse ../../protos/* ./protos/

Write-Host "Generating proto types for $serviceName..."
$protoFiles = Get-ChildItem -Recurse ../../protos -Filter *.proto | ForEach-Object { $_.FullName }

protoc `
  --plugin=protoc-gen-ts_proto=./node_modules/.bin/protoc-gen-ts_proto `
  --ts_proto_out=./apps/$serviceName/src/types `
  --ts_proto_opt=nestJs=true,outputTypePrefix=true `
  --proto_path=../../protos `
  $protoFiles

protoc `
  --plugin=protoc-gen-ts_proto=./node_modules/.bin/protoc-gen-ts_proto `
  --ts_proto_out=./libs/core/src/types `
  --ts_proto_opt=nestJs=true,outputTypePrefix=true `
  --proto_path=../../protos `
  $protoFiles

Write-Host "Serving $serviceName with Nx..."
npx nx serve $serviceName --skip-nx-cache

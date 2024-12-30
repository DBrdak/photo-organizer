
try {
    Set-Location ./API
    sam build
    sam package --s3-bucket 'photo.organizer' --output-template-file packaged.yaml
    sam deploy `
        --template-file packaged.yaml `
        --stack-name photo-organizer `
        --capabilities CAPABILITY_IAM
    
    Write-Host "Deploy successful!" -ForegroundColor Green
}
catch {
    Write-Host "Deploy failed!" -ForegroundColor Red
}
finally {
    Set-Location ..
}
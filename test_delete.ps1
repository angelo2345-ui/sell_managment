
# 1. Create Product
$pBody = @{ Name="TestDelete"; Description="Desc"; Price=10; Stock=100 } | ConvertTo-Json
$p = Invoke-RestMethod -Method Post -Uri "http://localhost:5227/api/products" -Body $pBody -ContentType "application/json"
$pId = $p.data # Assuming Create returns data? No, CreateProduct returns null data in my code.
# Wait, CreateProduct returns "Data = null". I need to find the ID.
# I'll search for it by name.
$allP = Invoke-RestMethod -Method Get -Uri "http://localhost:5227/api/products"
$product = $allP.data | Where-Object { $_.name -eq "TestDelete" } | Select-Object -Last 1
$pId = $product.id
Write-Host "Product Created: $pId"

# 2. Create Client
$cBody = @{ Name="TestClient"; Email="t@t.com"; Phone="123" } | ConvertTo-Json
$c = Invoke-RestMethod -Method Post -Uri "http://localhost:5227/api/clients" -Body $cBody -ContentType "application/json"
# Find Client ID
$allC = Invoke-RestMethod -Method Get -Uri "http://localhost:5227/api/clients"
$client = $allC.data | Where-Object { $_.name -eq "TestClient" } | Select-Object -Last 1
$cId = $client.id
Write-Host "Client Created: $cId"

# 3. Create Sale
$detail = @{ ProductId=$pId; Quantity=1; UnitPrice=10 }
$sBody = @{ ClientId=$cId; Total=10; Details=@($detail) } | ConvertTo-Json -Depth 5
try {
    $s = Invoke-RestMethod -Method Post -Uri "http://localhost:5227/api/sales" -Body $sBody -ContentType "application/json"
    Write-Host "Sale Created: $($s.data.saleId)"
} catch {
    Write-Host "Error creating sale: $_"
    exit
}

# 4. Delete Product (Should trigger Soft Delete)
try {
    $del = Invoke-RestMethod -Method Delete -Uri "http://localhost:5227/api/products/$pId"
    Write-Host "Delete Result: $($del.message)"
} catch {
    Write-Host "Delete Failed: $_"
    # Print the response body if available
    $_.Exception.Response.GetResponseStream() | %{ [System.IO.StreamReader]::new($_).ReadToEnd() }
}


$ErrorActionPreference = "Stop"

try {
    Write-Host "1. Creando producto de prueba..."
    $body = @{
        Name = "ProductoPruebaDelete"
        Description = "Para probar borrado"
        Price = 123.45
        Stock = 10
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Method Post -Uri "http://localhost:5227/api/products" -Body $body -ContentType "application/json"
    Write-Host "   Respuesta Creacion: $($response.message)"

    # Obtener el ID del producto creado (buscamos por nombre ya que el create no devuelve ID explicito en este diseño simple)
    $allProducts = Invoke-RestMethod -Method Get -Uri "http://localhost:5227/api/products"
    $product = $allProducts.data | Where-Object { $_.name -eq "ProductoPruebaDelete" } | Select-Object -Last 1

    if ($product) {
        $id = $product.id
        Write-Host "   Producto creado con ID: $id"

        Write-Host "2. Intentando eliminar producto ID: $id..."
        $deleteResponse = Invoke-RestMethod -Method Delete -Uri "http://localhost:5227/api/products/$id"
        Write-Host "   Respuesta Eliminacion: $($deleteResponse.message)"
        
        Write-Host "3. Verificando si desaparecio de la lista..."
        $finalProducts = Invoke-RestMethod -Method Get -Uri "http://localhost:5227/api/products"
        $check = $finalProducts.data | Where-Object { $_.id -eq $id }
        
        if (-not $check) {
            Write-Host "   EXITO: El producto ya no aparece en la lista (Soft Delete funciono)."
        } else {
            Write-Host "   FALLO: El producto sigue apareciendo en la lista."
        }

    } else {
        Write-Host "   Error: No se pudo encontrar el producto creado para borrarlo."
    }

} catch {
    Write-Host "ERROR CRITICO: $_"
    Write-Host "Detalles: $($_.Exception.Response)"
}

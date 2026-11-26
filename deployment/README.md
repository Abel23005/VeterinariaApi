# Guía de publicación de la solución

Esta guía deja la solución preparada para publicar los tres procesos y la base de datos. La publicación real requiere una suscripción del estudiante, nombres disponibles y secretos que no deben compartirse en el repositorio.

## Proveedor elegido y justificación

Se conserva SQL Server para no cambiar la lógica ni el proveedor EF Core de la S14. Azure SQL Database Free Tier ofrece una base General Purpose con una cuota mensual gratuita de 100 000 vCore-seconds, 32 GB de datos y 32 GB de copias de seguridad. Azure App Service permite publicar ASP.NET Core en el plan Free F1. Esta opción mantiene compatibilidad con `Microsoft.EntityFrameworkCore.SqlServer` y evita adaptar el script T-SQL a otro motor.

Los límites son adecuados para una sustentación, no para producción: la base puede pausarse al alcanzar el límite y App Service Free tiene límites de CPU, almacenamiento y disponibilidad. Confirma el precio cero en el portal y configura el comportamiento de exceso para pausar, no para facturar.

## 1. Crear la base Azure SQL

1. En Azure Portal abre Azure SQL Hub y elige crear una base con la opción `Start free`.
2. Crea un servidor lógico, una base `VeterinariaDb` y permite temporalmente la IP que usará `sqlcmd` o el editor de consultas.
3. Ejecuta, en este orden, `sql/01_create_database.sql` y `sql/02_auth_jwt.sql`. En Azure SQL el script debe ejecutarse sobre la base `VeterinariaDb`; si el primer script intenta crear la base, puedes omitir esa instrucción y crearla desde el portal.
4. Copia la cadena ADO.NET del portal y guárdala únicamente como configuración de la aplicación `Veterinaria.Api`.

## 2. Publicar los tres servicios

Crea tres Web Apps ASP.NET Core 10 en App Service Free F1:

| Aplicación | Proyecto | Uso |
|---|---|---|
| `gateway-api` | `GatewayApi` | único host que usa el cliente |
| `veterinaria-api` | `Veterinaria.Api` | API principal y conexión a Azure SQL |
| `propietarios-api` | `Propietarios.Api` | datos en memoria y JWT |

Publica cada proyecto desde Visual Studio/VS Code o desde CI. En cada aplicación configura `ASPNETCORE_ENVIRONMENT=Production`. En `Veterinaria.Api` agrega:

```text
ConnectionStrings__Veterinaria=<cadena de Azure SQL>
Jwt__Clave=<misma clave aleatoria de al menos 32 caracteres>
Jwt__Emisor=VeterinariaApi
Jwt__Audiencia=VeterinariaClientes
```

En `Propietarios.Api` agrega las tres variables `Jwt__...`. En `GatewayApi` agrega las tres variables `Jwt__...` y publica el archivo `ocelot.Production.json` junto con el proyecto.

## 3. Configurar el gateway público

Después de conocer los tres hostnames, copia `GatewayApi/ocelot.Production.example.json` a `GatewayApi/ocelot.Production.json` y reemplaza:

- `<gateway-api>.azurewebsites.net` por el host HTTPS real del gateway.
- `<veterinaria-api>.azurewebsites.net` por el host HTTPS real de `Veterinaria.Api`.
- `<propietarios-api>.azurewebsites.net` por el host HTTPS real de `Propietarios.Api`.

Todos los downstreams públicos deben usar `DownstreamScheme` `https` y puerto `443`. Verifica que el gateway se haya reiniciado después de publicar el archivo.

## 4. Pruebas locales y públicas

1. Obtén un token con `POST /gateway/auth/login`.
2. Repite `GatewayApi/requests.http` con `@gateway` apuntando primero a `http://localhost:5000` y luego al HTTPS público del gateway.
3. Registra la respuesta y toma capturas para 200, 201, 204, 401, 403, 404 y 502.
4. Para 502, detén `Propietarios.Api` localmente; en nube detén temporalmente la aplicación o usa una ruta con host downstream incorrecto solo durante la captura y restáurala de inmediato.
5. Llama directamente a `Propietarios.Api` sin token y guarda la respuesta 401.
6. Abre los tres hosts desde una ventana privada para comprobar que son accesibles por internet.

## 5. Evidencias y video

Completa en el documento de entrega el enlace del repositorio, los tres enlaces públicos, la tabla de rutas, las capturas, el enlace accesible del video y las conclusiones. El video debe durar al menos 10 minutos, mostrar la cámara durante toda la sustentación y explicar arquitectura, `ocelot.json`, JWT y las pruebas públicas, incluyendo 401, 403 y 502.

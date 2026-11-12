# API veterinaria por capas

API REST en ASP.NET Core 10 para registrar dueños y mascotas. Cada dueño puede tener varias mascotas; cada mascota pertenece a un dueño. Los controladores exponen solo DTOs.

## Arquitectura

- `Veterinaria.Entidades`: entidades y DTOs con Data Annotations.
- `Veterinaria.Datos`: `VeterinariaContext` y repositorios EF Core asíncronos.
- `Veterinaria.Negocio`: servicios, reglas de negocio, excepción propia y un `MappingProfile`.
- `Veterinaria.Api`: controladores, Swagger e inyección de dependencias.

La cadena de referencias es API → Negocio → Datos → Entidades. Las reglas incluyen correo de dueño único, impedir eliminar dueños con mascotas, validar fecha de nacimiento y exigir un dueño existente. Las reglas se traducen a HTTP 409.

## Base de datos primero

Ejecuta `sql/01_create_database.sql` en SQL Server. El script crea y carga `VeterinariaDb`. Luego, desde la raíz del repositorio instala la herramienta si hace falta y genera el modelo:

```powershell
dotnet tool install --global dotnet-ef --version 10.*
dotnet ef dbcontext scaffold "Server=localhost;Database=VeterinariaDb;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --project Veterinaria.Datos --startup-project Veterinaria.Api --output-dir Models --context VeterinariaContext --no-onconfiguring --force
```

El contexto y las entidades incluidos corresponden al esquema del script. En el flujo Scaffold exigido, mueve las clases `Dueno` y `Mascota` generadas a `Veterinaria.Entidades/Models` y actualiza sus namespaces; el contexto queda en Datos. La conexión local de ejemplo se configura en `Veterinaria.Api/appsettings.json`; usa secretos de usuario o variables de entorno para credenciales reales.

## Ejecutar

Con SQL Server disponible y la base creada:

```powershell
dotnet restore
dotnet build
dotnet run --project Veterinaria.Api
```

Swagger UI aparece en `/swagger` en entorno Development. La solución no incluye credenciales ni publica la API en la nube.

## Endpoints (11)

| Recurso | Método y ruta | Respuesta esperada |
|---|---|---|
| Dueños | `GET /api/duenos` | 200 |
| Dueño | `GET /api/duenos/{id}` | 200 / 404 |
| Dueños | `POST /api/duenos` | 201 / 400 / 409 |
| Dueño | `PUT /api/duenos/{id}` | 204 / 400 / 404 / 409 |
| Dueño | `DELETE /api/duenos/{id}` | 204 / 404 / 409 |
| Mascotas del dueño | `GET /api/duenos/{id}/mascotas` | 200 / 404 |
| Mascotas | `GET /api/mascotas?especie=Perro&duenoId=1` | 200 |
| Mascota | `GET /api/mascotas/{id}` | 200 / 404 |
| Mascotas | `POST /api/mascotas` | 201 / 400 / 409 |
| Mascota | `PUT /api/mascotas/{id}` | 204 / 400 / 404 / 409 |
| Mascota | `DELETE /api/mascotas/{id}` | 204 / 404 |

`Veterinaria.Api/requests.http` contiene solicitudes de ejemplo para ejecutar manualmente en REST Client, Swagger o Thunder Client.

Las pruebas automatizadas están en `Veterinaria.Tests`. Ejecuta `dotnet test Veterinaria.slnx`; el proyecto valida reglas de negocio y respuestas HTTP con una base SQLite en memoria, aislada para cada prueba. Se cubren 200, 201, 204, 400, 404 y 409.

El tema resuelve el problema de registrar dueños y sus mascotas en una veterinaria; evita duplicar correos, protege la relación 1:N al impedir borrar dueños con mascotas, y valida la fecha de nacimiento y la existencia del dueño al crear o editar mascotas.

## Observaciones y conclusión

La separación por capas mantiene persistencia y reglas fuera de los controladores; DTOs evitan exponer entidades y navegaciones de EF Core. Los filtros por especie y dueño facilitan consultas acotadas, y la ruta anidada permite consultar las mascotas del dueño. La suite valida las reglas de negocio y los códigos HTTP con un proveedor SQLite de prueba. Para validar la conexión real de SQL Server y capturar respuestas desde Swagger, crea la base con el script y ejecuta las solicitudes de `requests.http`.

## Evidencia pendiente

La suite automatizada pasó 13 de 13 pruebas. Repositorio público: https://github.com/Abel23005/VeterinariaApi. El registro está en `evidencias/pruebas.log`. Las capturas del Explorador de soluciones, del MappingProfile y de las respuestas HTTP en Swagger deben tomarse después de ejecutar el proyecto con SQL Server. No se incluyen capturas simuladas.

## Sustentación de mapeos

| CreateMap | Origen → destino | Motivo y configuración |
|---|---|---|
| `Dueno → DuenoDto` | Entidad → DTO lectura | La API devuelve datos seguros y derivados; `CantidadMascotas` usa `ForCtorParam` y `ForMember/MapFrom` con el conteo de la navegación. |
| `DuenoCrearDto → Dueno` | Entrada → entidad | Convierte los datos validados para insertar; propiedades homónimas. |
| `DuenoActualizarDto → Dueno` | Entrada → entidad existente | Actualiza campos permitidos; ignora ID y colección de mascotas para preservar estado persistido. |
| `Mascota → MascotaDto` | Entidad → DTO lectura | Respuesta plana para el cliente; `NombreDueno` toma `Dueno.Nombre` mediante `ForCtorParam` y `ForMember/MapFrom`. |
| `MascotaCrearDto → Mascota` | Entrada → entidad | Convierte datos de alta; ignora la clave generada y navegación, asignadas por la base de datos/relación. |
| `MascotaActualizarDto → Mascota` | Entrada → entidad existente | Actualiza campos editables; ignora clave y navegación, que se valida y conserva fuera del mapeo. |

No se usa `ReverseMap`: los modelos de entrada y salida tienen formas distintas y las navegaciones/claves requieren control explícito.

## Verificación académica pendiente

El código, el SQL y las solicitudes están preparados. No se adjuntan capturas de respuestas HTTP: para capturarlas se requiere ejecutar SQL Server, crear la base y correr las solicitudes localmente. Tras verificar cada respuesta, agrega las capturas solicitadas por la consigna junto con el enlace al repositorio.


## Capturas de pruebas automatizadas

- [Resumen de endpoints de dueños](evidencias/PruebasEndpoints-Duenos.png)
- [Resumen de endpoints de mascotas](evidencias/PruebasEndpoints-Mascotas.png)
- [Resultado de ejecución de pruebas (TRX)](evidencias/pruebas-endpoints.trx)

Las pruebas usan SQLite en memoria y verifican rutas, reglas y códigos indicados en cada resumen. No son capturas de Swagger ni validan una instancia externa de SQL Server.

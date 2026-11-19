# API veterinaria por capas

## Laboratorio 14 autenticación JWT

La API protege la información de dueños y mascotas. La Recepcionista puede crear y actualizar datos de dueños y registrar mascotas; la Veterinaria puede además actualizar/eliminar mascotas y eliminar dueños.

Aplica `sql/01_create_database.sql` y después `sql/02_auth_jwt.sql` en SQL Server. El segundo script crea `Usuarios` con correo único y agrega una cuenta por rol para pruebas. Ambas cuentas semilla usan la contraseña `VetCare123!`; son solo de laboratorio. La clave JWT real se obtiene de User Secrets, no del repositorio:

```powershell
dotnet user-secrets --project Veterinaria.Api set "ConnectionStrings:Veterinaria" "<cadena-local-de-tu-SQL-Server>"
dotnet user-secrets --project Veterinaria.Api set "Jwt:Clave" "pon-aqui-un-secreto-aleatorio-de-al-menos-32-caracteres"
dotnet run --project Veterinaria.Api
```

La web de la veterinaria se abre en la ruta raíz `/`; Swagger queda disponible en `/swagger`. Ninguna clave JWT ni cadena de conexión se almacena en los archivos del repositorio.

Swagger muestra el botón **Authorize**. Inicia sesión en `POST /api/auth/login`, copia el token y pégalo como Bearer. El registro crea únicamente usuarios Recepcionista; el servidor decide el rol. Para crear una cuenta de Veterinario, se asigna por un administrador en SQL después de registrar su correo.

### Matriz de permisos

| Endpoint | Sin token | Recepcionista | Veterinario |
|---|---:|---:|---:|
| `POST /api/auth/registro` y `POST /api/auth/login` | Sí | Sí | Sí |
| `GET /api/auth/perfil` | 401 | Sí | Sí |
| `GET /api/duenos`, `GET /api/mascotas` y consultas por ID | 401 | Sí | Sí |
| `POST /api/duenos`, `PUT /api/duenos/{id}`, `POST /api/mascotas` | 401 | Sí | Sí |
| `DELETE /api/duenos/{id}`, `PUT/DELETE /api/mascotas/{id}` | 401 | 403 | Sí |

`401` significa que falta un token válido o que el login falló; `403` significa que el token es válido pero su rol no tiene ese permiso. El JWT incluye `sub`, `email` y `rol`; no debe contener contraseñas ni hashes. La firma valida emisor, audiencia, vencimiento e integridad. Para regenerar una clave de prueba, usa el endpoint de registro y cambia el rol con SQL.

### Sustentación de seguridad

- El hash BCrypt se almacena en vez de la clave; una filtración de la base no revela directamente las contraseñas y cada hash incorpora salt.
- El rol se asigna en servidor y se firma en el JWT para impedir que el cliente se eleve permisos alterando su petición.
- Nunca se guardan claves JWT ni cadenas de conexión con credenciales en el repositorio. Usa User Secrets en desarrollo y un gestor de secretos en despliegue.

`Veterinaria.Api/requests.http` contiene ejemplos iniciales de registro, login, perfil y consulta protegida. Para revisar todos los códigos de la consigna (incluyendo 204, 401, 403 y 409), combina las peticiones existentes de la S13 con token de ambos roles y el SQL semilla.

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
```

El contexto y las entidades incluidos corresponden al esquema del script. En el flujo Scaffold exigido, mueve las clases `Dueno` y `Mascota` generadas a `Veterinaria.Entidades/Models` y actualiza sus namespaces; el contexto queda en Datos. Para Scaffold, carga la conexión local desde una variable de entorno que no subas al repositorio:

```powershell
$connectionString = $env:VETERINARIA_DB_CONNECTION
if (-not $connectionString) { throw 'Define VETERINARIA_DB_CONNECTION localmente.' }
dotnet ef dbcontext scaffold $connectionString Microsoft.EntityFrameworkCore.SqlServer --project Veterinaria.Datos --startup-project Veterinaria.Api --output-dir Models --context VeterinariaContext --no-onconfiguring --force
```

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

## Evidencia de verificación

Se ejecutaron 53 solicitudes HTTP contra la API local, conectada a SQL Server, con tokens de Recepcionista y Veterinario. El registro por caso está en `evidencias/pruebas-http-jwt-2026-11-18.csv`; incluye los resultados 200, 201, 204, 400, 401, 403, 404 y 409. Se limpiaron los registros temporales al terminar.


## Capturas de pruebas automatizadas

- [Resumen de endpoints de dueños](evidencias/PruebasEndpoints-Duenos.png)
- [Resumen de endpoints de mascotas](evidencias/PruebasEndpoints-Mascotas.png)
- [Resultado de ejecución de pruebas (TRX)](evidencias/pruebas-endpoints.trx)

Las pruebas automatizadas originales usan SQLite en memoria. Además, la evidencia CSV registra las verificaciones HTTP locales realizadas contra SQL Server; no contiene tokens ni contraseñas.

## Sustentación de seguridad

1. **401 y 403:** `GET /api/mascotas` sin token devolvió 401 porque no había autenticación. `DELETE /api/mascotas/8` con token válido de Recepcionista devolvió 403 porque la ruta requiere el rol Veterinario.
2. **Claims:** el token incluye `sub` para identificar al usuario, `email` para mostrar su perfil y `rol` para aplicar permisos. También incorpora emisor, audiencia y vencimiento. Nunca incluiría contraseñas, hashes, claves secretas ni datos sensibles innecesarios: el contenido del JWT firmado se puede leer, aunque no modificar sin invalidar la firma.
3. **Contraseña en texto:** si se filtra la base de datos, cualquiera puede leer las claves y probarlas en otros servicios donde se hayan reutilizado. BCrypt guarda un hash unidireccional con salt; el login verifica la clave contra ese hash.
4. **Rol asignado por el servidor:** el registro crea usuarios Recepcionista y no recibe el rol desde el cliente. Así, un usuario no puede solicitar permisos de Veterinario modificando su petición. La API lee el rol confiable de la base y lo firma en el token.

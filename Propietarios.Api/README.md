# Microservicio de propietarios

Segundo microservicio de la solución. Expone propietarios de la veterinaria con datos en memoria, sin base de datos ni referencias a los proyectos de la API principal.

## Endpoints

| Método | Ruta | Token | Respuesta |
|---|---|---|---|
| GET | `/api/propietarios` | Sí | 200 |
| GET | `/api/propietarios/{id}` | Sí | 200 o 404 |

Todos los endpoints tienen `[Authorize]` y validan el mismo emisor, audiencia y secreto que `Veterinaria.Api`. El servicio escucha en `http://localhost:5108`.

## Configuración local

```powershell
dotnet user-secrets --project Propietarios.Api set "Jwt:Clave" "una-clave-local-de-al-menos-32-caracteres"
dotnet run --project Propietarios.Api
```

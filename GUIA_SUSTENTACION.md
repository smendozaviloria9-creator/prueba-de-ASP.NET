# 🎓 Guía de Sustentación y Defensa — SkillHub (.NET 10)

## 1. 🎤 El Discurso Inicial (Tu presentación de 1 minuto)
> "Buenas tardes. Para esta prueba de desempeño desarrollé la plataforma **SkillHub**, una solución integral para gestionar talleres presenciales y sus inscripciones, resolviendo problemáticas reales como el sobrecupo, duplicados, cruces de horarios y control de cancelaciones.
> Implementé una arquitectura basada en **Clean Architecture** dividida en 4 capas desacopladas, utilizando **Entity Framework Core con Code First y PostgreSQL**, **ASP.NET Core Identity** con **JWT** para autorización basada en roles (`Empleado` y `Participante`), y dos capas de presentación: una **REST API con Swagger** y una **aplicación Web MVC** que consume directamente la capa de aplicación."

---

## 2. 🏛️ Cómo explicar la Arquitectura (Clean Architecture)

1. **`SkillHub.Domain` (El Núcleo):**
   - Contiene las entidades principales (`Workshop`, `Enrollment`, `ApplicationUser`), Enums y excepciones de negocio.
   - **Punto clave:** *El dominio es C# puro; no depende de Entity Framework ni de bases de datos. No es un modelo anémico: las entidades contienen sus propias reglas de negocio.*
2. **`SkillHub.Application` (Casos de Uso):**
   - Contiene los DTOs y los servicios (`WorkshopService`, `EnrollmentService`, `AuthService`, `ReportService`).
   - Define las abstracciones para librerías externas (`IPdfReportGenerator` y `IExcelReader`), cumpliendo el Principio de Inversión de Dependencias (DIP).
3. **`SkillHub.Infrastructure` (Detalles Técnicos):**
   - Implementa la persistencia con `ApplicationDbContext` (PostgreSQL), los Repositorios y `UnitOfWork`.
   - Implementa la generación de PDF con **QuestPDF** y lectura de Excel con **EPPlus**.
4. **Capas de Presentación:**
   - **`SkillHub.Api`:** Expone los endpoints REST para Swagger, Postman y operaciones de Empleados.
   - **`SkillHub.Web`:** Aplicación MVC para participantes (Opción A). Ambas capas consumen la misma lógica de `Application`.

---

## 3. 🛡️ Las Reglas de Negocio (Cómo explicarlas en el código)

| Regla de Negocio | Dónde está en el código | Qué responder |
|---|---|---|
| **1. Cupos Dinámicos** | `Workshop.cs` -> `GetAvailableCapacity()` | *El taller calcula sus cupos en vivo restando solo las inscripciones activas (`Capacity - ActiveEnrollmentsCount`). Las canceladas liberan el cupo automáticamente.* |
| **2. Sin Duplicados** | `EnrollmentService.cs` -> `EnrollAsync()` | *Consulto si el participante ya tiene una inscripción en estado `Active` para ese taller. Si existe, lanzo un `BusinessRuleException`.* |
| **3. Sin Cruces de Horario** | `Workshop.cs` -> `OverlapsWith()` | *Uso la fórmula matemática de intervalos: `StartDateTime < other.End && EndDateTime > other.Start`. Si se cruza con otro taller activo del usuario, se rechaza.* |
| **4. Solo Talleres Futuros** | `Workshop.cs` -> `Validate()` | *Tanto al crear el taller como al inscribirse, se valida que la fecha de inicio sea estrictamente mayor a `DateTime.UtcNow`.* |
| **5. Cancelación (24 Horas)** | `Enrollment.cs` -> `Cancel()` | *Si el usuario es participante, calculo `StartDateTime - now`. Si quedan menos de 24 horas, lanzo excepción. Si es empleado, puede cancelar en cualquier momento antes del inicio. Además, registro `CancelledById` y `CancelledAt`.* |
| **6. Reinscripción** | `EnrollmentService.cs` | *Como la inscripción previa quedó en estado `Cancelled`, ya no cuenta como activa, permitiéndole registrarse de nuevo si aún queda cupo disponible.* |

---

## 4. 💻 Guion para la Demostración en Vivo

### PARTE A: Demostración de la Web de Participantes (`SkillHub.Web`)
1. **Ejecutar la Web:**
   ```bash
   dotnet run --project src/SkillHub.Web
   ```
   Abre en el navegador: `http://localhost:5238`

2. **Catálogo Público (Sin iniciar sesión):**
   - Muestra las tarjetas de talleres, instructores, horarios y los cupos disponibles calculados en verde (ej. *15 cupos disponibles*).
   - Muestra el botón *Inicia sesión para inscribirte*.

3. **Iniciar Sesión:**
   - Clic en **Iniciar Sesión**.
   - Ingresa con:
     - **Email:** `jorge.perez@correo.com`
     - **Password:** `Skillhub2026*`

4. **Inscripción en Vivo:**
   - Elige un taller (ej. *FOT-001*) y haz clic en **"Inscribirme al Taller"**.
   - Verás el mensaje verde de confirmación y te redirige a **"Mis Inscripciones"**.

5. **Descargar Constancia PDF:**
   - En la fila del taller activo, haz clic en el botón verde **"Constancia PDF"**.
   - Muestra el PDF oficial generado con **QuestPDF** con membrete, datos, fecha de emisión y estado ACTIVA.

6. **Cancelar Inscripción:**
   - Clic en **"Cancelar"**.
   - Muestra el modal con la advertencia de las 24 horas y confirma.
   - El estado cambia a **Cancelada** y el cupo se libera de inmediato en el catálogo.

---

### PARTE B: Demostración de la REST API (Swagger)
1. **Ejecutar la API:**
   ```bash
   dotnet run --project src/SkillHub.Api
   ```
   Abre en el navegador: `http://localhost:5292/`

2. **Login de Empleado:**
   - En `POST /api/v1/auth/login`:
     ```json
     {
       "email": "andres.mejia@correo.com",
       "password": "Instructor2026*"
     }
     ```
   - Copia el token, ve a **Authorize**, ingresa `Bearer <token>` y dale Authorize.

3. **Reporte Top Talleres:**
   - Ejecuta `GET /api/v1/reports/top-workshops`.
   - Muestra cómo devuelve los talleres ordenados por inscritos activos en orden descendente.

4. **Carga Masiva desde Excel:**
   - En `POST /api/v1/enrollments/import`, sube `SkillHub_Data.xlsx`.
   - Muestra el resultado: `totalProcessed`, `enrolledCount` y reporte de filas rechazadas con motivo.

---

### PARTE C: Pruebas Unitarias
```bash
dotnet test
```
Muestra que las **11 pruebas unitarias** corren en milisegundos y pasan al 100% en verde.

---

## 5. ❓ Preguntas Típicas de los Evaluadores y Respuestas

- **P: ¿Por qué la lógica de cancelación está en la entidad Enrollment y no en el Controller?**
  - **R:** *Porque aplicamos Domain-Driven Design dentro de Clean Architecture. Las reglas de negocio deben residir en el Dominio para evitar modelos anémicos y garantizar que no puedan ser eludidas desde otras capas.*

- **P: ¿Dónde se implementó la generación de PDFs y por qué?**
  - **R:** *En la capa de `Infrastructure` usando QuestPDF, pero desacoplada detrás de la interfaz `IPdfReportGenerator` en `Application`, cumpliendo con el Principio de Inversión de Dependencias (DIP).*

- **P: ¿Cómo manejas los errores para que la API no se caiga?**
  - **R:** *Con un middleware global `GlobalExceptionHandlingMiddleware` que captura `NotFoundException` (404), `BusinessRuleException` (400) y devuelve un JSON estándar con el mensaje claro.*

- **P: ¿Cómo se cargaron los instructores y participantes iniciales?**
  - **R:** *Con el servicio `DataSeeder` en el arranque de la aplicación (`Program.cs`), que asegura que los roles `Empleado` y `Participante`, los 6 instructores y los talleres existan en PostgreSQL desde el inicio.*

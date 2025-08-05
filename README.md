# 🧪 Automated Tests – Eulalia Backend

This module contains the unit tests for the core business logic of the Eulalia digital voter ID system.

---

## 🇬🇧 English Version

### ✅ Structure

```
eulalia-backend.Tests/
├── Services/
│   ├── AfiliacionServiceTests.cs
│   ├── CiudadanoServiceTests.cs
│   ├── OrganizacionServiceTests.cs
│   ├── ParametrosSistemaServiceTests.cs
│   ├── AuthServiceTests.cs
│   └── BlockchainServiceTests.cs
```

### ✅ Technologies Used

- `xUnit` – Main testing framework
- `Moq` – For mocking dependencies
- `FluentAssertions` – For clean, expressive assertions
- `coverlet.collector` – For code coverage reports (optional)

### ✅ Execution

**From Visual Studio:** Open `Test Explorer`, then click "Run All Tests".  
**From terminal:**

```bash
dotnet test
```

Make sure the following projects are referenced in `eulalia-backend.Tests`:
- `eulalia-backend.Application`
- `eulalia-backend.Domain`
- `eulalia-backend.Shared`

### ✅ Scope of Tests

- Citizen registration and validation
- Political organization creation
- Political affiliation rules and status updates
- Authentication logic
- System parameter configuration
- Blockchain transaction data simulation

---

## 🇪🇸 Versión en Español

### ✅ Estructura

```
eulalia-backend.Tests/
├── Services/
│   ├── AfiliacionServiceTests.cs
│   ├── CiudadanoServiceTests.cs
│   ├── OrganizacionServiceTests.cs
│   ├── ParametrosSistemaServiceTests.cs
│   ├── AuthServiceTests.cs
│   └── BlockchainServiceTests.cs
```

### ✅ Tecnologías utilizadas

- `xUnit` – Framework principal de pruebas
- `Moq` – Para simular dependencias
- `FluentAssertions` – Para validaciones limpias y legibles
- `coverlet.collector` – Para reporte de cobertura (opcional)

### ✅ Ejecución

**Desde Visual Studio:** Abrir el `Test Explorer` y ejecutar "Run All Tests".  
**Desde consola:**

```bash
dotnet test
```

Asegúrate de tener referenciados los siguientes proyectos en `eulalia-backend.Tests`:
- `eulalia-backend.Application`
- `eulalia-backend.Domain`
- `eulalia-backend.Shared`

### ✅ Cobertura de las pruebas

- Registro y validación de ciudadanos
- Creación de organizaciones políticas
- Reglas de afiliación y cambios de estado
- Lógica de autenticación
- Configuración de parámetros del sistema
- Simulación de inserciones blockchain

---

**Author:** David Tacuri – 2025
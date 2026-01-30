# 🧪 Eulalia Backend - VoterID System

Este repositorio contiene el núcleo transaccional y la lógica de orquestación para el sistema de Identidad Digital Descentralizada (VoterID) **Eulalia**.

---

## 🏛️ Arquitectura (Clean Architecture)

El proyecto sigue los principios de **Arquitectura Limpia** para asegurar el desacoplamiento y la mantenibilidad:

- **Api**: Endpoints REST, autenticación JWT y configuración.
- **Application**: Casos de uso y lógica de negocio (Servicios y DTOs).
- **Domain**: Entidades de negocio, reglas core y enums.
- **Infrastructure**: Persistencia (PostgreSQL), clientes externos (SSI/Identus, Blockchain) y Biometría.

---

## 🚀 Milestone 3: Estado Actual

Se ha completado la **Fase 1** de la cimentación técnica:
- [x] Desacoplamiento de Controladores (Uso de Servicios y DTOs).
- [x] Implementación de la Capa de Aplicación.
- [x] Configuración de Entorno (.env y appsettings).
- [x] Base de Datos PostgreSQL operativa.

---

## 🛠️ Tecnologías

- **C# / .NET 8**
- **Entity Framework Core 9** (PostgreSQL)
- **JWT** para Autenticación.
- **Doxygen/Swagger** para documentación de API.
- **xUnit / Moq** para pruebas automatizadas.

---

## ⚙️ Configuración y Ejecución

1. **Requisitos**: .NET 8 SDK, Docker (para PostgreSQL).
2. **Base de Datos**: Configura el archivo `.env` en la raíz con tus credenciales.
3. **Ejecución**:
   ```bash
   dotnet restore
   dotnet run --project eulalia-backend.Api
   ```

---

## 🆔 Integración SSI (En Desarrollo)

Eulalia se integra con **Hyperledger Identus** para la gestión de Identidades Descentralizadas:
- Emisión de DIDs con anclaje en Cardano.
- Gestión de Credenciales Verificables (VCs).
- Flujos DIDComm para comunicación segura Holder-Issuer.

---
**David Tacuri** - 2026 | Proyecto Catalyst Fund 12
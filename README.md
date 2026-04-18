<div align="center">
  <a href="https://dotnet.microsoft.com/" target="_blank">
    <img src="https://raw.githubusercontent.com/dotnet/brand/main/logo/dotnet-logo.png" width="100" alt=".NET Logo" />
  </a>
  &nbsp;&nbsp;&nbsp;&nbsp;
  <a href="https://hyperledger-identus.github.io/docs/" target="_blank">    
    <img src="https://hyperledger-identus.github.io/docs/img/identus-navbar-light.png" width="220" alt="Identus Logo" />
  </a>
</div>
<div align="center"> 
  <a href="https://dotnet.microsoft.com/apps/aspnet" target="_blank">
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET Version">
  </a>
  <a href="https://www.postgresql.org/" target="_blank">
    <img src="https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white" alt="PostgreSQL">
  </a>
  <img src="https://img.shields.io/badge/Architecture-Clean-blue" alt="Clean Architecture">
</div>

# Eulalia Backend - VoterID
**STATUS**: ACTIVE (MVP COMPLETE - Webhook + SSI Flow + Credential Issuance)

---

### English Version

## Decentralized Identity & Voting Backend

This component is the core transactional engine of the **Eulalia VoterID System**, designed to integrate **Self-Sovereign Identity (SSI)** and **Blockchain** technology into electoral processes to enhance **security**, **privacy**, and **transparency**.

### Table of Contents
1. [Actors](#actors-en)
2. [Project Phases](#project-phases-en)
3. [Functional Aspects](#functional-aspects-en)
4. [Technical Aspects](#technical-aspects-en)
5. [Installation Procedure](#installation-procedure-en)
6. [API Endpoints](#api-endpoints-en)
7. [SSI Workflow](#ssi-workflow-en)
8. [Configuration](#configuration-en)
9. [Resources](#resources-en)

<a name="actors-en"></a>
### Actors
* **David Tacuri** (Lead Developer)

<a name="project-phases-en"></a>
### Project Phases
- [x] **Planning**: Architecture and SRS definition
- [x] **Implementation**: Core API, SSI integration, webhook, credential issuance
- [x] **Testing**: Full flow verification
- [ ] **Production**: Hardening and deployment

<a name="functional-aspects-en"></a>
### Functional Aspects

#### Core Features
1. **Citizen Registration**: Creates and manages citizen users with secure password hashing (PBKDF2).
2. **Authentication**: JWT-based authentication with role and cedula claims.
3. **SSI Integration**: Creates DIDComm invitations via Identus Cloud Agent.
4. **Webhook Handler**: Processes connection events from Identus and automatically issues credentials.
5. **Credential Management**: Tracks credential issuance status (`Requested` → `InvitationGenerated` → `CredentialIssued`).
6. **Biometric Enrollment**: Stores facial biometric embeddings securely.
7. **Political Affiliation**: Manages citizen-to-organization affiliations with validation gates.
8. **Status API**: Consolidated registration status (`base_ok`, `ssi_ok`, `bio_ok`, `ready_for_affiliation`).

#### Key Validation Gates
- **Afiliación (Affiliation)**: Requires `CredentialIssued` (not just `InvitationGenerated`) + biometric enrollment.
- **SSI Status**: Only allows `ready_for_affiliation = true` when all three components are complete.

<a name="technical-aspects-en"></a>
### Technical Aspects

#### Technological Platform
| Feature | Detail |
| :--- | :--- |
| Application Type | Clean Architecture Web API |
| Development Framework | .NET 8 - ASP.NET Core |
| Database Server | PostgreSQL |
| Identity Protocol | Hyperledger Identus (via Cloud Agent) |
| Programming Language | C# |
| Authentication | JWT (HS256) |
| Password Hashing | PBKDF2 (HMACSHA256, 100K iterations) |

#### Project Structure
```
eulalia-backend/
├── eulalia-backend.Api/         # Web API Controllers
├── eulalia-backend.Application/  # Services & Interfaces
├── eulalia-backend.Domain/     # Entities & Enums
├── eulalia-backend.Infrastructure/ # Data & Identus Client
└── eulalia-backend.sln
```

#### Prerequisites
- **.NET 8 SDK**
- **PostgreSQL Database** (port 5432)
- **Identus Cloud Agent** (accessible via port 8080)

#### Environment Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=voterid;Username=postgres;Password=Pg12345!"
  },
  "Jwt": {
    "Key": "EulaliaSecretKeyForDevelopmentOnly12345!",
    "Issuer": "eulalia-backend",
    "Audience": "eulalia-users"
  },
  "Identus": {
    "BaseUrl": "http://localhost:8080/cloud-agent",
    "ApiKey": "eulalia-local-key"
  }
}
```

<a name="installation-procedure-en"></a>
### Installation Procedure

1. **Clone the repository**:
   ```bash
   git clone https://github.com/democracyonchain/eulalia-backend.git
   ```

2. **Configure PostgreSQL**:
   - Ensure PostgreSQL is running on port 5432
   - Create database `voterid`
   - Update connection string in `appsettings.Development.json`

3. **Run Application**:
   ```bash
   dotnet restore
   dotnet run --project eulalia-backend.Api
   ```

4. **Verify**:
   - Swagger UI: http://localhost:5219/swagger/index.html

#### Running Tests
```bash
dotnet test
```

<a name="api-endpoints-en"></a>
### API Endpoints

| Method | Endpoint | Auth | Description |
| :--- | :--- | :--- | :--- |
| POST | `/api/Usuario/crear-usuario-ciudadano` | Anonymous | Create citizen user |
| POST | `/api/Auth/login` | Anonymous | Login and get JWT |
| GET | `/api/Usuario` | JWT | List all users |
| GET | `/api/Usuario/{id}` | JWT | Get user by ID |
| POST | `/api/SSI/invitation/{cedula}` | JWT | Create SSI invitation |
| GET | `/api/SSI/status/{cedula}` | JWT | Get SSI status |
| POST | `/api/SSI/webhook` | Anonymous | Handle Identus events |
| GET | `/api/RegistroCiudadano/estado/{cedula}` | JWT | Get registration status |
| POST | `/api/Biometria/register` | JWT | Register biometric |
| POST | `/api/Biometria/status/{cedula}` | JWT | Get biometric status |
| GET | `/api/Afiliacion` | JWT | List affiliations |
| POST | `/api/Afiliacion` | JWT | Create affiliation |
| PUT | `/api/Afiliacion/{id}/anular` | JWT | Cancel affiliation |

<a name="ssi-workflow-en"></a>
### SSI Workflow

```
1. Citizen registers → POST /api/Usuario/crear-usuario-ciudadano
2. User logs in → POST /api/Auth/login → gets JWT
3. Wallet integration step:
   a. App calls POST /api/SSI/invitation/{cedula}
   b. Backend calls Identus Cloud Agent → creates connection invitation
   c. Backend returns invitationUrl (shown as QR in app)
4. User scans QR with wallet (external or integrated)
5. Wallet accepts connection → Identus sends webhook event
6. Backend receives webhook → POST /api/SSI/webhook
7. Backend:
   a. Creates credential offer via Identus
   b. Updates status to "CredentialIssued"
   c. Stores HolderDID
8. Status check → GET /api/SSI/status/{cedula} returns "CredentialIssued"
9. Affiliation is now allowed
```

**Webhook Event Processing**:
- Event Type: `ConnectionEstablished`
- Extracts `connectionId` and `theirLabel` (format: `VoterID-{cedula}`)
- Updates SsiIssuance record with HolderDID and CredentialRecordId

<a name="configuration-en"></a>
### Configuration

#### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=voterid;Username=postgres;Password=Pg12345!"
  },
  "Jwt": {
    "Key": "EulaliaSecretKeyForDevelopmentOnly12345!",
    "Issuer": "eulalia-backend",
    "Audience": "eulalia-users",
    "ExpirationMinutes": 60
  },
  "Identus": {
    "BaseUrl": "http://localhost:8080/cloud-agent",
    "ApiKey": "eulalia-local-key",
    "TimeoutSeconds": 30
  },
  "Biometria": {
    "EncryptionKey": "EulaliaBiometriaKey32BytesValue!",
    "LivenessMinScore": 0.75,
    "MaxEmbeddingBase64Length": 16384
  }
}
```

#### Database Schema
Key tables:
- `usuario` - User accounts
- `ciudadano` - Citizen profiles
- `ssi_issuances` - SSI invitation and credential records
- `biometrias_ciudadano` - Biometric embeddings
- `afiliaciones` - Political affiliations
- `organizacion` - Political organizations

<a name="resources-en"></a>
### Resources
- [Official Identus Documentation](https://hyperledger-identus.github.io/docs/)
- [Hyperledger Foundation](https://www.hyperledger.org/)
- [.NET 8 Documentation](https://docs.microsoft.com/dotnet/)

---

### Spanish Version (ES)

## Backend de Identidad y Votación Descentralizada

Este componente es el motor transaccional del **Sistema VoterID Eulalia**, diseñado para integrar **Identidad Soberana (SSI)** y tecnología **Blockchain** en los procesos electorales.

### Tabla de Contenidos
1. [Actores](#actores-es)
2. [Fases del Proyecto](#fases-es)
3. [Aspectos Funcionales](#aspectos-funcionales-es)
4. [Aspectos Técnicos](#aspectos-tecnicos-es)
5. [Procedimiento de Instalación](#procedimiento-instalacion-es)
6. [Endpoints de API](#endpoints-es)
7. [Flujo SSI](#flujo-ssi-es)
8. [Configuración](#configuracion-es)
9. [Recursos](#recursos-es)

<a name="actores-es"></a>
### Actores
* **David Tacuri** (Desarrollador Principal)

<a name="project-phases-es"></a>
### Fases del Proyecto
- [x] **Planificación**: Definición de arquitectura y SRS
- [x] **Implementación**: API core, integración SSI, webhook, emisión de credenciales
- [x] **Pruebas**: Verificación de flujo completo
- [ ] **Producción**: Endurecimiento y despliegue

<a name="aspectos-funcionales-es"></a>
### Aspectos Funcionales

#### Características Principales
1. **Registro de Ciudadano**: Crea y gestiona usuarios con hash seguro (PBKDF2).
2. **Autenticación**: JWT con claims de rol y cedula.
3. **Integración SSI**: Crea invitaciones DIDComm via Identus Cloud Agent.
4. **Procesador de Webhook**: Procesa eventos de conexión y emite credenciales automáticamente.
5. **Gestión de Credenciales**: Rastrea estado de emisión (`Solicitada` → `InvitacionGenerada` → `CredencialEmitida`).
6. **Enrollment Biométrico**: Almacena embeddings faciales de forma segura.
7. **Afiliación Política**: Gestiona afiliaciones ciudadano-organización con validaciones.
8. **Estado Unificado**: Estado consolidado (`base_ok`, `ssi_ok`, `bio_ok`, `ready_for_affiliation`).

#### Validaciones Clave
- **Afiliación**: Requiere `CredentialIssued` (no solo `InvitacionGenerada`) + enrollment biométrico.
- **Estado SSI**: Solo permite `ready_for_affiliation = true` cuando los tres componentes están completos.

<a name="aspectos-tecnicos-es"></a>
### Aspectos Técnicos

#### Plataforma Tecnológica
| Característica | Detalle |
| :--- | :--- |
| Tipo de aplicación | Web API con Arquitectura Limpia |
| Framework de Desarrollo | .NET 8 - ASP.NET Core |
| Servidor de Base de Datos | PostgreSQL |
| Protocolo de Identidad | Hyperledger Identus |
| Lenguaje de programación | C# |
| Autenticación | JWT (HS256) |
| Hash de Contraseña | PBKDF2 (HMACSHA256, 100K iteraciones) |

#### Estructura del Proyecto
```
eulalia-backend/
├── eulalia-backend.Api/         # Controladores Web API
├── eulalia-backend.Application/  # Servicios e Interfaces
├── eulalia-backend.Domain/     # Entidades y Enums
├── eulalia-backend.Infrastructure/ # Datos y Cliente Identus
└── eulalia-backend.sln
```

#### Requisitos Previos
- **.NET 8 SDK**
- **PostgreSQL** (puerto 5432)
- **Identus Cloud Agent** (accesible via puerto 8080)

<a name="procedimiento-instalacion-es"></a>
### Procedimiento de Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/democracyonchain/eulalia-backend.git
   ```

2. **Configurar PostgreSQL**:
   - Asegurar que PostgreSQL está corriendo en puerto 5432
   - Crear base de datos `voterid`
   - Actualizar cadena de conexión en `appsettings.Development.json`

3. **Ejecutar Proyecto**:
   ```bash
   dotnet restore
   dotnet run --project eulalia-backend.Api
   ```

4. **Verificar**:
   - Swagger UI: http://localhost:5219/swagger/index.html

#### Ejecución de Pruebas
```bash
dotnet test
```

<a name="endpoints-es"></a>
### Endpoints de API

| Método | Endpoint | Auth | Descripción |
| :--- | :--- | :--- | :--- |
| POST | `/api/Usuario/crear-usuario-ciudadano` | Anónimo | Crear usuario ciudadano |
| POST | `/api/Auth/login` | Anónimo | Login y obtener JWT |
| GET | `/api/Usuario` | JWT | Listar usuarios |
| GET | `/api/Usuario/{id}` | JWT | Obtener usuario por ID |
| POST | `/api/SSI/invitation/{cedula}` | JWT | Crear invitación SSI |
| GET | `/api/SSI/status/{cedula}` | JWT | Obtener estado SSI |
| POST | `/api/SSI/webhook` | Anónimo | Manejar eventos Identus |
| GET | `/api/RegistroCiudadano/estado/{cedula}` | JWT | Obtener estado de registro |
| POST | `/api/Biometria/register` | JWT | Registrar biométricos |
| POST | `/api/Biometria/status/{cedula}` | JWT | Obtener estado biométrico |
| GET | `/api/Afiliacion` | JWT | Listar afiliaciones |
| POST | `/api/Afiliacion` | JWT | Crear afiliación |
| PUT | `/api/Afiliacion/{id}/anular` | JWT | Anular afiliación |

<a name="flujo-ssi-es"></a>
### Flujo SSI

```
1. Ciudadano se registra → POST /api/Usuario/crear-usuario-ciudadano
2. Usuario hace login → POST /api/Auth/login → obtiene JWT
3. Paso de integración Wallet:
   a. App llama POST /api/SSI/invitation/{cedula}
   b. Backend llama Identus Cloud Agent → crea invitación de conexión
   c. Backend retorna invitationUrl (mostrada como QR en app)
4. Usuario escanea QR con wallet (externa o integrada)
5. Wallet acepta conexión → Identus envía evento webhook
6. Backend recibe webhook → POST /api/SSI/webhook
7. Backend:
   a. Crea credential offer via Identus
   b. Actualiza estado a "CredencialEmitida"
   c. Almacena HolderDID
8. Consulta de estado → GET /api/SSI/status/{cedula} retorna "CredencialEmitida"
9. Afiliación ahora permitida
```

**Procesamiento de Eventos Webhook**:
- Tipo de Evento: `ConnectionEstablished`
- Extrae `connectionId` y `theirLabel` (formato: `VoterID-{cedula}`)
- Actualiza registro SsiIssuance con HolderDID y CredentialRecordId

<a name="configuracion-es"></a>
### Configuración

Ver configuración en inglés arriba.

<a name="recursos-es"></a>
### Recursos
- [Documentación Oficial de Identus](https://hyperledger-identus.github.io/docs/)
- [Hyperledger Foundation](https://www.hyperledger.org/)
- [.NET 8 Documentación](https://docs.microsoft.com/dotnet/)

---

## 🚀 Quick Start

```bash
# Clone
git clone https://github.com/democracyonchain/eulalia-backend.git
cd eulalia-backend

# Restore and run
dotnet restore
dotnet run --project eulalia-backend.Api

# Swagger UI: http://localhost:5219/swagger
```

## 📋 Servicios Requeridos

| Servicio | Puerto | Descripción |
| :--- | :--- | :--- |
| **Backend .NET** | 5219 | API REST de Eulalia |
| **Cloud Agent** | 8080 | Identus Cloud Agent |
| **PostgreSQL** | 5432 | Base de datos (voterid) |
| **PostgreSQL (Docker)** | 5433 | Base de datos Docker (Identus) |

### Iniciar Todos los Servicios

```bash
# 1. PostgreSQL local
# (configurar manualmente en puerto 5432)

# 2. Docker con Identus
cd eulalia-identus/cloud-agent/infrastructure/shared
docker compose --env-file .env up -d

# 3. Backend
cd eulalia-backend
dotnet run --project eulalia-backend.Api
```

---

## 🔗 Connections

- **Mobile App**: [eulalia-app](https://github.com/democracyonchain/eulalia-app)
- **Frontend Web**: [eulalia-frontend](https://github.com/democracyonchain/eulalia-frontend)
- **Identus Infrastructure**: [eulalia-identus](https://github.com/democracyonchain/eulalia-identus)

---

## 👤 Autor

**David Tacuri** – Lead Developer

---

**Project Catalyst Fund 12 | 2026**
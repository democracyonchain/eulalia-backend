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
**STATUS**: ACTIVE

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
6. [Resources](#resources-en)

<a name="actors-en"></a>
### Actors
* **David Tacuri**

<a name="project-phases-en"></a>
### Project Phases
- [x] Planning
- [/] Implementation
- [ ] Production

<a name="functional-aspects-en"></a>
### Functional Aspects
#### Objective
Back-end orchestrator for the decentralized voting and identity system.

#### Process Flow
1. **SSI Onboarding**: Generates DIDComm invitations using Identus for mobile wallets.
2. **Citizen Registration**: Securely verifies and stores identity and biometric metadata.
3. **Credential Issuance**: Issues Verifiable Credentials (VCs) to citizens.
4. **Blockchain Anchoring**: Publishes cryptographic hashes for immutable auditing.

<a name="technical-aspects-en"></a>
### Technical Aspects

#### Technological Platform
| Feature | Detail |
| :--- | :--- |
| Application Type | Clean Architecture Web API |
| Development Framework | .NET 8 - ASP.NET Core |
| Database Server | PostgreSQL |
| Identity Protocol | Hyperledger Identus |
| Programming Language | C# |

#### Prerequisites
- **.NET 8 SDK**
- **PostgreSQL Database**
- **Identus Cloud Agent** (Accessible via APISIX Gateway)

<a name="installation-procedure-en"></a>
### Installation Procedure

1. **Clone the repository**:
   ```bash
   git clone https://github.com/democracyonchain/eulalia-backend.git
   ```
2. **Configuration**:
   Copy `.env.example` to `.env` and update `appsettings.json` with your credentials.
3. **Run Application**:
   ```bash
   dotnet restore
   dotnet run --project eulalia-backend.Api
   ```

#### Running Tests
```bash
dotnet test
```

<a name="resources-en"></a>
### Resources
- [Official Identus Documentation](https://hyperledger-identus.github.io/docs/)
- [Hyperledger Foundation](https://www.hyperledger.org/)

---

### Spanish Version

## Backend de Identidad y Votación Descentralizada

Este componente es el motor transaccional del **Sistema VoterID Eulalia**, diseñado para integrar **Identidad Soberana (SSI)** y tecnología **Blockchain** en los procesos electorales para mejorar la **seguridad**, **privacidad** y **transparencia**.

### Tabla de Contenidos
1. [Actores](#actores-es)
2. [Fases del Proyecto](#fases-es)
3. [Aspectos Funcionales](#aspectos-funcionales-es)
4. [Aspectos Técnicos](#aspectos-tecnicos-es)
5. [Procedimiento de Instalación](#procedimiento-instalacion-es)
6. [Recursos](#recursos-es)

<a name="actores-es"></a>
### Actores
* **David Tacuri**

<a name="fases-es"></a>
### Fases del Proyecto
- [x] Planificación
- [/] Implementación
- [ ] Producción

<a name="aspectos-funcionales-es"></a>
### Aspectos Funcionales
#### Objetivo
Orquestador back-end para el sistema de identidad y conteo de votos en blockchain.

#### Flujo del Proceso
1. **Onboarding SSI**: Genera invitaciones DIDComm mediante Identus para wallets móviles.
2. **Registro de Ciudadano**: Verifica y almacena datos personales y biométricos de forma segura.
3. **Emisión de Credencial**: Emite Credenciales Verificables (VCs) a los ciudadanos.
4. **Anclaje en Blockchain**: Publica hashes criptográficos para auditoría inmutable.

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

#### Requisitos Previos
- **.NET 8 SDK**
- **Base de datos PostgreSQL**
- **Identus Cloud Agent** (Accesible mediante APISIX Gateway)

<a name="procedimiento-instalacion-es"></a>
### Procedimiento de Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/democracyonchain/eulalia-backend.git
   ```
2. **Configuración**:
   Configura tu cadena de conexión PostgreSQL y la URL del Gateway Identus en `appsettings.json`.
3. **Ejecutar el Proyecto**:
   ```bash
   dotnet restore
   dotnet run --project eulalia-backend.Api
   ```

#### Ejecución de Pruebas
```bash
dotnet test
```

<a name="recursos-es"></a>
### Recursos
- [Documentación Oficial de Identus](https://hyperledger-identus.github.io/docs/)
- [Hyperledger Foundation](https://www.hyperledger.org/)

---
**David Tacuri** | Project Catalyst Fund 12 | 2026